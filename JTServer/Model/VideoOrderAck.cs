using JX;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace JTServer.Model
{
    public class VideoOrderAck
    {
        /// <summary>
        /// 返回值状态:0(初始)，1（成功）,2（设备不在线），3（失败），4（等待回应超时），5（等待回应中），6（作废）
        /// </summary>
        public int Status { get; set; }
        public JTVideoListInfo VideoList { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrMessage { get; set; }
    }

    /// <summary>
    /// 通道传输状态
    /// </summary>
    public class JT0x9105ChannelItem 
    {
        /// <summary>
        /// 通道号
        /// </summary>
        [DataMember]
        public byte Channel { get; set; }

        /// <summary>
        /// 丢包率
        /// </summary>
        [DataMember]
        public byte PacketLossRate { get; set; }


    }
    /// <summary>
    /// cheji传输状态
    /// </summary>
    public class JT0x9105SimItem 
    {
        /// <summary>
        /// 手机号
        /// </summary>
        [DataMember]
        public string Sim { get; set; }

        /// <summary>
        /// 通道信息
        /// </summary>
        [DataMember]
        public List<JT0x9105ChannelItem> NotifyList { get; set; }

    }
}
