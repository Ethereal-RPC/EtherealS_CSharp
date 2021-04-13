using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace EtherealS.NativeServer
{

    public sealed class DataTokenPool
    {

        Stack<DataToken> pool;

        public DataTokenPool(int capacity)
        {
            this.pool = new Stack<DataToken>(capacity);
        }
        public DataToken Pop()
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

        public void Push(DataToken item)
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
