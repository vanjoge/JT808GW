using JX;
using System;
using System.Collections.Generic;
using System.Text;

namespace JTServer.Model
{
    public class OfflineCmd
    {
        /// <summary>
        /// 指令内容
        /// </summary>
        public JTDataBase JTData { get; set; }
        /// <summary>
        /// 消息ID
        /// </summary>
        public ushort MsgId { get; set; }
    }
}
