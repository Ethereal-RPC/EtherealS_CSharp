﻿using System;
using System.Reflection;
using System.Text;
using EtherealS.Core.Model;
using EtherealS.Request.Abstract;
using EtherealS.Server.Abstract;

namespace EtherealS.Request.WebSocket
{
    public class WebSocketRequest : Abstract.Request
    {
        #region --属性--

        public new WebSocketRequestConfig Config { get => (WebSocketRequestConfig)config; set => config = value; }

        #endregion

        public static new Abstract.Request Register<T>(string netName, string servicename, RequestConfig config)
        {
            Abstract.Request proxy = Create<T, WebSocketRequest>() as Abstract.Request;
            proxy.Name = servicename;
            proxy.NetName = netName ?? throw new ArgumentNullException(nameof(netName));
            proxy.Config = config;
            return proxy;
        }
        protected override object Invoke(MethodInfo targetMethod, object[] args)    
        {
            Attribute.Request rpcAttribute = targetMethod.GetCustomAttribute<Attribute.Request>();
            if (rpcAttribute != null)
            {
                //这里要连接字符串，发现StringBuilder效率高一些.
                StringBuilder methodid = new StringBuilder(targetMethod.Name);
                string[] obj = null;
                int param_count;
                if (args != null) param_count = args.Length;
                else param_count = 0;
                if (param_count > 1)
                {
                    obj = new string[param_count - 1];
                    if (rpcAttribute.Paramters == null)
                    {
                        ParameterInfo[] parameters = targetMethod.GetParameters();
                        for (int i = 1; i < param_count; i++)
                        {
                            if(Config.Types.TypesByType.TryGetValue(parameters[i].ParameterType,out RPCType type))
                            {
                                
                                methodid.Append("-" + type.Name);
                                obj[i - 1] = type.Serialize(args[i]);
                            }
                            else throw new RPCException($"C#对应的{args[i].GetType()}类型参数尚未注册");
                        }
                    }
                    else
                    {
                        string[] types_name = rpcAttribute.Paramters;
                        if(param_count == types_name.Length)
                        {
                            for (int i = 1; i < param_count; i++)
                            {
                                if (Config.Types.TypesByName.TryGetValue(types_name[i], out RPCType type))
                                {
                                    methodid.Append("-" + type.Name);
                                    obj[i - 1] = type.Serialize(args[i]);
                                }
                                else throw new RPCException($"C#对应的{args[i].GetType()}类型参数尚未注册") ;
                            }
                        }
                        else throw new RPCException($"方法体{targetMethod.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length}个,Method:{param_count}个") ;
                    }

                }
                ServerRequestModel request = new ServerRequestModel(Name, methodid.ToString(), obj);
                if (args[0] != null && (args[0] as Token != null))
                {
                    if (!((args[0] as Token).IsWebSocket))
                    {
                        throw new RPCException(RPCException.ErrorCode.Runtime, $"{name}-{methodid}传递了非WebSocket协议的Token！");
                    }
                    string log = "";
                    log += "---------------------------------------------------------\n";
                    log += $"{ DateTime.Now}::{NetName}::[服 - 指令]\n{ request}\n";
                    log += "---------------------------------------------------------\n";
                    OnLog(RPCLog.LogCode.Runtime, log);
                    (args[0] as Token).SendServerRequest(request);
                    return null;
                }
                return null;
            }
            return null;
        }

    }
}