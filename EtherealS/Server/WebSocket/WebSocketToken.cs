using System;
using System.Net.WebSockets;
using System.Threading;
using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Server.Abstract;

namespace EtherealS.Server.WebSocket
{
    public abstract class WebSocketToken : BaseToken
    {
        #region --字段--
        private HttpListenerWebSocketContext context;
        private CancellationToken cancellationToken;
        #endregion

        #region --属性--
        public CancellationToken CancellationToken { get => cancellationToken; set => cancellationToken = value; }
        public HttpListenerWebSocketContext Context { get => context; set => context = value; }
        public new WebSocketServerConfig Config { get => (WebSocketServerConfig)config; set => config = value; }

        #endregion

        #region --方法--

        #endregion

        #region --网络方法--
        public WebSocketToken()
        {

        }
        internal void Connect(HttpListenerWebSocketContext context)
        {
            this.Context = context;
            OnConnect();
            ProcessData();
        }
        internal async void ProcessData()
        {
            System.Net.WebSockets.WebSocket webSocket = Context.WebSocket;
            byte[] receiveBuffer = null;
            int offset = 0;
            int free = Config.BufferSize;
            ClientRequestModel request = null;

            // While the WebSocket connection remains open run a simple loop that receives data and sends it back.
            while (webSocket.State == WebSocketState.Open)
            {
                if (receiveBuffer == null)
                {
                    receiveBuffer = new byte[Config.BufferSize];
                }

                try
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, offset, free), CancellationToken);
                    offset += receiveResult.Count;
                    free -= receiveResult.Count;
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        OnDisConnect();
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken);
                        continue;
                    }

                    if (receiveResult.EndOfMessage)
                    {
                        string data = Config.Encoding.GetString(receiveBuffer);
                        offset = 0;
                        free = Config.BufferSize;
                        request = Config.ClientRequestModelDeserialize(Config.Encoding.GetString(receiveBuffer));
                        if (!NetCore.Get(netName, out Net.Abstract.Net net))
                        {
                            SendClientResponse(new ClientResponseModel( null, null, request.Id, request.Service, new Error(Error.ErrorCode.NotFoundNet, $"Token查询{netName} Net时 不存在", null)));
                            return;
                        }
                        SendClientResponse(net.ClientRequestReceiveProcess(this, request));
                    }
                    else if (free == 0)
                    {
                        var newSize = receiveBuffer.Length + Config.BufferSize;
                        if (newSize > Config.MaxBufferSize)
                        {
                            SendClientResponse(new ClientResponseModel(null, null, null, null, new Error(Error.ErrorCode.NotFoundNet, $"缓冲区:{newSize}-超过最大字节数:{Config.MaxBufferSize}，已断开连接！", null)));
                            DisConnect($"缓冲区:{newSize}-超过最大字节数:{Config.MaxBufferSize}，已断开连接！");
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
                    if (request != null)
                    {
                        SendClientResponse(new ClientResponseModel(null, null, request.Id, request.Service, new Error(Error.ErrorCode.Common, $"{e.Message}", null)));
                    }
                    else SendClientResponse(new ClientResponseModel( null, null, null,null, new Error(Error.ErrorCode.Common, $"{e.Message}", null)));
                    DisConnect("发生报错");
                    return;
                }
            }
        }
        public override void DisConnect(string reason)
        {
            if(Config.AutoManageTokens)UnRegister();
            Context.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken);
            OnDisConnect();
        }

        internal override void SendClientResponse(ClientResponseModel response)
        {
            try
            {
                if (Context.WebSocket.State == WebSocketState.Open && isWebSocket)
                {
                    string log = "--------------------------------------------------\n" +
                                $"{DateTime.Now}::{netName}::[服-返回]\n{response}" +
                                "--------------------------------------------------\n";
                    OnLog(TrackLog.LogCode.Runtime, log);
                    Context.WebSocket.SendAsync(Config.Encoding.GetBytes(Config.ClientResponseModelSerialize(response)), WebSocketMessageType.Text, true, CancellationToken);
                }
            }
            catch(TrackException e)
            {
                OnException(e);
            }
        }
        internal override void SendServerRequest(ServerRequestModel request )
        {
            try
            {
                if (Context.WebSocket.State == WebSocketState.Open && isWebSocket)
                {
                    string log = "--------------------------------------------------\n" +
                                $"{DateTime.Now}::{netName}::[服-请求]\n{request}" +
                                "--------------------------------------------------\n";
                    OnLog(TrackLog.LogCode.Runtime, log);
                    Context.WebSocket.SendAsync(Config.Encoding.GetBytes(Config.ServerRequestModelSerialize(request)), WebSocketMessageType.Text, true, CancellationToken);
                }
            }
            catch (TrackException e)
            {
                OnException(e);
            }
        }
        #endregion
    }
}
