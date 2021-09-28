using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Server.Abstract;

namespace EtherealS.Server.WebSocket
{

    public class WebSocketServer : Abstract.Server
    {
        #region --属性--
        public new WebSocketServerConfig Config { get => (WebSocketServerConfig)base.Config; set => base.Config = value; }

        #endregion
        public WebSocketServer(string netName,string[] prefixes,ServerConfig config)
        {
            if (!HttpListener.IsSupported)
            {
                OnLog(TrackLog.LogCode.Runtime,"Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException(" ");
            this.netName = netName;
            this.Config = config as WebSocketServerConfig;
            this.prefixes = new List<string>(prefixes);
            // Create a listener.
            Listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                Listener.Prefixes.Add("http://" + s);
            } 
            Listener.IgnoreWriteExceptions = true;
        }

        public override void Start()
        {
            Listener.Start();
            OnListenerSuccess();
            Listener.BeginGetContext(new AsyncCallback(ListenerCallbackAsync), null);
        }
        public override void Close()
        {
            Listener.Close();
            OnListenerFail();
        }

        private async void ListenerCallbackAsync(IAsyncResult result)
        {
            Listener.BeginGetContext(new AsyncCallback(ListenerCallbackAsync), null);
            // Call EndGetContext to complete the asynchronous operation.
            HttpListenerContext context = Listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            try
            {
                WebSocketToken baseToken = null;
                baseToken = base.Config.CreateMethod() as WebSocketToken;
                baseToken.LogEvent += OnLog;
                baseToken.ExceptionEvent += OnException;
                baseToken.ConnectEvent += Token_ConnectEvent;
                baseToken.DisConnectEvent += Token_DisConnectEvent;
                // Construct a response.
                if (request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null, Config.KeepAliveInterval);
                    baseToken.NetName = netName;
                    baseToken.Config = Config;
                    baseToken.CancellationToken = cancellationToken;
                    baseToken.IsWebSocket = true;
                    baseToken.Connect(webSocketContext);
                }
                else if (request.HttpMethod == "POST" && request.InputStream != null)
                {
                    try
                    {
                        //后续可以加入池优化，这里暂时先直接申请。
                        if (request.ContentLength64 > Config.MaxBufferSize)
                        {
                            SendErrorToClient(context, Error.ErrorCode.BufferFlow, $"Net最大允许接收{Config.MaxBufferSize}字节");
                        }
                        byte[] body = new byte[request.ContentLength64];
                        await request.InputStream.ReadAsync(body, 0, body.Length);
                        //客户端发来的一定是请求
                        ClientRequestModel clientRequestModel = null;
                        clientRequestModel = base.Config.ClientRequestModelDeserialize(Config.Encoding.GetString(body));
                        if (!NetCore.Get(netName, out Net.Abstract.Net net))
                        {
                            SendErrorToClient(context, Error.ErrorCode.NotFoundNet, $"未找到节点{netName}");
                        }
                        //构造处理请求环境
                        baseToken.IsWebSocket = false;
                        baseToken.OnConnect();
                        ClientResponseModel clientResponseModel = await Task.Run(() => net.ClientRequestReceiveProcess(baseToken, clientRequestModel));
                        byte[] bytes = Config.Encoding.GetBytes(base.Config.ClientResponseModelSerialize(clientResponseModel));
                        //发送请求结果
                        context.Response.ContentEncoding = Config.Encoding;
                        context.Response.OutputStream.WriteAsync(bytes, cancellationToken);
                    }
                    finally
                    {
                        context.Response.Close();
                    }
                }
            }
            catch (TrackException exception)
            {
                SendErrorToClient(context, Error.ErrorCode.Common, exception.Message);
                OnException(exception);
            }
        }

        private void Token_ConnectEvent(BaseToken token)
        {
            
        }
        private void Token_DisConnectEvent(BaseToken token)
        {

        }
        internal override void SendErrorToClient(HttpListenerContext context,Error.ErrorCode code,string message)
        {
            try
            {
                context.Response.ContentEncoding = Config.Encoding;
                string exception_serialize = base.Config.ClientResponseModelSerialize(new ClientResponseModel(null, null, null, null, new Error(code, $"{message}", null)));
                byte[] exception_bytes = Config.Encoding.GetBytes(exception_serialize);
                context.Response.OutputStream.WriteAsync(exception_bytes, 0, exception_bytes.Length);
                context.Response.Close();
            }
            catch
            {

            }
            throw new TrackException(TrackException.ErrorCode.Runtime, $"{message}");
        }
    }
}
