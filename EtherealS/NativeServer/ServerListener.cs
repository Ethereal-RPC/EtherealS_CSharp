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

        public ServerListener(Net net,Tuple<string, string> serverKey, ServerConfig config)
        {
            netName = net.Name;
            this.serverKey = serverKey;
            this.config = config;
            this.numConnectedSockets = 0;
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

        public void Start()
        {
            for (int i = 0; i < config.NumChannels; i++)
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        StartAccept(null);
                    }
                    catch(Exception e)
                    {
                        config.OnException(e,this);
                    }
                });
                thread.Name = i.ToString();
                thread.Start();
            }
        }

        public bool CloseClientSocket(SocketAsyncEventArgs e)
        {
            if(e.AcceptSocket != null && e.AcceptSocket.Connected)
            {
                try
                {
                    e.AcceptSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                {

                }
                if (config.AutoManageTokens)
                {
                    if (NetCore.Get(netName, out Net net))
                    {
                        if ((e.UserToken as DataToken).Token != null) net.Tokens.TryRemove((e.UserToken as DataToken).Token.Key, out BaseUserToken value);
                    }
                    else config.OnException(new RPCException(RPCException.ErrorCode.Runtime, "未找到NetConfig"),this);
                }
                (e.UserToken as DataToken).DisConnect();
                e.AcceptSocket.Close();
                e.AcceptSocket.Dispose();
                e.AcceptSocket = null;
                this.semaphoreAcceptedClients.Release();
                this.readWritePool.Push(e.UserToken as DataToken);
                Interlocked.Decrement(ref this.numConnectedSockets);
                config.OnLog(RPCLog.LogCode.Runtime,$"A client has been disconnected from the server. There are {this.numConnectedSockets} clients connected to the server",this);
            }
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
                        Interlocked.Increment(ref this.numConnectedSockets);
                        config.OnLog(RPCLog.LogCode.Runtime, $"Client connection accepted. There are {this.numConnectedSockets} clients connected to the server",this);
                        if (!s.ReceiveAsync(token.EventArgs))
                        {
                            ProcessReceive(token.EventArgs);
                        }
                    }
                    else
                    {
                        config.OnException(RPCException.ErrorCode.Runtime, "There are no more available sockets to allocate.",this);
                    }
                }
                else throw new SocketException((int)SocketError.NotConnected);
            }
            catch (SocketException ex)
            {
                config.OnException(RPCException.ErrorCode.Runtime, $"Error when processing data received from {e.RemoteEndPoint}:\r\n{ex.ToString()}",this);
            }
            catch (Exception ex)
            {
                config.OnException(RPCException.ErrorCode.Runtime, ex.ToString(),this);
            }
            finally
            {
                keepalive.Set();
            }
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            config.OnLog(RPCLog.LogCode.Runtime, $"[线程]{Thread.CurrentThread.Name}:{serverKey.Item1}:{serverKey.Item2}线程任务已经开始运行", this);
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
                config.OnLog(RPCLog.LogCode.Runtime, $"[线程]{Thread.CurrentThread.Name}:开始异步等待{serverKey.Item1}:{serverKey.Item2}中Accpet请求",this);
                if (!this.listenSocket.AcceptAsync(acceptEventArg))
                {
                    this.ProcessAccept(acceptEventArg);
                }
                else
                {
                    keepalive.Reset();
                    keepalive.WaitOne();
                }
                config.OnLog(RPCLog.LogCode.Runtime, $"[线程]{Thread.CurrentThread.Name}:完成{serverKey.Item1}:{serverKey.Item2}中请求的Accpet",this);
                mutex.ReleaseMutex();
            }
        }
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // Check if the remote host closed the connection.
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    try
                    {
                        (e.UserToken as DataToken).ProcessData();
                        if (!e.AcceptSocket.ReceiveAsync(e))
                        {
                            // Read the next block of data sent by client.
                            this.ProcessReceive(e);
                        }
                    }
                    catch
                    {
                        CloseClientSocket(e);
                    }
                }
                else
                {
                    CloseClientSocket(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessReceive(e);
        }
        public void Stop()
        {
            this.listenSocket.Close();
            mutex.ReleaseMutex();
        }
        private void SendClientResponse(BaseUserToken token,ClientResponseModel response)
        {
            if (token!=null && token.Net != null && (token.Net as SocketAsyncEventArgs).AcceptSocket.Connected)
            {
                string log = "--------------------------------------------------\n" +
                            $"{DateTime.Now}::{serverKey.Item1}:{serverKey.Item2}::[服-返回]\n{response}" +
                            "--------------------------------------------------\n";
                config.OnLog(RPCLog.LogCode.Runtime, log,this);
                //构造data数据
                byte[] bodyBytes = config.Encoding.GetBytes(config.ClientResponseModelSerialize(response));
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
                (token.Net as SocketAsyncEventArgs).AcceptSocket.SendAsync(sendEventArgs);
            }
            else
            {
                config.OnException(new SocketException((Int32)SocketError.NotConnected),this);
            }
        }
        private void SendServerRequest(BaseUserToken token,ServerRequestModel request)
        {
            if (token != null && token.Net != null && (token.Net as SocketAsyncEventArgs).AcceptSocket.Connected)
            {
                string log = "--------------------------------------------------\n" +
                            $"{DateTime.Now}::{serverKey.Item1}:{serverKey.Item2}::[服-指令]\n{request}\n" +
                            "--------------------------------------------------\n";
                config.OnLog(RPCLog.LogCode.Runtime, log,this);
                //构造data数据
                byte[] bodyBytes = config.Encoding.GetBytes(config.ServerRequestModelSerialize(request));
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
                (token.Net as SocketAsyncEventArgs).AcceptSocket.SendAsync(sendEventArgs);
            }
            else
            {
                config.OnException(new SocketException((Int32)SocketError.NotConnected), this);
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
            Console.WriteLine($"{Thread.CurrentThread.Name}开始销毁{serverKey.Item1}:{serverKey.Item2}实例");
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
            if ((token.Net as Socket).Connected)
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
            if ((token.Net as Socket).Connected)
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
