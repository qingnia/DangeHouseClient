using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TCPHelper
{
    /// <summary>
    /// 接收socket的行为
    /// </summary>
    public enum EnSocketAction
    {
        /// <summary>
        /// socket发生连接
        /// </summary>
        Connect = 1,
        /// <summary>
        /// socket发送数据
        /// </summary>
        SendMsg = 2,
        /// <summary>
        /// socket关闭
        /// </summary>
        Close = 4
    }
    /// <summary>
    /// 对异步接收时的对象状态的封装，将socket与接收到的数据封装在一起
    /// </summary>
    public class StateObject
    {
        public TcpClient Client { get; set; }
        private byte[] listData = new byte[2048];
        /// <summary>
        /// 接收的数据
        /// </summary>
        public byte[] ListData
        {
            get
            {
                return listData;
            }
            set
            {
                listData = value;
            }
        }
    }
}