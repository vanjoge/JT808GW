using JX;
using Network;
using SQ.Base;
using SQ.Base.GW;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JTServer.GW
{
    public class JTClient : Client<JTCheji>
    {

        protected override bool SendDataInternal(byte[] bts, int offset, int size, ulong timestamp)
        {
            throw new NotImplementedException();
        }





        public JTTask MyTask { get; set; }
        ///// <summary>
        ///// Tcp
        ///// </summary>
        private Channel Channel;
        string IPED = "";

        JX.SplitData spData = new SplitData();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcp"></param>
        /// <param name="mKey"></param>
        public JTClient(JTTask MyTask, Channel channel)
        {
            this.MyTask = MyTask;
            this.Channel = channel;
            IPED = channel.RemoteHost + ":" + channel.RemotePort;
        }
        /// <summary>
        /// 协议分析
        /// </summary>
        /// <param name="bGps"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public void ProtocolUp(byte[] bGps/*,int index*/)
        {
            spData.SplitDataBy7E(bGps,
                (byte[] data, byte[] dataOriginal) =>
                {
                    if (data != null && data.Length > 11)
                    {
                        JXJTData(data);
                    }
                });
        }

        #region 协议解析

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="bGps"></param>
        /// <remarks></remarks>
        public void JXJTData(byte[] bGps)
        {
            if (bGps.Length > 11 && CheckHelper.CheckXOR(bGps, 0, bGps.Length - 1) == bGps[bGps.Length - 1])//效验通过
            {
                var head = JTHeader.NewEntity(bGps);
                var item = GetChejiFromList(head.Sim);
                if (item == null)
                {
                    item = JTCheji.NewCheji(this, Channel.RemoteHost + ":" + Channel.RemotePort, head, bGps);//1、记录车辆上线**
                    if (item.IsAuthority)
                    {
                        LastSuccessTime = DateTime.Now;
                        //if (!item.IsTest)
                        //{
                        #region 移除已有连接(防止多次登录)
                        var oldcj = MyTask.GetChejiByClientPool(item.SimKey);
                        if (oldcj != null)
                        {
                            oldcj.cl.RemoveCheji(oldcj, LogOutReason.Relogin, true);//2、此时在数据库记录车辆下线
                        }
                        #endregion
                        AddChejiToList(head.Sim, item);
                        MyTask.OnChejiAdd(item);
                    }
                    else
                        item.Dispose();
                }
                else
                {
                    item.JXData(head, bGps);
                }
            }
        }

        #endregion

        public override void RemoveCheji(JTCheji nowcj, LogOutReason logOutReason, bool Force = false)
        {
            base.RemoveCheji(nowcj, logOutReason, Force);
            MyTask.OnChejiRemove(nowcj, Force);
        }
        #region 接收数据
        public void OnReceive(object sender, ChannelReceiveArg arg)
        {
            try
            {


                //CnUser.RemoteEndPoint = e.Channel.EndPoint;
                Receive2(arg.Buffer, arg.BufferOffset, arg.BufferSize);
                MyTask.LastReciveTime = DateTime.Now;

            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("DataReceive", ex);
            }
        }
        public void Receive2(byte[] Buffer, int Offset, int N)
        {
            MyTask.WriteBytesLog("[R " + IPED + "] ", Buffer, 0, N);

            LastDataTime = DateTime.Now;
            spData.SplitDataBy7E(Buffer, Offset, N,
                (byte[] data) =>
                {
                    if (data != null && data.Length > 11)
                    {
                        JXJTData(data);
                    }
                });
        }
        #endregion

        #region 下发数据

        /// <summary>
        /// 向终端发送数据
        /// </summary>
        /// <param name="CJUser"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public bool CjSend(string SimKey, byte[] Message)
        {
            return CjSendData(SimKey, Message) == SendDataState.Success;
        }
        /// <summary>
        /// 下发数据
        /// </summary>
        /// <param name="CJUser"></param>
        /// <param name="Message"></param>
        /// <returns> -1 '车辆不存在/1 '成功/2 '不在线/3 '出错</returns>
        /// <remarks></remarks>
        public SendDataState CjSendData(string SimKey, byte[] Message)
        {
            try
            {
                if (Channel.Socket == null)
                {
                    return SendDataState.OffLine;
                }
                else
                {
                    Channel.Send(Message);
                    MyTask.WriteBytesLog("[S " + IPED + "] ", Message, 0, Message.Length);
                    return SendDataState.Success;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog4("CjSend:" + ex.ToString(), LOGTYPE.ERRORD);
                return SendDataState.Error;
                //出错
            }
        }
        #endregion
        public override void CheckChejiSinge(JTCheji nowcj)
        {
            try
            {
                nowcj.CheckPack();
                nowcj.CheckSendOrderAll();
                nowcj.CheckPlatAlarmData();
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("TCPClient.CheckChejiSinge", ex);
            }
        }

        /// <summary>
        /// 检查当前连接是否超时
        /// </summary>
        /// <param name="TimeOutSec"></param>
        /// <param name="STimeOutSec"></param>
        /// <returns></returns>
        public bool CheckTimeOut(int TimeOutSec, int STimeOutSec)
        {
            return (DateTime.Now - LastDataTime).TotalSeconds > TimeOutSec
                || (CheJiList.Count == 0 && (DateTime.Now - LastSuccessTime).TotalSeconds > STimeOutSec);
        }
        public override bool TimeoutCheck()
        {
            return CheckTimeOut(MyTask.Config.SckTimeoutSec, MyTask.Config.SckTimeoutSecNoAuthorityID);
        }

        public override void Dispose()
        {
            base.Dispose();
            Channel?.Close();
            Channel?.Dispose();
            Channel = null;
        }
    }
}
