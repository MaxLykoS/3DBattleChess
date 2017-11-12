﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MasterServer.BottomLayer
{
    class Conn
    {
        //连接
        private Socket Client;

        public bool IsUse=false;

        //缓冲区长度
        public const int BUFFER_SIZE = 1024;
        //缓冲
        public byte[] ReadBuff = new byte[BUFFER_SIZE];
        public int BuffCount = 0;

        //粘包分包
        public byte[] LenBytes = new byte[sizeof(Int32)];
        public Int32 msgLength = 0;

        //心跳时间
        public long LastTickTime = long.MinValue;

        /// <summary>
        /// 客户端ip地址
        /// </summary>
        /// <returns></returns>
        public string Address
        {
            get
            {
                if (!IsUse)
                    return "该连接未启用";
                return Client.RemoteEndPoint.ToString();
            }
        }

        public Conn()
        {
            ReadBuff = new byte[BUFFER_SIZE];
        }

        public void Init(Socket client)
        {
            Client = client;
            IsUse = true;
            BuffCount = 0;

            //心跳处理
        }

        /// <summary>
        /// 剩余缓冲区长度
        /// </summary>
        /// <returns></returns>
        public int BuffRemain()
        {
            return BUFFER_SIZE - BuffCount;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Close()
        {
            if (!IsUse)
                return;

            Console.WriteLine("断开连接" + Address);
            Client.Shutdown(SocketShutdown.Both);
            Client.Close();
            IsUse = false;

        }

        /// <summary>
        /// 异步接收连接
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void BeginReceive(AsyncCallback callback, object state)
        {
            Client.BeginReceive(ReadBuff, BuffCount, BuffRemain(), SocketFlags.None, callback, state);
        }

        /// <summary>
        /// 结束接受数据
        /// </summary>
        /// <param name="ar"></param>
        public int EndReceive(IAsyncResult ar)
        {
            return Client.EndReceive(ar);
        }
    }
}
