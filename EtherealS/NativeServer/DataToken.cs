﻿using System;
using System.Net.Sockets;
using System.Text;
using EtherealS.Model;
using EtherealS.RPCNet;
using Newtonsoft.Json;

namespace EtherealS.NativeServer
{
    public sealed class DataToken
    {
        #region --User_Cutsom--
        private BaseUserToken token;
        #endregion
        private SocketAsyncEventArgs eventArgs;
        private DotNetty.Buffers.IByteBuffer buffer;
        private uint dynamicAdjustBufferCount = 1;
        //下面两部分只负责接收部分，发包构造部分并没有使用，修改时请注意！
        //下面这部分用于拆包分析
        private static int headsize = 32;//头包长度
        private static int bodysize = 4;//数据大小长度
        private static int patternsize = 1;//消息类型长度
        private static int futuresize = 27;//后期看情况加

        private Tuple<string, string> serverKey;
        private string netName;
        private ServerConfig config;
        public BaseUserToken Token  { get => token; set => token = value; }
        public SocketAsyncEventArgs EventArgs { get => eventArgs; set => eventArgs = value; }

        public DataToken(string netName,Tuple<string, string> serverKey,ServerConfig config)
        {
            this.serverKey = serverKey;
            this.netName = netName;
            this.config = config;
            dynamicAdjustBufferCount = config.DynamicAdjustBufferCount;
            eventArgs = new SocketAsyncEventArgs();
            eventArgs.UserToken = this;
            buffer = DotNetty.Buffers.UnpooledByteBufferAllocator.Default.DirectBuffer(config.BufferSize, config.MaxBufferSize);
            eventArgs.SetBuffer(buffer.Array, 0, buffer.Capacity);
        }
        public void DisConnect()
        {
            try
            {
                eventArgs.AcceptSocket?.Close();
                eventArgs.AcceptSocket?.Dispose();
            }
            catch
            {

            }
            eventArgs.AcceptSocket = null;
            if(buffer.Capacity != config.BufferSize)buffer.AdjustCapacity(config.BufferSize);
            buffer.ResetReaderIndex();
            buffer.ResetWriterIndex();
            eventArgs.SetBuffer(buffer.Array, 0, buffer.Capacity);
            if (token != null)
            {
                token.OnDisConnect();
                token.Net = null;
                token = null;
            }
        }
        public void Connect(Socket socket)
        {
            buffer.ResetReaderIndex();
            buffer.ResetWriterIndex();
            token = config.CreateMethod();
            token.NetName = netName;
            eventArgs.AcceptSocket = socket;
            token.Net = this;
            token.OnConnect();
        }
        public void ProcessData()
        {
            buffer.ResetReaderIndex();
            buffer.SetWriterIndex(eventArgs.BytesTransferred + buffer.WriterIndex);
            while (buffer.ReaderIndex < buffer.WriterIndex)
            {
                //数据包大小
                int count = buffer.WriterIndex - buffer.ReaderIndex;
                //凑够头包
                if (headsize < count)
                {
                    //Body数据长度 4 字节
                    int body_length = BitConverter.ToInt32(buffer.Array, buffer.ReaderIndex);
                    //请求方式 1 字节
                    byte pattern = buffer.Array[buffer.ReaderIndex + bodysize];
                    //未来用 27 字节
                    byte[] future = new byte[futuresize];
                    Buffer.BlockCopy(buffer.Array, buffer.ReaderIndex + bodysize + patternsize, future, 0, futuresize);
                    //数据总长
                    int length = body_length + headsize;
                    //判断Body数据是否足够
                    if (length <= count)
                    {
                        try
                        {
                            
                            ClientRequestModel request = null;
                            request = config.ClientRequestModelDeserialize(buffer.GetString(buffer.ReaderIndex + headsize, body_length, config.Encoding));
                            buffer.SetReaderIndex(buffer.ReaderIndex + length);
                            if (!NetCore.Get(netName, out Net net))
                            {
                                throw new RPCException(RPCException.ErrorCode.Runtime, $"Token查询{netName} Net时 不存在");
                            }
                            if (pattern == 0 && request != null)
                            {
                                net.ClientRequestReceive(token, request);
                            }
                        }
                        catch(Exception e)
                        {
                            throw new RPCException(RPCException.ErrorCode.Runtime,$"{serverKey}-{EventArgs.RemoteEndPoint}:用户函数发生异常错误，已自动断开连接！\n"  + e.Message);
                        }
                    }
                    else
                    {
                        //迁移数据至缓冲区头
                        if (buffer.ReaderIndex != 0)
                        {
                            Buffer.BlockCopy(buffer.Array, buffer.ReaderIndex, buffer.Array,0 , count);
                            buffer.ResetReaderIndex();
                            buffer.SetWriterIndex(count);
                        }
                        //数据还有一些未接收到，原因可能是前面已经有了数据，也可能是本身大小不够
                        if (length > buffer.Capacity)
                        {
                            if(length < buffer.MaxCapacity)
                            {
                                buffer.AdjustCapacity(length);
                                dynamicAdjustBufferCount = config.DynamicAdjustBufferCount;
                                EventArgs.SetBuffer(buffer.Array,buffer.WriterIndex,buffer.Capacity - count);
                                return;
                            }
                            else
                            {
                                token.DisConnect();
                                NetCore.Get(netName, out Net net);
                                throw new RPCException(RPCException.ErrorCode.Runtime, $"{serverKey}-{EventArgs.RemoteEndPoint}:用户请求数据量太大，中止接收！");
                            }
                        }
                        EventArgs.SetBuffer(count, buffer.Capacity - count);
                        return;
                    }
                }
                else
                {
                    //头包凑不够，迁移数据至缓冲区头
                    if (buffer.ReaderIndex != 0)
                    {
                        Buffer.BlockCopy(buffer.Array, buffer.ReaderIndex, buffer.Array, 0, count);
                        buffer.ResetReaderIndex();
                        buffer.SetWriterIndex(count);
                    }
                    EventArgs.SetBuffer(count, buffer.Capacity - count);
                    return;
                }
            }
            buffer.ResetReaderIndex();
            buffer.ResetWriterIndex();
            //充分退出，说明可能存在一定的空间浪费
            if (buffer.Capacity > config.BufferSize && dynamicAdjustBufferCount-- == 0)
            {
                buffer.AdjustCapacity(config.BufferSize);
                dynamicAdjustBufferCount = config.DynamicAdjustBufferCount;
                EventArgs.SetBuffer(buffer.Array,0,buffer.Capacity);
            }
            else EventArgs.SetBuffer(0, buffer.Capacity);
        }
    }
}
