using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Server.Abstract;
using EtherealS.Service;

namespace EtherealS.Server.WebSocket
{

    public class WebSocketServer : Abstract.Server
    {
        #region --字段--

        #endregion
        #region --属性--
        public new WebSocketServerConfig Config { get => (WebSocketServerConfig)base.Config; set => base.Config = value; }

        #endregion
        public WebSocketServer(List<string> prefixes) : base(prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                OnLog(TrackLog.LogCode.Runtime,"Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            Config = new WebSocketServerConfig();
        }

        public override void Start()
        {
            // Create a listener.
            Listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                Listener.Prefixes.Add(s.Replace("ethereal://","http://"));
            }
            Listener.IgnoreWriteExceptions = true;
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
            Service.Abstract.Service service = null;
            ClientRequestModel clientRequestModel = null;
            try
            {
                string[] urls = request.RawUrl.Split(@"/",StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                string service_name = urls[urls.Length - 1];
                //分析URL
                if (!ServiceCore.Get(net, service_name, out service))
                {
                    context.Response.StatusCode = 404;
                    SendHttpRaw(context, $"未找到服务{service_name}");
                    return;
                }
                WebSocketToken baseToken = null;
                baseToken = service.TokenCreateInstance() as WebSocketToken;
                baseToken.Service = service;
                baseToken.LogEvent += OnLog;
                baseToken.ExceptionEvent += OnException;
                // Construct a response.
                if (request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null, Config.KeepAliveInterval);
                    baseToken.CancellationToken = cancellationToken;
                    baseToken.CanRequest = true;
                    baseToken.Connect(webSocketContext);
                }
                else if (request.HttpMethod == "POST")
                {
                    try
                    {
                        if (request.ContentLength64 == -1)
                        {
                            SendHttpModel(context, service, new ClientResponseModel(null, clientRequestModel?.Id, new Error(Error.ErrorCode.BufferFlow, $"HTTP请求头请携带ContentLength", null)));
                            return;
                        }
                        //后续可以加入池优化，这里暂时先直接申请。
                        if (request.ContentLength64 > service.Config.MaxBufferSize)
                        {
                            SendHttpModel(context, service, new ClientResponseModel(null, clientRequestModel?.Id, new Error(Error.ErrorCode.BufferFlow, $"{service.Name}最大允许接收{service.Config.MaxBufferSize}字节", null)));
                            return;
                        }
                        byte[] body = new byte[request.ContentLength64];
                        await request.InputStream.ReadAsync(body, 0, body.Length);
                        clientRequestModel = service.Config.ClientRequestModelDeserialize(service.Config.Encoding.GetString(body));
                        //构造处理请求环境
                        baseToken.CanRequest = false;
                        baseToken.OnConnect();
                        ClientResponseModel clientResponseModel = await Task.Run(() => service.ClientRequestReceiveProcess(baseToken, clientRequestModel));
                        SendHttpModel(context, service, clientResponseModel);
                    }
                    catch(Exception e)
                    {
                        SendHttpModel(context, service, new ClientResponseModel(null, clientRequestModel?.Id, new Error(Error.ErrorCode.Common, $"{e.Message}\n{e.StackTrace}", null)));
                    }
                    finally
                    {
                        context.Response.Close();
                        baseToken.OnDisConnect();
                    }
                }
            }
            catch (Exception exception)
            {
                if (service != null) SendHttpModel(context, service, new ClientResponseModel(null, clientRequestModel?.Id, new Error(Error.ErrorCode.Common, exception.Message, null)));
                else SendHttpRaw(context, $"{exception.Message}\n{exception.StackTrace}");
            }
        }

        internal void SendHttpModel(HttpListenerContext context,Service.Abstract.Service service, ClientResponseModel clientResponse)
        {
            try
            {
                context.Response.ContentEncoding = service.Config.Encoding;
                string exception_serialize = service.Config.ClientResponseModelSerialize(clientResponse);
                byte[] exception_bytes = service.Config.Encoding.GetBytes(exception_serialize);
                context.Response.OutputStream.Write(exception_bytes, 0, exception_bytes.Length);
                context.Response.Close();
            }
            catch
            {

            }
        }
        internal void SendHttpRaw(HttpListenerContext context, string data)
        {
            try
            {
                context.Response.ContentEncoding = context.Request.ContentEncoding;
                byte[] exception_bytes = context.Response.ContentEncoding.GetBytes(data);
                context.Response.OutputStream.Write(exception_bytes, 0, exception_bytes.Length);
                context.Response.Close();
            }
            catch
            {

            }
        }
    }
}
