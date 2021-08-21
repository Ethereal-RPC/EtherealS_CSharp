using EtherealS.Model;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EtherealS.NativeServer
{

    public sealed class ServerListener
    { 

        private Socket listenSocket;

        private static Mutex mutex = new Mutex(false);

        private int numConnectedSockets;

        private DataTokenPool readWritePool;

        private Semaphore semaphoreAcceptedClients;

        private AutoResetEvent keepalive = new AutoResetEvent(false);

        private ServerConfig config;

        private Tuple<string, string> serverKey;

        private string netName;

        public Socket ListenSocket { get=>listenSocket; set => listenSocket = value; }
        public Tuple<string, string> ServerKey { get => serverKey; set => serverKey = value; }
        public int NumConnectedSockets { get => numConnectedSockets; set => numConnectedSockets = value; }
        public ServerConfig Config { get => config; set => config = value; }

        #region --委托--

        public delegate void OnExceptionDelegate(Exception exception, ServerListener server);

        public delegate void OnLogDelegate(RPCLog log, ServerListener server);

        #endregion

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
        /// <summary>
        /// 日志输出事件
        /// </summary>
        public event OnLogDelegate LogEvent
        {
            add
            {
                logEvent -= value;
                logEvent += value;
            }
            remove
            {
                logEvent -= value;
            }
        }
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent
        {
            add
            {
                exceptionEvent -= value;
                exceptionEvent += value;
            }
            remove
            {
                exceptionEvent -= value;
            }

        }
        #endregion
        public ServerListener(Net net,Tuple<string, string> serverKey, ServerConfig config)
        {
            netName = net.Name;
            this.ServerKey = serverKey;
            this.Config = config;
            this.NumConnectedSockets = 0;
            this.readWritePool = new DataTokenPool(config.NumConnections);
            this.semaphoreAcceptedClients = new Semaphore(config.NumConnections, config.NumConnections);
            for (int i = 0; i < config.NumConnections; i++)
            {
                DataToken token = new DataToken(netName,serverKey,config);
                token.EventArgs.Completed += OnReceiveCompleted;
                this.readWritePool.Push(token);
            }

            IPAddress[] addressList = Dns.GetHostEntry(serverKey.Item1).AddressList;

            IPEndPoint localEndPoint = new IPEndPoint(addressList[addressList.Length - 1],int.Parse(serverKey.Item2));

            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                this.listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
            }
            else
            {

                this.listenSocket.Bind(localEndPoint);
            }
            this.listenSocket.Listen(config.NumConnections);
            net.ClientResponseSend = SendClientResponse;
            net.ServerRequestSend = SendServerRequest;
        }

        internal void Start()
        {
            for (int i = 0; i < Config.NumChannels; i++)
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        StartAccept(null);
                    }
                    catch(Exception e)
                    {
                        OnException(e);
                    }
                });
                thread.Name = i.ToString();
                thread.Start();
            }
        }

        internal bool CloseClientSocket(SocketAsyncEventArgs e)
        {
            try
            {
                e.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {

            }
            if (Config.AutoManageTokens)
            {
                if (NetCore.Get(netName, out Net net))
                {
                    if ((e.UserToken as DataToken).Token != null) net.Tokens.TryRemove((e.UserToken as DataToken).Token.Key, out BaseUserToken value);
                }
                else OnException(new RPCException(RPCException.ErrorCode.Runtime, "未找到NetConfig"));
            }
            (e.UserToken as DataToken).DisConnect();
            this.semaphoreAcceptedClients.Release();
            this.readWritePool.Push(e.UserToken as DataToken);
            Interlocked.Decrement(ref numConnectedSockets);
            OnLog(RPCLog.LogCode.Runtime, $"A client has been disconnected from the server. There are {this.NumConnectedSockets} clients connected to the server");
            return true;
        }
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.AcceptSocket == null) throw new SocketException((int)SocketError.SocketError);
            Socket s = e.AcceptSocket;
            try
            {
                if (s.Connected)
                {
                    DataToken token = this.readWritePool.Pop();
                    if (token != null)
                    {
                        // Get the socket for the accepted client connection and put it into the 
                        // ReadEventArg object User user.
                        token.Connect(s);
                        Interlocked.Increment(ref numConnectedSockets);
                        OnLog(RPCLog.LogCode.Runtime, $"Client connection accepted. There are {this.NumConnectedSockets} clients connected to the server");
                        if (!s.ReceiveAsync(token.EventArgs))
                        {
                            ProcessReceive(token.EventArgs);
                        }
                    }
                    else
                    {
                        OnException(RPCException.ErrorCode.Runtime, "There are no more available sockets to allocate.");
                    }
                }
                else throw new SocketException((int)SocketError.NotConnected);
            }
            catch (SocketException ex)
            {
                OnException(RPCException.ErrorCode.Runtime, $"Error when processing data received from {e.RemoteEndPoint}:\r\n{ex.ToString()}");
            }
            catch (Exception ex)
            {
                OnException(RPCException.ErrorCode.Runtime, ex.ToString());
            }
            finally
            {
                keepalive.Set();
            }
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            OnLog(RPCLog.LogCode.Runtime, $"[线程]{Thread.CurrentThread.Name}:{ServerKey.Item1}:{ServerKey.Item2}线程任务已经开始运行");
            while (true)
            {
                mutex.WaitOne();
                if (acceptEventArg == null)
                {
                    acceptEventArg = new SocketAsyncEventArgs();
                    acceptEventArg.Completed += OnAcceptCompleted;
                }
                else
                {
                    // Socket must be cleared since the context object is being reused.
                    acceptEventArg.AcceptSocket = null;
                }
                this.semaphoreAcceptedClients.WaitOne();
                OnLog(RPCLog.LogCode.Runtime, $"[线程]{Thread.CurrentThread.Name}:开始异步等待{ServerKey.Item1}:{ServerKey.Item2}中Accpet请求");
                if (!this.listenSocket.AcceptAsync(acceptEventArg))
                {
                    this.ProcessAccept(acceptEventArg);
                }
                else
                {
                    keepalive.Reset();
                    keepalive.WaitOne();
                }
                OnLog(RPCLog.LogCode.Runtime, $"[线程]{Thread.CurrentThread.Name}:完成{ServerKey.Item1}:{ServerKey.Item2}中请求的Accpet");
                mutex.ReleaseMutex();
            }
        }
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                // Check if the remote host closed the connection.
                if (e.BytesTransferred > 0)
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        (e.UserToken as DataToken).ProcessData();
                        if (!e.AcceptSocket.ReceiveAsync(e))
                        {
                            // Read the next block of data sent by client.
                            this.ProcessReceive(e);
                        }
                    }
                    else
                    {
                        throw new RPCException("SocketError不为Success");
                    }
                }
                else
                {
                    throw new RPCException("字节交换为0，可能已经断开");
                }
            }
            catch(Exception exception)
            {
                CloseClientSocket(e);
                OnException(exception,false);
            }
        }
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessReceive(e);
        }
        internal void Stop()
        {
            this.listenSocket.Close();
            mutex.ReleaseMutex();
        }
        private void SendClientResponse(BaseUserToken token,ClientResponseModel response)
        {
            if ((token?.Net as DataToken).EventArgs.AcceptSocket.Connected)
            {
                string log = "--------------------------------------------------\n" +
                            $"{DateTime.Now}::{ServerKey.Item1}:{ServerKey.Item2}::[服-返回]\n{response}" +
                            "--------------------------------------------------\n";
                OnLog(RPCLog.LogCode.Runtime, log);
                //构造data数据
                byte[] bodyBytes = Config.Encoding.GetBytes(Config.ClientResponseModelSerialize(response));
                //构造表头数据，固定4个字节的长度，表示内容的长度
                byte[] headerBytes = BitConverter.GetBytes(bodyBytes.Length);
                byte[] pattern = { 1 };
                //预备未来的一些数据
                byte[] future = new byte[27];
                //总计需要
                byte[] sendBuffer = new byte[32 + bodyBytes.Length];
                //拷贝到同一个byte[]数组中
                Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerBytes.Length);
                Buffer.BlockCopy(pattern, 0, sendBuffer, headerBytes.Length, pattern.Length);
                Buffer.BlockCopy(future, 0, sendBuffer, headerBytes.Length + pattern.Length, future.Length);
                Buffer.BlockCopy(bodyBytes, 0, sendBuffer, headerBytes.Length + pattern.Length + future.Length, bodyBytes.Length);
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                (token.Net as DataToken).EventArgs.AcceptSocket.SendAsync(sendEventArgs);
            }
            else
            {
                OnException(new SocketException((Int32)SocketError.NotConnected));
            }
        }
        private void SendServerRequest(BaseUserToken token,ServerRequestModel request)
        {
            if ((token?.Net as DataToken).EventArgs.AcceptSocket.Connected)
            {
                string log = "--------------------------------------------------\n" +
                            $"{DateTime.Now}::{ServerKey.Item1}:{ServerKey.Item2}::[服-指令]\n{request}\n" +
                            "--------------------------------------------------\n";
                OnLog(RPCLog.LogCode.Runtime, log);
                //构造data数据
                byte[] bodyBytes = Config.Encoding.GetBytes(Config.ServerRequestModelSerialize(request));
                //构造表头数据，固定4个字节的长度，表示内容的长度
                byte[] headerBytes = BitConverter.GetBytes(bodyBytes.Length);
                byte[] pattern = { 0 };
                //预备未来的一些数据
                byte[] future = new byte[27];
                //总计需要
                byte[] sendBuffer = new byte[32 + bodyBytes.Length];
                ///拷贝到同一个byte[]数组中
                Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerBytes.Length);
                Buffer.BlockCopy(pattern, 0, sendBuffer, headerBytes.Length, pattern.Length);
                Buffer.BlockCopy(future, 0, sendBuffer, headerBytes.Length + pattern.Length, future.Length);
                Buffer.BlockCopy(bodyBytes, 0, sendBuffer, headerBytes.Length + pattern.Length + future.Length, bodyBytes.Length);
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                (token.Net as DataToken).EventArgs.AcceptSocket.SendAsync(sendEventArgs);
            }
            else
            {
                OnException(new SocketException((Int32)SocketError.NotConnected));
            }
        }

        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e,bool isThrow = true)
        {
            if (exceptionEvent != null)
            {
                exceptionEvent.Invoke(e,this);
            }
            if(isThrow)throw e;
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        public void OnLog(RPCLog log)
        {
            if (logEvent != null)
            {
                logEvent.Invoke(log, this);
            }
        }
        #region IDisposable Members
        bool isDipose = false;

        ~ServerListener()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (isDipose) return;
            Console.WriteLine($"{Thread.CurrentThread.Name}开始销毁{ServerKey.Item1}:{ServerKey.Item2}实例");
            if (disposing)
            {
                semaphoreAcceptedClients = null;
                readWritePool = null;
            }
            //处理非托管资源
            try
            {
                listenSocket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
                // Throw if client has closed, so it is not necessary to catch.
            }
            finally
            {
                listenSocket.Close();
                listenSocket = null;
            }
            isDipose = true;
        }
        public static void ServerRequestSend(BaseUserToken token, ServerRequestModel request)
        {
            if ((token.Net as Socket)?.Connected == true)
            {   
                //构造data数据
                byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                //构造表头数据，固定4个字节的长度，表示内容的长度
                byte[] headerBytes = BitConverter.GetBytes(bodyBytes.Length);
                //构造消息类型 1 为Respond,0 为Request
                byte[] pattern = { 0 };
                //预备未来的一些数据
                byte[] future = new byte[27];
                //总计需要
                byte[] sendBuffer = new byte[headerBytes.Length + pattern.Length + future.Length + bodyBytes.Length];
                ///拷贝到同一个byte[]数组中
                Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerBytes.Length);
                Buffer.BlockCopy(pattern, 0, sendBuffer, headerBytes.Length, pattern.Length);
                Buffer.BlockCopy(future, 0, sendBuffer, headerBytes.Length + pattern.Length, future.Length);
                Buffer.BlockCopy(bodyBytes, 0, sendBuffer, headerBytes.Length + pattern.Length + future.Length, bodyBytes.Length);
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                (token.Net as Socket).SendAsync(sendEventArgs);
            }
            else
            {
                throw new SocketException((Int32)SocketError.NotConnected);
            }
        }
        public static void ClientResponseSend(BaseUserToken token, ClientResponseModel response)
        {
            if ((token.Net as Socket)?.Connected == true)
            {
#if DEBUG
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"{DateTime.Now}::{token.NetName}::[服-返回]\n{response}");
                Console.WriteLine("---------------------------------------------------------");
#endif
                //构造data数据
                byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                //构造表头数据，固定4个字节的长度，表示内容的长度
                byte[] headerBytes = BitConverter.GetBytes(bodyBytes.Length);
                //构造消息类型 1 为Respond,0 为Request
                byte[] pattern = { 1 };
                //预备未来的一些数据
                byte[] future = new byte[27];
                //总计需要
                byte[] sendBuffer = new byte[headerBytes.Length + pattern.Length + future.Length + bodyBytes.Length];
                ///拷贝到同一个byte[]数组中
                Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerBytes.Length);
                Buffer.BlockCopy(pattern, 0, sendBuffer, headerBytes.Length, pattern.Length);
                Buffer.BlockCopy(future, 0, sendBuffer, headerBytes.Length + pattern.Length, future.Length);
                Buffer.BlockCopy(bodyBytes, 0, sendBuffer, headerBytes.Length + pattern.Length + future.Length, bodyBytes.Length);
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                (token.Net as Socket).SendAsync(sendEventArgs);
            }
        }
        #endregion
    }
}
