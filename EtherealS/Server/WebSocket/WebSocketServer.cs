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
        public WebSocketServer(List<string> prefixes,CreateInstance createMethod) : base(prefixes,createMethod)
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
            //客户端发来的一定是请求
            ClientRequestModel clientRequestModel = null;
            try
            {
                WebSocketToken baseToken = null;
                baseToken = base.CreateMethod() as WebSocketToken;
                baseToken.LogEvent += OnLog;
                baseToken.ExceptionEvent += OnException;
                // Construct a response.
                if (request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null, Config.KeepAliveInterval);
                    baseToken.Server = this;
                    baseToken.Config = Config;
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
                            SendHttpToClient(context, new ClientResponseModel(null, clientRequestModel?.Id, clientRequestModel?.Service, new Error(Error.ErrorCode.BufferFlow, $"HTTP请求头请携带ContentLength", null)));
                            return;
                        }
                        //后续可以加入池优化，这里暂时先直接申请。
                        if (request.ContentLength64 > Config.MaxBufferSize)
                        {
                            SendHttpToClient(context, new ClientResponseModel(null, clientRequestModel?.Id, clientRequestModel?.Service, new Error(Error.ErrorCode.BufferFlow, $"Net最大允许接收{Config.MaxBufferSize}字节", null)));
                            return;
                        }
                        byte[] body = new byte[request.ContentLength64];
                        await request.InputStream.ReadAsync(body, 0, body.Length);
                        clientRequestModel = base.Config.ClientRequestModelDeserialize(Config.Encoding.GetString(body));
                        string log = "--------------------------------------------------\n" +
                                     $"{DateTime.Now}::{base.net}::[服-返回]\n{request}\n" +
                                     "--------------------------------------------------\n";
                        if (config.Debug) OnLog(TrackLog.LogCode.Runtime, log);
                        //构造处理请求环境
                        baseToken.CanRequest = false;
                        baseToken.OnConnect();
                        ClientResponseModel clientResponseModel = await Task.Run(() => net.ClientRequestReceiveProcess(baseToken, clientRequestModel));
                        SendHttpToClient(context, clientResponseModel);
                    }
                    catch(Exception e)
                    {
                        SendHttpToClient(context, new ClientResponseModel(null, clientRequestModel?.Id, clientRequestModel?.Service, new Error(Error.ErrorCode.Common, $"{e.Message}\n{e.StackTrace}", null)));
                    }
                    finally
                    {
                        context.Response.Close();
                    }
                }
            }
            catch (Exception exception)
            {
                SendHttpToClient(context, new ClientResponseModel(null,clientRequestModel?.Id,clientRequestModel?.Service,new Error(Error.ErrorCode.Common,exception.Message,null)));
                OnException(new TrackException(exception));
            }
        }

        internal void SendHttpToClient(HttpListenerContext context, ClientResponseModel clientResponse)
        {
            try
            {
                context.Response.ContentEncoding = Config.Encoding;
                string exception_serialize = base.Config.ClientResponseModelSerialize(clientResponse);
                byte[] exception_bytes = Config.Encoding.GetBytes(exception_serialize);
                context.Response.OutputStream.Write(exception_bytes, 0, exception_bytes.Length);
                context.Response.Close();
            }
            catch
            {

            }
        }
    }
}
