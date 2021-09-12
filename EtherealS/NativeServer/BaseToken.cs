using EtherealS.Model;
using EtherealS.RPCNet;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;

namespace EtherealS.NativeServer
{
    public class BaseToken
    {

        #region --委托--

        internal delegate void OnExceptionDelegate(Exception exception, BaseToken token);

        internal delegate void OnLogDelegate(RPCLog log, BaseToken toke);

        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void ConnectDelegate(BaseToken token);
        /// <summary>
        ///     
        /// </summary>
        /// <param name="token"></param>
        public delegate void DisConnectDelegate(BaseToken token);

        #endregion

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectDelegate ConnectEvent;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event DisConnectDelegate DisConnectEvent;
        #endregion

        #region --事件属性--
        /// <summary>
        /// 日志输出事件
        /// </summary>
        internal event OnLogDelegate LogEvent
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
        internal event OnExceptionDelegate ExceptionEvent
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

        #region --字段--
        private string netName;
        private ServerConfig config;
        private object key;
        private bool isWebSocket;
        HttpListenerWebSocketContext context;
        CancellationToken cancellationToken;

        #endregion

        #region --属性--
        public object Key { get => key; set => key = value; }
        public bool IsWebSocket { get => isWebSocket; set => isWebSocket = value; }
        #endregion

        #region --方法--
        /// <summary>
        /// 注册Token信息至Tokens表
        /// </summary>
        /// <param name="replace">当已存在Token信息，是否替换</param>
        /// <returns></returns>
        public bool Register(bool replace = false)
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
            }
            if (replace)
            {
                net.Tokens.TryRemove(Key, out BaseToken token);
            }
            return net.Tokens.TryAdd(Key, this);
        }
        /// <summary>
        /// 从Tokens表中注销Token信息
        /// </summary>
        /// <returns></returns>
        public bool UnRegister()
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
            }
            return net.Tokens.TryRemove(Key, out BaseToken value);
        }
        /// <summary>
        /// 得到该Token所属的Tokens表单
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<object, BaseToken> GetTokens()
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
            }
            return net.Tokens;
        }
        /// <summary>
        /// 得到特定的Token信息
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="key">Token唯一凭据Key</param>
        /// <param name="value">返回的值</param>
        /// <returns></returns>
        public bool GetToken<T>(object key, out T value) where T : BaseToken
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
            }
            if (net.Tokens.TryGetValue(key, out BaseToken result))
            {
                value = (T)result;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
        /// <summary>
        /// 得到某网络层{ip}-{port}中的Tokens表单
        /// </summary>
        /// <param name="serverkey"></param>
        /// <returns></returns>
        public static ConcurrentDictionary<object, BaseToken> GetTokens(string netName)
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
            }
            return net.Tokens;
        }
        #endregion

        #region --网络方法--
        public BaseToken()
        {

        }
        internal void Init(string netName, ServerConfig config,CancellationToken cancellationToken)
        {
            this.netName = netName;
            this.config = config;
            this.cancellationToken = cancellationToken;
        }
        internal void Connect(HttpListenerWebSocketContext context)
        {
            this.context = context;
            OnConnect();
            ProcessData();
        }
        internal async void ProcessData()
        {
            WebSocket webSocket = context.WebSocket;
            byte[] receiveBuffer = null;
            int offset = 0;
            int free = config.BufferSize;
            // While the WebSocket connection remains open run a simple loop that receives data and sends it back.
            while (webSocket.State == WebSocketState.Open)
            {
                if (receiveBuffer == null)
                {
                    receiveBuffer = new byte[config.BufferSize];
                }

                try
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, offset, free), cancellationToken);
                    offset += receiveResult.Count;
                    free -= receiveResult.Count;
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        OnDisConnect();
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                        continue;
                    }

                    if (receiveResult.EndOfMessage)
                    {
                        string data = config.Encoding.GetString(receiveBuffer);
                        offset = 0;
                        free = config.BufferSize;
                        //客户端发来的一定是请求
                        ClientRequestModel request = null;
                        request = config.ClientRequestModelDeserialize(config.Encoding.GetString(receiveBuffer));
                        if (!NetCore.Get(netName, out Net net))
                        {
                            SendClientResponse(new ClientResponseModel( null, null, request.Id, request.Service, new Error(Error.ErrorCode.NotFoundNet, $"Token查询{netName} Net时 不存在", null)));
                            return;
                        }
                        SendClientResponse(net.ClientRequestReceiveProcess(this, request));
                    }
                    else if (free == 0)
                    {
                        var newSize = receiveBuffer.Length + config.BufferSize;
                        if (newSize > config.MaxBufferSize)
                        {
                            SendClientResponse(new ClientResponseModel(null, null, null, null, new Error(Error.ErrorCode.NotFoundNet, $"缓冲区:{newSize}-超过最大字节数:{config.MaxBufferSize}，已断开连接！", null)));
                            DisConnect(WebSocketCloseStatus.MessageTooBig, $"缓冲区:{newSize}-超过最大字节数:{config.MaxBufferSize}，已断开连接！");
                            return;
                        }
                        byte[] new_bytes = new byte[newSize];
                        Array.Copy(receiveBuffer, 0, new_bytes, 0, offset);
                        receiveBuffer = new_bytes;
                        free = receiveBuffer.Length - offset;
                        continue;
                    }
                }
                catch(Exception e)
                {
                    SendClientResponse(new ClientResponseModel( null, null, null, null, new Error(Error.ErrorCode.Common, $"{e.Message}", null)));
                }
            }
        }

        public void DisConnect(WebSocketCloseStatus status,string reason)
        {
            if(config.AutoManageTokens)UnRegister();
            context.WebSocket.CloseAsync(status, reason, cancellationToken);
            OnDisConnect();
        }

        internal void SendClientResponse(ClientResponseModel response)
        {
            try
            {
                if (context.WebSocket.State == WebSocketState.Open && isWebSocket)
                {
                    string log = "--------------------------------------------------\n" +
                                $"{DateTime.Now}::{netName}::[服-返回]\n{response}" +
                                "--------------------------------------------------\n";
                    OnLog(RPCLog.LogCode.Runtime, log);
                    context.WebSocket.SendAsync(config.Encoding.GetBytes(config.ClientResponseModelSerialize(response)), WebSocketMessageType.Text, true, cancellationToken);
                }
            }
            catch
            {

            }
        }
        internal void SendServerRequest(ServerRequestModel request )
        {
            try
            {
                if (context.WebSocket.State == WebSocketState.Open && isWebSocket)
                {
                    string log = "--------------------------------------------------\n" +
                                $"{DateTime.Now}::{netName}::[服-请求]\n{request}" +
                                "--------------------------------------------------\n";
                    OnLog(RPCLog.LogCode.Runtime, log);
                    context.WebSocket.SendAsync(config.Encoding.GetBytes(config.ServerRequestModelSerialize(request)), WebSocketMessageType.Text, true, cancellationToken);
                }
            }
            catch
            {

            }
        }

        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e, bool isThrow = true)
        {
            if (exceptionEvent != null)
            {
                exceptionEvent.Invoke(e, this);
            }
            if (isThrow) throw e;
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


        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        public void OnConnect()
        {
            ConnectEvent?.Invoke(this);
        }
        /// <summary>
        /// 断开连接时激活断开连接事件
        /// </summary>
        public void OnDisConnect()
        {
            DisConnectEvent?.Invoke(this);
        }
        #endregion
    }
}
