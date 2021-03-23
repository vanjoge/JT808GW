using System;
using System.Collections.Generic;
using System.Text;

namespace JTServer.Model
{
    public class Force_Command
    {
        /// <summary>
        /// 车辆ID
        /// </summary>
        public long F_VEHICLE_ID { get; set; }
        /// <summary>
        /// 指令名称
        /// </summary>
        public string F_CMD_NAME { get; set; }
        /// <summary>
        /// 808/北斗消息ID
        /// </summary>
        public ushort F_MSG_ID { get; set; }
        /// <summary>
        /// 指令参数json串
        /// </summary>
        public string F_CMD_PARAMS { get; set; }
        /// <summary>
        /// 命令发送状态
        /// </summary>
        public short F_CMD_STATUS { get; set; }
        /// <summary>
        /// 发送次数
        /// </summary>
        public short F_SEND_COUNT { get; set; }
        /// <summary>
        /// -发送时间
        /// </summary>
        public DateTime F_SEND_TIME { get; set; }
        /// <summary>
        /// 指令信息表ID
        /// </summary>
        public long F_ORDER_ID { get; set; }
    }
}
