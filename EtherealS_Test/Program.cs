﻿using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Net.Abstract;
using EtherealS.Net.WebSocket;
using EtherealS.Request;
using EtherealS.Server;
using EtherealS.Server.WebSocket;
using EtherealS.Service;
using EtherealS_Test.RequestDemo;
using EtherealS_Test.ServiceDemo;
using System;

namespace EtherealS_Test
{
    public class Program
    {
        public static void Main()
        {
            string ip = "127.0.0.1";
            string port;
            Console.WriteLine("请选择端口(0-3)");
            int mode = int.Parse(Console.ReadLine());
            switch (mode)
            {
                case 0:
                    port = "28015";
                    break;
                case 1:
                    port = "28016";
                    break;
                case 2:
                    port = "28017";
                    break;
                case 3:
                    port = "28018";
                    break;
                default:
                    port = mode.ToString();
                    break;
            }
            Console.Title = $"{ip}-{port}";
            //建立网关
            Net net = NetCore.Register(new WebSocketNet("demo"));
            net.ExceptionEvent += Config_ExceptionEvent;
            net.LogEvent += Config_LogEvent;
            //向网关注册服务
            ServerService service = ServiceCore.Register(net, new ServerService());
            //向网关注册请求
            ClientRequest request = RequestCore.Register<ClientRequest>(service);
            //本例中，突出服务类可作为正常类
            service.UserRequest = request;
            //向网关注册连接(提供一个生成User的方法)
            WebSocketServer server = new WebSocketServer();
            server.Prefixes.Add($"ethereal://{ip}:{port}/NetDemo/");
            ServerCore.Register(net, server);
            //发布服务
            net.Publish();
            Console.WriteLine("服务器初始化完成....");
            Console.ReadKey();
        }


        private static void Config_LogEvent(TrackLog log)
        {
            Console.WriteLine(log.Message);
        }

        private static void Config_ExceptionEvent(Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw exception;
        }
    }
}
