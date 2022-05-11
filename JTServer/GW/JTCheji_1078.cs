using JTServer.Model;
using JTServer.Worker;
using JX;
using SQ.Base;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace JTServer.GW
{
    public partial class JTCheji
    {
        public JTRTAVParametersUpload AvParameters;

        public void Send0x9105(List<JT0x9105ChannelItem> notifyList)
        {
            foreach (var item in notifyList)
            {
                SendAnswer(
                    jtdata.Package(0x9105, SimKey,
                       new JTRealVideoTransferStatusNotify()
                       {
                           Channel = item.Channel,
                           PacketLossRate = item.PacketLossRate
                       }.GetBinaryData()));
            }
        }
        public string Send1078ToDev(bool IsSuperiorPlatformSend, JTHeader head, byte[] bGps)
        {
            var jtpd = jtdata.Package(head.MsgId, SimKey, bGps);

            if (head.MsgId == 0x9206)
            {
                SendData_Active(jtpd);
                return jtpd.FirstHead.SerialNumber.ToString();
            }
            else if (head.MsgId == 0x9201 || head.MsgId == 0x9205)
            {
                var orderid = cl.MyTask.GetOrderID();

                SendData_Active(jtpd, OrderId: orderid);
                return orderid.ToString();
            }
            else
            {
                SendAnswer(jtpd);
                return "1";
            }
        }

        private bool JXJT1077(JTHeader head, byte[] bGps)
        {
            switch (head.MsgId)
            {
                case 0x1003:
                    var avp = JTRTAVParametersUpload.NewEntity(bGps);
                    var he = new HashEntry[]{
                        new HashEntry("AudioChannels", (int)avp.AudioChannels),
                        new HashEntry("AudioMaxChannels", (int)avp.AudioMaxChannels),
                        new HashEntry("AudioCodeType",(int) avp.AudioCodeType),
                        new HashEntry("AudioFrameLength", (int)avp.AudioFrameLength),
                        new HashEntry("AudioOut", (int)avp.AudioOut),
                        new HashEntry("AudioSamplingDigit", (int)avp.AudioSamplingDigit),
                        new HashEntry("AudioSamplingRate", (int)avp.AudioSamplingRate),
                        new HashEntry("VideoCodeType", (int)avp.VideoCodeType),
                        new HashEntry("VideoMaxChannels", (int)avp.VideoMaxChannels)
                    };
                    AvParameters = avp;
                    cl.MyTask.schRedis.Add(new HashSetAllWorker("AVParameters:" + head.Sim, he));
                    return true;
                case 0x1005:
                    SendDefaultAnswer(head);
                    return true;
                case 0x1205:

                    var videolist = JTVideoListInfo.NewEntity(bGps);
                    if (AllSendOrder.ContainsKey(videolist.SerialNumber))
                    {
                        var so = AllSendOrder[videolist.SerialNumber];
                        var sd = AllSendData[videolist.SerialNumber];
                        sd.Answer = SendDataState.Success;
                        CheckSendOrderByAnswer(sd.Head);
                        VideoOrderAck ack = new VideoOrderAck()
                        {
                            Status = 1,
                            VideoList = videolist
                        };

                        cl.MyTask.schRedis.Add(new StringSetWorker<VideoOrderAck>("OCX_ORDERINFO_" + so.OrderId, ack, new TimeSpan(1, 0, 0)));
                    }
                    return true;
                case 0x1206:
                    SendDefaultAnswer(head);
                    return true;


            }
            return false;
        }

    }
}
