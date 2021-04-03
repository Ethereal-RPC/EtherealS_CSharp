using EtherealS.Model;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EtherealS.NativeNetwork
{

    public sealed class SocketListener
    { 

        private Socket listenSocket;

        private static Mutex mutex = new Mutex(false);

        private int numConnectedSockets;

        private SocketAsyncEventArgsPool readWritePool;

        private Semaphore semaphoreAcceptedClients;

        private AutoResetEvent keepalive = new AutoResetEvent(false);

        private ServerConfig config;

        private Tuple<string, string> serverKey;

        public Socket ListenSocket { get=>listenSocket; set => listenSocket = value; }

        public SocketListener(Tuple<string, string> serverKey, ServerConfig config)
        {
            this.serverKey = serverKey;
            this.config = config;
            this.numConnectedSockets = 0;
            this.readWritePool = new SocketAsyncEventArgsPool(config.NumConnections);
            this.semaphoreAcceptedClients = new Semaphore(config.NumConnections, config.NumConnections);
            for (int i = 0; i < config.NumConnections; i++)
            {
                SocketAsyncEventArgs receiveEventArg = new SocketAsyncEventArgs();
                receiveEventArg.Completed += OnReceiveCompleted;
                receiveEventArg.SetBuffer(new Byte[config.BufferSize], 0, config.BufferSize);
                DataToken token = new DataToken(receiveEventArg, serverKey, config);
                receiveEventArg.UserToken = token;
                this.readWritePool.Push(receiveEventArg);
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

            if(NetCore.Get(serverKey,out NetConfig netConfig))
            {
                netConfig.ClientResponseSend = SendClientResponse;
                netConfig.ServerRequestSend = SendServerRequest;
            }
        }

        public void Start()
        {
            for (int i = 0; i < config.NumChannels; i++)
            {
                Thread thread = new Thread(() => StartAccept(null));
                thread.Name = i.ToString();
                thread.Start();
            }
        }
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            try
            {
                e.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                
            }
            e.AcceptSocket.Close();
            e.AcceptSocket.Dispose();
            e.AcceptSocket = null;
            this.semaphoreAcceptedClients.Release();
            if (config.AutoManageTokens)
            {
                if (NetCore.Get(serverKey, out NetConfig netConfig))
                {
                    netConfig.Tokens.TryRemove((e.UserToken as DataToken).Token.Key, out BaseUserToken value);
                }
                else throw new RPCException(RPCException.ErrorCode.NotFoundNetConfig, "未找到NetConfig");
            }
            (e.UserToken as DataToken).DisConnect();
            this.readWritePool.Push(e);
            Interlocked.Decrement(ref this.numConnectedSockets);
            Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", this.numConnectedSockets);
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
                    SocketAsyncEventArgs readEventArgs = this.readWritePool.Pop();
                    if (readEventArgs != null)
                    {
                        // Get the socket for the accepted client connection and put it into the 
                        // ReadEventArg object User user.
                        readEventArgs.AcceptSocket = s;
                        (readEventArgs.UserToken as DataToken).Connect(config.CreateMethod.Invoke(), s);
                        Interlocked.Increment(ref this.numConnectedSockets);
                        Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                            this.numConnectedSockets);
                        if (!s.ReceiveAsync(readEventArgs))
                        {
                            ProcessReceive(readEventArgs);
                        }
                    }
                    else
                    {
                        Console.WriteLine("There are no more available sockets to allocate.");
                    }
                }
                else throw new SocketException((int)SocketError.NotConnected);
            }
            catch (SocketException ex)
            {
                DataToken token = e.UserToken as DataToken;
                Console.WriteLine("Error when processing data received from {0}:\r\n{1}", e.RemoteEndPoint, ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                keepalive.Set();
            }
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            Console.WriteLine($"[线程]{Thread.CurrentThread.Name}:{serverKey.Item1}:{serverKey.Item2}线程任务已经开始运行");
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
                Console.WriteLine($"[线程]{Thread.CurrentThread.Name}:开始异步等待{serverKey.Item1}:{serverKey.Item2}中Accpet请求");
                if (!this.listenSocket.AcceptAsync(acceptEventArg))
                {
                    this.ProcessAccept(acceptEventArg);
                }
                else
                {
                    keepalive.Reset();
                    keepalive.WaitOne();
                }
                Console.WriteLine($"[线程]{Thread.CurrentThread.Name}:完成{serverKey.Item1}:{serverKey.Item2}中请求的Accpet");
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

                    (e.UserToken as DataToken).ProcessData();
                    if (!e.AcceptSocket.ReceiveAsync(e))
                    {
                        // Read the next block of data sent by client.
                        this.ProcessReceive(e);
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
        public void SendClientResponse(BaseUserToken token,ClientResponseModel response)
        {
            if (token!=null && token.Net != null && (token.Net as Socket).Connected)
            {
#if DEBUG
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"{DateTime.Now}::{serverKey.Item1}:{serverKey.Item2}::[服-返回]\n{response}");
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
                //拷贝到同一个byte[]数组中
                Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerBytes.Length);
                Buffer.BlockCopy(pattern, 0, sendBuffer, headerBytes.Length, pattern.Length);
                Buffer.BlockCopy(future, 0, sendBuffer, headerBytes.Length + pattern.Length, future.Length);
                Buffer.BlockCopy(bodyBytes, 0, sendBuffer, headerBytes.Length + pattern.Length + future.Length, bodyBytes.Length);
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                (token.Net as Socket).SendAsync(sendEventArgs);
            }
        }
        internal void SendServerRequest(BaseUserToken token,ServerRequestModel request)
        {
            if (token != null && token.Net != null && (token.Net as Socket).Connected)
            {
#if DEBUG
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"{DateTime.Now}::{serverKey.Item1}:{serverKey.Item2}::[服-指令]\n{request}");
                Console.WriteLine("---------------------------------------------------------");
#endif
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
        #region IDisposable Members
        bool isDipose = false;

        ~SocketListener()
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
#if DEBUG
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"{DateTime.Now}::{token.ServerKey.Item1}:{token.ServerKey.Item2}::[服-指令]\n{request}");
                Console.WriteLine("---------------------------------------------------------");
#endif
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
                Console.WriteLine($"{DateTime.Now}::{token.ServerKey.Item1}:{token.ServerKey.Item2}::[服-返回]\n{response}");
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
