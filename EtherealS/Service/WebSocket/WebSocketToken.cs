using EtherealS.Core.Model;
using EtherealS.Service.Abstract;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EtherealS.Service.WebSocket
{
    public class WebSocketToken : Token
    {
        #region --字段--
        private HttpListenerWebSocketContext context;
        private CancellationToken cancellationToken;
        #endregion

        #region --属性--
        public CancellationToken CancellationToken { get => cancellationToken; set => cancellationToken = value; }
        public HttpListenerWebSocketContext Context { get => context; set => context = value; }

        #endregion

        #region --方法--

        #endregion

        #region --网络方法--
        public WebSocketToken()
        {

        }
        internal void Connect(HttpListenerWebSocketContext context)
        {
            Context = context;
            OnConnect();
            ProcessData();
        }
        internal async void ProcessData()
        {
            System.Net.WebSockets.WebSocket webSocket = Context.WebSocket;
            ServiceConfig config = Service.Config;
            byte[] receiveBuffer = null;
            int offset = 0;
            int free = config.BufferSize;
            // While the WebSocket connection remains open run a simple loop that receives data and sends it back.
            while (webSocket.State == WebSocketState.Open && service != null && service.Enable)
            {
                if (receiveBuffer == null)
                {
                    receiveBuffer = new byte[config.BufferSize];
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
                        string data = config.Encoding.GetString(receiveBuffer);
                        offset = 0;
                        free = config.BufferSize;
                        string a = config.Encoding.GetString(receiveBuffer);
                        Console.WriteLine(a);
                        ClientRequestModel request = config.ClientRequestModelDeserialize(config.Encoding.GetString(receiveBuffer));
                        ClientResponseModel responseModel = await Task.Run(() => Service.ClientRequestReceiveProcess(this, request));
                        SendClientResponse(responseModel);
                    }
                    else if (free == 0)
                    {
                        var newSize = receiveBuffer.Length + config.BufferSize;
                        if (newSize > config.MaxBufferSize)
                        {
                            SendClientResponse(new ClientResponseModel(null, null, new Error(Error.ErrorCode.NotFoundNet, $"缓冲区:{newSize}-超过最大字节数:{config.MaxBufferSize}，已断开连接！", null)));
                            DisConnect($"缓冲区:{newSize}-超过最大字节数:{config.MaxBufferSize}，已断开连接！");
                            return;
                        }
                        byte[] new_bytes = new byte[newSize];
                        Array.Copy(receiveBuffer, 0, new_bytes, 0, offset);
                        receiveBuffer = new_bytes;
                        free = receiveBuffer.Length - offset;
                        continue;
                    }
                }
                catch (Exception e)
                {
                    SendClientResponse(new ClientResponseModel(null, null, new Error(Error.ErrorCode.Common, $"{e.Message}", null)));
                    DisConnect("发生报错");
                    return;
                }
            }
            DisConnect("循环结束");
        }
        public override void DisConnect(string reason)
        {
            if (Service.Config.AutoManageTokens) UnRegister();
            Context.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken);
            OnDisConnect();
        }

        internal override void SendClientResponse(ClientResponseModel response)
        {
            if (response == null) return;
            try
            {
                if (Context.WebSocket.State == WebSocketState.Open && canRequest)
                {
                    Context.WebSocket.SendAsync(Service.Config.Encoding.GetBytes(Service.Config.ClientResponseModelSerialize(response)), WebSocketMessageType.Text, true, CancellationToken);
                }
            }
            catch (TrackException e)
            {
                OnException(e);
            }
        }
        internal override void SendServerRequest(ServerRequestModel request)
        {
            if (request == null) return;
            try
            {
                if (Context.WebSocket.State == WebSocketState.Open && canRequest)
                {
                    Context.WebSocket.SendAsync(Service.Config.Encoding.GetBytes(Service.Config.ServerRequestModelSerialize(request)), WebSocketMessageType.Text, true, CancellationToken);
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
