using EtherealS.Model;
using EtherealS.RPCNet;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EtherealS.NativeServer
{

    public class Server
    {

        #region --ί��--

        public delegate void OnExceptionDelegate(Exception exception, Server server);

        public delegate void OnLogDelegate(RPCLog log, Server server);

        /// <summary>
        /// ����ί��
        /// </summary>
        /// <param name="token"></param>
        public delegate void ListenerSuccessDelegate(Server listener);
        /// <summary>
        ///     
        /// </summary>
        /// <param name="token"></param>
        public delegate void ListenerFailDelegate(Server listener);

        #endregion

        #region --�¼��ֶ�--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --�¼�����--
        /// <summary>
        /// ��־����¼�
        /// </summary>
        public event OnLogDelegate LogEvent
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
        /// �׳��쳣�¼�
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent
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

        /// <summary>
        /// �����¼�
        /// </summary>
        public event ListenerSuccessDelegate ListenerSuccessEvent;
        /// <summary>
        /// �Ͽ������¼�
        /// </summary>
        public event ListenerFailDelegate ListenerFailEvent;
        #endregion

        #region --�ֶ�--
        private string netName;
        private ServerConfig config;
        private HttpListener listener;
        private CancellationToken cancellationToken = CancellationToken.None;
        private List<string> prefixes;
        #endregion

        #region --����--

        public HttpListener Listener { get => listener; set => listener = value; }
        public List<string> Prefixes { get => prefixes; set => prefixes = value; }
        #endregion
        public Server(string netName,string[] prefixes,ServerConfig config)
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

        public void Start()
        {
            Listener.Start();
            OnListenerSuccess();
            Listener.BeginGetContext(new AsyncCallback(ListenerCallbackAsync), null);
        }
        public void Close()
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
                BaseToken baseToken = null;
                baseToken = config.CreateMethod();
                baseToken.LogEvent += Token_LogEvent;
                baseToken.ExceptionEvent += Token_ExceptionEvent;
                baseToken.ConnectEvent += Token_ConnectEvent;
                baseToken.DisConnectEvent += Token_DisConnectEvent;
                // Construct a response.
                if (request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null,config.KeepAliveInterval);
                    baseToken.Init(netName, config, cancellationToken);
                    baseToken.IsWebSocket = true;
                    baseToken.Connect(webSocketContext);
                }
                else if (request.HttpMethod == "POST" && request.InputStream != null)
                {
                    try
                    {
                        //�������Լ�����Ż���������ʱ��ֱ�����롣
                        if (request.ContentLength64 > config.MaxBufferSize)
                        {
                            SendErrorToClient(context, Error.ErrorCode.BufferFlow, $"Net����������{config.MaxBufferSize}�ֽ�");
                        }
                        byte[] body = new byte[request.ContentLength64];
                        await request.InputStream.ReadAsync(body, 0, body.Length);
                        //�ͻ��˷�����һ��������
                        ClientRequestModel clientRequestModel = null;
                        clientRequestModel = config.ClientRequestModelDeserialize(config.Encoding.GetString(body));
                        if (!NetCore.Get(netName, out Net net))
                        {
                            SendErrorToClient(context, Error.ErrorCode.NotFoundNet, $"δ�ҵ��ڵ�{netName}");
                        }
                        //���촦�����󻷾�
                        baseToken.IsWebSocket = false;
                        baseToken.OnConnect();
                        ClientResponseModel clientResponseModel = await Task.Run(() => net.ClientRequestReceiveProcess(baseToken, clientRequestModel));
                        byte[] bytes = config.Encoding.GetBytes(config.ClientResponseModelSerialize(clientResponseModel));
                        //����������
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
        private void SendErrorToClient(HttpListenerContext context,Error.ErrorCode code,string message)
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


        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {
            if (exceptionEvent != null)
            {
                exceptionEvent.Invoke(e,this);
            }
            
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
        private void Token_ExceptionEvent(Exception exception, BaseToken token)
        {
            OnException(exception);
        }

        private void Token_LogEvent(RPCLog log, BaseToken toke)
        {
            OnLog(log);
        }
        /// <summary>
        /// ����ʱ���������¼�
        /// </summary>
        public void OnListenerSuccess()
        {
            ListenerSuccessEvent?.Invoke(this);
        }
        /// <summary>
        /// �Ͽ�����ʱ����Ͽ������¼�
        /// </summary>
        public void OnListenerFail()
        {
            ListenerFailEvent?.Invoke(this);
        }
    }
}
