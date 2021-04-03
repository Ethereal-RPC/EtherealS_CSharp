using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace EtherealS.NativeNetwork
{

    public sealed class SocketAsyncEventArgsPool
    {

        Stack<SocketAsyncEventArgs> pool;

        public SocketAsyncEventArgsPool(int capacity)
        {
            this.pool = new Stack<SocketAsyncEventArgs>(capacity);
        }
        public SocketAsyncEventArgs Pop()
        {
            lock (this.pool)
            {
                if (this.pool.Count > 0)
                {
                    return this.pool.Pop();
                }
                else
                {
                    return null;
                }
            }
        }

        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null) 
            { 
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); 
            }
            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }
    }
}
