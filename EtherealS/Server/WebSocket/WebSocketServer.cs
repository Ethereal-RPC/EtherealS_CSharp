using EtherealS.Core.Model;
using EtherealS.Service;
using EtherealS.Service.WebSocket;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace EtherealS.Server.WebSocket
{

    public class WebSocketServer : Abstract.Server
    {
        #region --字段--

        #endregion
        #region --属性--
        public new WebSocketServerConfig Config { get => (WebSocketServerConfig)base.Config; set => base.Config = value; }

        #endregion
        public WebSocketServer()
        {
            if (!HttpListener.IsSupported)
            {
                OnLog(TrackLog.LogCode.Runtime, "Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
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
                Listener.Prefixes.Add(s.Replace("ethereal://", "http://"));
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
                string[] urls = request.RawUrl.Split(@"/", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                string service_name = urls[urls.Length - 1];
                //分析URL
                if (!ServiceCore.Get(net, service_name, out service))
                {
                    context.Response.StatusCode = 404;
                    ClientResponseModel responseModel = new ClientResponseModel();
                    responseModel.Error = new Error(Error.ErrorCode.NotFoundService, $"未找到服务{service_name}");
                    SendHttpHttp(context, service , responseModel);
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
                    ClientResponseModel responseModel = new ClientResponseModel();
                    try
                    {
                        if (request.ContentLength64 == -1)
                        {
                            responseModel.Error = new Error(Error.ErrorCode.Common, $"HTTP请求头请携带ContentLength");
                            SendHttpHttp(context, service,responseModel);
                            return;
                        }
                        //后续可以加入池优化，这里暂时先直接申请。
                        if (request.ContentLength64 > service.Config.MaxBufferSize)
                        {
                            responseModel.Error = new Error(Error.ErrorCode.BufferFlow, $"{service.Name}最大允许接收{service.Config.MaxBufferSize}字节");
                            SendHttpHttp(context, service, responseModel);
                            return;
                        }
                        byte[] body = new byte[request.ContentLength64];
                        await request.InputStream.ReadAsync(body, 0, body.Length);
                        clientRequestModel = service.Config.ClientRequestModelDeserialize(service.Config.Encoding.GetString(body));
                        responseModel.Id = clientRequestModel.Id;
                        //构造处理请求环境
                        baseToken.CanRequest = false;
                        baseToken.OnConnect();
                        ClientResponseModel clientResponseModel = await Task.Run(() => service.ClientRequestReceiveProcess(baseToken, clientRequestModel));
                        SendHttpHttp(context, service, clientResponseModel);
                    }
                    catch (Exception e)
                    {
                        responseModel.Error = new Error(Error.ErrorCode.BufferFlow, $"{e.Message}\n{e.StackTrace}");
                        SendHttpHttp(context, service, responseModel);
                    }
                    finally
                    {
                        context.Response.Close();
                        baseToken.OnDisConnect();
                    }
                }
            }
            catch (Exception e)
            {
                ClientResponseModel responseModel = new ClientResponseModel();
                responseModel.Error = new Error(Error.ErrorCode.BufferFlow, $"{e.Message}\n{e.StackTrace}");
                SendHttpHttp(context, service, responseModel);
            }
        }


        internal void SendHttpHttp(HttpListenerContext context, Service.Abstract.Service service, ClientResponseModel model)
        {
            if (model == null) return;
            try
            {
                string data = null;
                if (service != null)
                {
                    context.Response.ContentEncoding = service.Config.Encoding;
                    data = service.Config.ClientResponseModelSerialize(model);
                }
                else
                {
                    context.Response.ContentEncoding = context.Request.ContentEncoding;
                    data = JsonConvert.SerializeObject(model);
                }
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
