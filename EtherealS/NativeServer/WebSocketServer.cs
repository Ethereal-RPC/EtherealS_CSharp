using EtherealS.Model;
using EtherealS.NativeServer.Abstract;
using EtherealS.RPCNet;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace EtherealS.NativeServer
{

    public class WebSocketServer : Server
    {
        public WebSocketServer(string netName,string[] prefixes,ServerConfig config)
        {
            if (!HttpListener.IsSupported)
            {
                OnLog(RPCLog.LogCode.Runtime,"Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException(" ");
            this.netName = netName;
            this.config = config;
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
                baseToken = (WebSocketToken)config.CreateMethod();
                baseToken.LogEvent += OnLog;
                baseToken.ExceptionEvent += OnException;
                baseToken.ConnectEvent += Token_ConnectEvent;
                baseToken.DisConnectEvent += Token_DisConnectEvent;
                // Construct a response.
                if (request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null,config.KeepAliveInterval);
                    baseToken.NetName = netName;
                    baseToken.Config = config;
                    baseToken.CancellationToken = cancellationToken;
                    baseToken.IsWebSocket = true;
                    baseToken.Connect(webSocketContext);
                }
                else if (request.HttpMethod == "POST" && request.InputStream != null)
                {
                    try
                    {
                        //后续可以加入池优化，这里暂时先直接申请。
                        if (request.ContentLength64 > config.MaxBufferSize)
                        {
                            SendErrorToClient(context, Error.ErrorCode.BufferFlow, $"Net最大允许接收{config.MaxBufferSize}字节");
                        }
                        byte[] body = new byte[request.ContentLength64];
                        await request.InputStream.ReadAsync(body, 0, body.Length);
                        //客户端发来的一定是请求
                        ClientRequestModel clientRequestModel = null;
                        clientRequestModel = config.ClientRequestModelDeserialize(config.Encoding.GetString(body));
                        if (!NetCore.Get(netName, out Net net))
                        {
                            SendErrorToClient(context, Error.ErrorCode.NotFoundNet, $"未找到节点{netName}");
                        }
                        //构造处理请求环境
                        baseToken.IsWebSocket = false;
                        baseToken.OnConnect();
                        ClientResponseModel clientResponseModel = await Task.Run(() => net.ClientRequestReceiveProcess(baseToken, clientRequestModel));
                        byte[] bytes = config.Encoding.GetBytes(config.ClientResponseModelSerialize(clientResponseModel));
                        //发送请求结果
                        context.Response.ContentEncoding = config.Encoding;
                        context.Response.OutputStream.WriteAsync(bytes, cancellationToken);
                    }
                    finally
                    {
                        context.Response.Close();
                    }
                }
            }
            catch (Exception exception)
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
                context.Response.ContentEncoding = config.Encoding;
                string exception_serialize = config.ClientResponseModelSerialize(new ClientResponseModel(null, null, null, null, new Error(code, $"{message}", null)));
                byte[] exception_bytes = config.Encoding.GetBytes(exception_serialize);
                context.Response.OutputStream.WriteAsync(exception_bytes, 0, exception_bytes.Length);
                context.Response.Close();
            }
            catch
            {

            }
            throw new RPCException(RPCException.ErrorCode.Runtime, $"{message}");
        }
    }
}
