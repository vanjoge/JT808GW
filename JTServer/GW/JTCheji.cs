using JTServer.Model;
using JX;
using SQ.Base;
using SQ.Base.GW;
using SQ.Base.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace JTServer.GW
{
    public partial class JTCheji : Cheji
    {
        public override bool TimeoutCheck()
        {
            if ((DateTime.Now - LastHeart).TotalSeconds > cl.MyTask.Config.HeartTimeoutSec)
            {
                Log.WriteLog4("[" + SimKey + "] Timeout", LOGTYPE.INFO);
                return true;
            }
            return false;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (IsAuthority && DevInfo != null)
            {
                IsAuthority = false;
            }
            StopGBCheji();
        }

        #region 事件
        #endregion

        #region 初始化
        public static JTCheji NewCheji(JTClient cl, string Ip, JTHeader head, byte[] bGps)
        {
            JTCheji item = new JTCheji(cl);
            item.Ip = Ip;
            item.JXData(head, bGps);
            item.Key = item.SimKey;
            return item;
        }

        private JTCheji(JTClient cl)
        {
            this.cl = cl;
            LastHeart = DateTime.Now;
            lastRescue = DateTime.Now.AddMinutes(-6);
        }
        #endregion

        #region 属性

        public string NewAuthorityCode
        {
            get
            {
                var str = DateTime.Now.Ticks.ToString();
                return str.Substring(str.Length - 11);
            }
        }

        GBCheji gbCheji;

        public JTGPSInfo LastGpsInfo { get; set; }
        public string Ip { get; set; }

        public JTClient cl;
        /// <summary>
        /// 分包锁定
        /// </summary>
        private object _MLockPack = new object();
        /// <summary>
        /// 发送数据锁定
        /// </summary>
        private object _MLockSD = new object();
        /// <summary>
        /// 上次收到心跳时间
        /// </summary>
        public DateTime LastHeart { get; set; }

        ///// <summary>
        ///// 使用UDP进行通讯
        ///// </summary>
        ///// <remarks></remarks> 
        //public bool UsdUdp = false;
        /// <summary>
        /// 解析类
        /// </summary>
        /// <remarks></remarks>
        public JX.JTData jtdata = new JX.JTData();
        /// <summary>
        /// 下发的数据
        /// </summary>
        /// <remarks></remarks>
        public ConcurrentDictionary<UInt16, ActiveSendData> AllSendData = new ConcurrentDictionary<UInt16, ActiveSendData>();
        /// <summary>
        /// 下发的命令
        /// </summary>
        public ConcurrentDictionary<UInt16, ActiveSendOrder> AllSendOrder = new ConcurrentDictionary<UInt16, ActiveSendOrder>();
        /// <summary>
        /// 打包的数据
        /// Key 消息流水号
        /// Value 包数据
        /// </summary>
        public ConcurrentDictionary<UInt16, PackDataInfo> AllPackData = new ConcurrentDictionary<UInt16, PackDataInfo>();


        ///// <summary>
        ///// 多媒体信息
        ///// </summary>
        //ConcurrentDictionary<uint, MediaInfo> AllMediaInfo = new ConcurrentDictionary<uint, MediaInfo>();
        ///// <summary>
        ///// 上次发送的RS232数据
        ///// </summary>
        ///// <remarks></remarks>
        //public CSendData LastRS232;
        ///// <summary>
        ///// 拍照命令
        ///// </summary>
        ///// <remarks></remarks>
        //public FT.GBPhotograph Photograph;
        ///// <summary>
        ///// 照片数据文件说明
        ///// </summary>
        ///// <remarks></remarks>
        //public FT.GBPhotoHead PhotoHead;
        ///// <summary>
        ///// 照片数据
        ///// </summary>
        ///// <remarks></remarks>
        //public FT.GBPhotoData[] PhotoData;
        ///// <summary>
        ///// 已收到的最大照片数据ID
        ///// </summary>
        ///// <remarks></remarks>
        //public ushort ReceivedPID;
        ///// <summary>
        ///// 允许执行检查照片线程
        ///// </summary>
        ///// <remarks></remarks> 
        //private bool PhotoDataCheckFlag;
        //#region "解析数据"

        /// <summary>
        /// 已鉴权
        /// </summary>
        /// <remarks></remarks>
        private bool _isauthority = false;
        public bool IsAuthority
        {
            get { return _isauthority; }
            set
            {
                if (_isauthority != value)
                {
                    _isauthority = value;
                    if (value)
                    {
                        cl.MyTask.WriteLog("Sim[" + SimKey + "]上线");
                        //启动SIP服务
                        AutoStartGBCheji();
                    }
                    else
                    {
                        cl.MyTask.WriteLog("Sim[" + SimKey + "]下线");
                        StopGBCheji();
                    }
                }

            }
        }

        public DateTime AuthorityTime { get; protected set; }

        /// <summary>
        /// 车牌颜色
        /// </summary>
        public JTPlateColor PlateColor;
        /// <summary>
        /// 车牌号
        /// </summary>
        public string PlateName;
        public string SimKey { get; private set; }


        private DeviceInfo _devinfo;
        /// <summary>
        /// 设备信息
        /// </summary>
        public DeviceInfo DevInfo
        {
            get
            {
                return _devinfo;
            }
            set
            {
                //if (value != null)
                //{
                //    isSendImpulse = !cl.MyTask.Config.ShieldImpulseEnts.Contains(value.F_ENTERPRISE_CODE);
                //}
                _devinfo = value;
            }
        }
        ///// <summary>
        ///// 安装参数
        ///// </summary>
        ///// <remarks></remarks>
        //public FT.GBInstallParInfo InsPar = new FT.GBInstallParInfo();
        ///// <summary>
        ///// 定位、告警设置
        ///// </summary>
        ///// <remarks></remarks>
        //    #endregion
        //public FT.GBGpsSetting GpsSetting = new FT.GBGpsSetting();

        //新增设备ID,企业编号
        public long EnterpriseCode { get; set; }
        private object oilMassLockObject = new object();

        /// <summary>
        /// 多媒体ID列表0x0805
        /// guozh
        /// 2013-08-23
        /// [多媒体ID（终端上传）,拍照明细表主键ID]
        /// </summary>
        private ConcurrentDictionary<uint, long> DicMeidaID = new ConcurrentDictionary<uint, long>();
        /// <summary>
        /// [多媒体ID（终端上传）,多媒体信息表主键ID]
        /// </summary>
        private ConcurrentDictionary<uint, long> DicMeidaEventID = new ConcurrentDictionary<uint, long>();

        #region 报警相关

        /// <summary>
        /// 最后位置ID
        /// guozh
        /// 2013-07-12
        /// </summary>
        public long LastTrackID { get; set; }
        /// <summary>
        /// 平台报警数据
        /// </summary>
        public long PlatAlarmData { get; set; }


        #endregion

        #region 平台围栏报警

        /// <summary>
        /// 当前车辆平台围栏（平台产生报警）
        /// </summary>
        public object objPlatRulesAlarm = new object();

        #endregion

        #region OBD指令

        public ConcurrentDictionary<UInt16, long> OBDSendDic = new ConcurrentDictionary<ushort, long>();

        #endregion

        #endregion

        #region 下发数据记录
        /////// <summary>
        /////// 查询终端参数下发数据
        /////// </summary>
        ////private ActiveSendData AS_SelectConfig;
        ///// <summary>
        ///// 位置信息查询下发数据
        ///// </summary>
        //public ActiveSendOrder AS_SelectGPS { get; set; }
        /// <summary>
        /// 存储多媒体数据上传命令下发数据
        /// </summary>
        public ActiveSendOrder AS_MediaUploadCmd { get; set; }
        /// <summary>
        /// 单条多媒体信息上传命令下发数据
        /// </summary>
        public ActiveSendOrder AS_MediaUploadSingeCmd { get; set; }
        /// <summary>
        /// 录音命令下发数据
        /// </summary>
        public ActiveSendOrder AS_RecordSoundCmd { get; set; }
        /// <summary>
        /// 拍照/摄像命令下发数据
        /// </summary>
        public ActiveSendOrder AS_PhotoCmd { get; set; }
        /// <summary>
        /// 记录北斗查询终端属性
        /// </summary>
        public ActiveSendOrder AS_BDProperties { get; set; }
        /// <summary>
        /// 锁车指令ID
        /// 2013-09-24
        /// guozh
        /// </summary>
        public long LockVehicleOrderID { get; set; }
        ///// <summary>
        ///// 行使记录仪上次下发数据
        ///// </summary>
        //public ActiveSendOrder AS_DRCmd { get; set; }
        /// <summary>
        /// 最后一次发送救援的时间
        /// </summary>
        public DateTime lastRescue { set; get; }
        #endregion


        JTPData PackageSQ(byte CmdVal, byte[] Data)
        {
            var sqAnswer = new JTSQInfo
            {
                CmdVal = CmdVal,
                List = new List<byte[]>(1)
            };
            sqAnswer.List.Add(Data);
            return jtdata.Package(0X8F80, SimKey, sqAnswer.GetBinaryData());
        }

        #region 下发

        /// <summary>
        /// 下发文本消息
        /// </summary>
        /// <param name="Flag"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public bool SendTextMsg(byte Flag, string Text)
        {
            var jtpd = jtdata.Package(0x8300, SimKey,
                new JTSendTextMsg
                {
                    Flag = (JTTextFlag)Flag,
                    TextInfo = Text
                }.GetBinaryData());

            return SendData_Active(jtpd) == SendDataState.Success;
        }

        #region 公共

        /// <summary>
        /// 发送离线指令
        /// </summary>
        private void SendOfflineCmds()
        {
            if (cl.MyTask.dicOfflineCmds.TryRemove(SimKey, out var dit))
            {
                var cmds = dit.Values.ToArray();
                foreach (var cmd in cmds)
                {
                    var jtpd = jtdata.Package(cmd.MsgId, SimKey, cmd.JTData.GetBinaryData());
                    SendData_Active(jtpd);
                }
            }
        }
        /// <summary>
        /// 平台通用应答
        /// </summary>
        /// <param name="head"></param>
        /// <param name="Result"></param>
        /// <returns></returns>
        private bool SendDefaultAnswer(JTHeader head, JTAnswer Result = JTAnswer.Success, bool WithoutPD = false)
        {
            if (cl.MyTask.Config.CanRegSecond && head.Sub && !WithoutPD)
            {
                return true;
            }
            else
            {
                return SendDefaultAnswer(head.SerialNumber, head.MsgId, Result);
            }
        }
        /// <summary>
        /// 平台通用应答
        /// </summary>
        /// <param name="ASerial">应答流水号</param>
        /// <param name="AID">应答ID</param>
        /// <param name="Result">结果</param>
        /// <returns></returns>
        private bool SendDefaultAnswer(UInt16 ASerial, UInt16 AID, JTAnswer Result)
        {
            var lst = jtdata.Package(0x8001, SimKey,
                new JTAnswerInfo
                {
                    AID = AID,
                    ASerial = ASerial,
                    Result = Result
                }.GetBinaryData()
                );

            return SendAnswer(lst);
        }
        /// <summary>
        /// 发送应答
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SendAnswer(JTPData data)
        {
            foreach (var item in data)
            {
                if (!cl.CjSend(item.Head.Sim, item.Data.ToArray()))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 监管平台主动下发数据(消息格式)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="OrderSendId"></param>
        /// <param name="ExtendedProperties"></param>
        /// <param name="SendByMonitor"></param>
        /// <returns></returns>
        public bool SendData_ActiveMon(JTPData data, long OrderSendId = -1, Dictionary<string, object> ExtendedProperties = null)
        {
            return SendData_Active(data, OrderSendId, ExtendedProperties, true) == SendDataState.Success;
        }
        /// <summary>
        /// 主动下发数据(消息格式)
        /// </summary>
        /// <param name="data">消息格式二进制数据</param>
        /// <param name="OrderSendId">命令发送编号(-1表示非命令数据)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public SendDataState SendData_Active(JTPData data, long OrderSendId = -1, Dictionary<string, object> ExtendedProperties = null, bool SendByMonitor = false, Int64 OrderId = -1, bool IsForceOrder = false, Model.Force_Command FOrder = null)
        {
            lock (_MLockSD)
            {
                ActiveSendOrder order = new ActiveSendOrder(data.FirstHead, OrderSendId, ExtendedProperties, SendByMonitor);
                order.OrderId = OrderId;
                order.UserObject = data.UserObj;
                if (IsForceOrder)
                {
                    order.IsForceOrder = true;
                    order.FOrder = FOrder;
                }
                AllSendOrder[order.FirstHead.SerialNumber] = order;
                ////如果为RS232数据 则记录上次发送RS232数据
                //if (cdata.IsRS232)
                //{
                //    LastRS232 = cdata;
                //}
                //发送
                foreach (var item in data)
                {
                    ActiveSendData cdata = new ActiveSendData
                    {
                        Head = item.Head,
                        Data = item.Data.ToArray()
                    };
                    AllSendData[item.Head.SerialNumber] = cdata;
                    order.SData.Add(cdata);
                    var flag = cl.CjSendData(item.Head.Sim, item.Data.ToArray());
                    //如果不为成功则更新命令状态
                    if (flag != SendDataState.Success)
                    {
                        UpdateOrderSendBack(order, DateTime.Now, flag);
                        return flag;
                    }
                }
                if (data.FirstHead.MsgId == 0x8803)//存储的多媒体数据上传命令
                {
                    AS_MediaUploadCmd = order;
                }
                else if (data.FirstHead.MsgId == 0x8805)//单条多媒体信息上传命令
                {
                    AS_MediaUploadSingeCmd = order;
                }
                //else if (data.FirstHead.MsgId == 0x8201)//位置信息查询
                //{
                //    AS_SelectGPS = order;
                //}
                else if (data.FirstHead.MsgId == 0x8804)//录音
                {
                    AS_RecordSoundCmd = order;
                }
                else if (data.FirstHead.MsgId == 0x8801)//拍照录像
                {
                    AS_PhotoCmd = order;
                }
                else if (data.FirstHead.MsgId == 0x8107)//北斗查询终端属性
                {
                    AS_BDProperties = order;
                }
                //else if (data.FirstHead.MsgId == 0x8701 || data.FirstHead.MsgId == 0x8700)//行驶记录仪
                //{
                //    AS_DRCmd = order;
                //}
            }
            return SendDataState.Success;
        }
        /// <summary>
        /// 重发数据(消息格式)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private SendDataState RSendData(ActiveSendData item, bool IsUdp = false)
        {
            //发送记录中包含此消息编号且重试次数小于设定次数时才执行
            if (item.RTimes < cl.MyTask.Config.RTimes)
            {
                item.RTimes += 1;
                item.LastSendTime = DateTime.Now;
                return cl.CjSendData(item.Head.Sim, item.Data);

            }
            return SendDataState.Overrun;
        }
        #endregion
        #endregion

        //#region "协议解析"
        ///// <summary>
        ///// 解析国标记录仪协议
        ///// </summary>
        ///// <param name="bGps"></param>
        ///// <remarks></remarks>

        //public void JXData(byte[] bGps, bool IsUdp = false)
        //{
        //    var head = JTHeader.NewEntity(bGps);
        //    JXData(head, bGps, IsUdp);
        //} 

        #region 检查



        /// <summary>
        /// 检查所有分包情况
        /// </summary>
        public void CheckPack()
        {
            try
            {
                var keys = AllPackData.Keys.ToArray();
                foreach (var key in keys)
                {
                    if (AllPackData.ContainsKey(key))
                    {
                        lock (_MLockPack)
                        {
                            if (AllPackData.ContainsKey(key))
                            {
                                var pd = AllPackData[key];
                                if (CheckPackByPD(pd))
                                {
                                    AllPackData.Remove(GetFirstSerialNumber(pd.Head), out var rm);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("[" + SimKey + "]Cheji.CheckPack", ex);
            }
        }
        /// <summary>
        /// 检查单个分包情况
        /// </summary>
        /// <param name="pd"></param>
        /// <returns>解析完成</returns>
        private bool CheckPackByPD(PackDataInfo pd)
        {
            lock (_MLockPack)
            {
                if (pd.PackData.Count == pd.Head.PackInfo.Sum)//判断数据包数和总包数是否相等
                {
                    List<byte> lst = new List<byte>();
                    for (UInt16 i = 1; i <= pd.Head.PackInfo.Sum; i++)
                    {
                        lst.AddRange(pd.PackData[i]);
                    }
                    //解析失败时返回true防止一直解析此错误数据
                    try
                    {
                        JXJTData(pd.Head, lst.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog4Ex("CheckConn", ex);
                        return true;
                    }
                    return true;
                }
                else if ((DateTime.Now - pd.LastRTime).TotalSeconds >= cl.MyTask.Config.PackTimeoutSec)//判断是否已超时，判断条件(当前时间减去上一次收到数据时间大于60秒)
                {
                    if (cl.MyTask.Config.SubContract == 1)
                    {
                        //0x8003补传分包请求
                        if (pd.PackData.ContainsKey(1))//包含第一包
                        {
                            var bts = pd.PackData[1];
                            uint EventID = ByteHelper.Byte4ToUInt(bts[0], bts[1], bts[2], bts[3]);
                            List<UInt16> lst = new List<UInt16>();
                            bool isNotSend = true;
                            for (UInt16 i = 1; i <= pd.Head.PackInfo.Sum; i++)
                            {
                                if (!pd.PackData.ContainsKey(i))//不包含包ID时，添加到临时列表
                                {
                                    lst.Add(i);
                                    if (lst.Count == 255)//每次最多发255个补传请求包
                                    {
                                        isNotSend = false;
                                        SendAnswer(jtdata.Package(0x8003, SimKey,
                                            new JTBDSupplementSubPackage
                                            {
                                                OldASerial = pd.Head.SerialNumber,
                                                Items = lst
                                            }.GetBinaryData()));
                                        lst.Clear();

                                    }
                                }
                            }
                            if (lst.Count > 0 || isNotSend)//当发送数据列表里包含数据或者没有发送过数据
                            {
                                SendAnswer(jtdata.Package(0x8003, SimKey,
                                    new JTBDSupplementSubPackage
                                    {
                                        OldASerial = pd.Head.SerialNumber,
                                        Items = lst
                                    }.GetBinaryData()));
                            }
                            if (lst.Count == 0 && pd.RTime >= cl.MyTask.Config.RTimes)
                                return true;
                            else
                            {
                                pd.LastRTime = DateTime.Now;
                                pd.RTime++;
                            }
                        }
                    }
                    if (pd.Head.MsgId == 0x0801)//多媒体数据上传
                    {
                        if (pd.PackData.ContainsKey(1))//包含第一包
                        {
                            var bts = pd.PackData[1];
                            uint EventID = ByteHelper.Byte4ToUInt(bts[0], bts[1], bts[2], bts[3]);
                            List<UInt16> lst = new List<UInt16>();
                            bool isNotSend = true;
                            for (UInt16 i = 1; i <= pd.Head.PackInfo.Sum; i++)
                            {
                                if (!pd.PackData.ContainsKey(i))//不包含包ID时，添加到临时列表
                                {
                                    lst.Add(i);
                                    if (lst.Count == 255)//每次最多发255个补传请求包
                                    {
                                        isNotSend = false;
                                        SendAnswer(jtdata.Package(0x8800, SimKey,
                                            new JTMediaInfoAnswer
                                            {
                                                EventID = EventID,
                                                PIDList = lst
                                            }.GetBinaryData()));
                                        lst.Clear();

                                    }
                                }
                            }
                            if (lst.Count > 0 || isNotSend)//当发送数据列表里包含数据或者没有发送过数据
                            {
                                SendAnswer(jtdata.Package(0x8800, SimKey,
                                    new JTMediaInfoAnswer
                                    {
                                        EventID = EventID,
                                        PIDList = lst
                                    }.GetBinaryData()));
                            }
                            if (lst.Count == 0 && pd.RTime >= cl.MyTask.Config.RTimes)
                                return true;
                            else
                            {
                                pd.LastRTime = DateTime.Now;
                                pd.RTime++;
                            }
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 检查所有发送记录
        /// </summary>
        public void CheckSendOrderAll()
        {
            try
            {
                if (AllSendOrder.Count > 0)
                {
                    lock (_MLockSD)
                    {
                        var keys = AllSendOrder.Keys.ToArray();
                        foreach (var key in keys)
                        {
                            if (AllSendOrder.ContainsKey(key))
                            {
                                var item = AllSendOrder[key];
                                if (!item.NotCheck)//执行过CHECK不必再次执行CHECK
                                {
                                    CheckSendOrder(item);
                                    //修改CHECK变量在UpdateOrderState中，可以避免CheckSendOrderByAnswer和这里检查两次

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("[" + SimKey + "]Cheji.CheckSendOrderAll", ex);
            }
        }
        /// <summary>
        /// 检查命令发送情况 根据通用应答
        /// </summary>
        /// <param name="head"></param>
        private void CheckSendOrderByAnswer(JTHeader head)
        {
            CheckSendOrder(AllSendOrder[head.SerialNumber]);
        }
        /// <summary>
        /// 检查命令发送情况
        /// </summary>
        /// <param name="head"></param>
        private void CheckSendOrder(ActiveSendOrder so)
        {
            var ans = GetSendOrderAnswer(so);
            UpdateOrderState(so, ans);
        }
        private void UpdateOrderState(JTHeader head, SendDataState ans)
        {
            UInt16 sid = GetFirstSerialNumber(head);
            if (AllSendOrder.ContainsKey(sid))
                UpdateOrderState(AllSendOrder[sid], ans);
        }
        /// <summary>
        /// 更新命令相关
        /// </summary>
        /// <param name="head"></param>
        /// <param name="ans"></param>
        private void UpdateOrderState(ActiveSendOrder so, SendDataState ans)
        {
            OrderFlag flag = OrderFlag.Remove;
            if (so.FirstHead.MsgId == 0x8F80)
            {
                if (so.FirstHead.MsgChildID == 0xFF)
                {
                    flag = OrderFlag.StopRSend; //防止通用应答移除需业务应答的数据
                }
            }
            else
            {
                flag = cl.MyTask.Config.NowRemove.Contains(so.FirstHead.MsgId) ? OrderFlag.Skip : OrderFlag.Remove;//防止通用应答移除需业务应答的数据
            }
            if (ans == SendDataState.Success)
            {
                UpdateOrderSendBack(so, DateTime.Now, SendDataState.Success, flag);
                //SendToMon_PhotoAnswer(so, JTSPhotoRspFlag.FinishPhotoTransmission);//=809Old=//
                //SendToMon_VoiceCallAnswer(so, JTSUpCtrlMsgMonitorVehicleAckResult.Success);//=809Old=//
                //SendToMon_MonitorAnswer(so, JTSUpCtrlMsgEmergencyMonitoringAckResult.Received);//=809Old=//
                //SendToMon_MsgTextAnswer(so, JTSUpCtrlMsgTextInfoAckResult.Success);//=809Old=//

            }
            else if (ans == SendDataState.Unavailable)
            {
                UpdateOrderSendBack(so, DateTime.Now, SendDataState.Unavailable, flag);
                //SendToMon_PhotoAnswer(so, JTSPhotoRspFlag.USupportPhoto);//=809Old=//
                //SendToMon_VoiceCallAnswer(so, JTSUpCtrlMsgMonitorVehicleAckResult.Failure);//=809Old=//
                //SendToMon_MonitorAnswer(so, JTSUpCtrlMsgEmergencyMonitoringAckResult.Other);//=809Old=//
                //SendToMon_MsgTextAnswer(so, JTSUpCtrlMsgTextInfoAckResult.Failure);//=809Old=//

                if (so.IsForceOrder)//如果是强制指令更新Redis发送次数
                {
                    so.FOrder.F_SEND_COUNT = (short)(so.FOrder.F_SEND_COUNT + 1);
                }
            }
            else if (ans != SendDataState.Default)
            {
                UpdateOrderSendBack(so, DateTime.Now, ans, flag);
                //SendToMon_PhotoAnswer(so, JTSPhotoRspFlag.Other);//=809Old=//
                //SendToMon_VoiceCallAnswer(so, JTSUpCtrlMsgMonitorVehicleAckResult.Failure);//=809Old=//
                //SendToMon_MonitorAnswer(so, JTSUpCtrlMsgEmergencyMonitoringAckResult.Other);//=809Old=//
                //SendToMon_MsgTextAnswer(so, JTSUpCtrlMsgTextInfoAckResult.Failure);//=809Old=//

                if (so.IsForceOrder)//如果是强制指令更新Redis发送次数
                {
                    so.FOrder.F_SEND_COUNT = (short)(so.FOrder.F_SEND_COUNT + 1);
                }
            }
        }


        /// <summary>
        /// 更新命令回复状态
        /// </summary>
        /// <param name="cdata">命令发送ID</param>
        /// <param name="BackTime">回复时间</param>
        /// <param name="SendFlag"></param>
        /// <param name="flag">指令处理标记</param>
        /// <remarks></remarks>
        public void UpdateOrderSendBack(ActiveSendOrder cdata, DateTime BackTime, SendDataState SendFlag = SendDataState.Default, OrderFlag flag = OrderFlag.Remove)
        {
            cdata.NotCheck = true;
            cl.MyTask.dicOrderInfo.TryGetValue(cdata.OrderId, out var OInfo);
            if (OInfo != null)
            {
                OInfo.F_SEND_FLAG = (int)SendFlag;
                OInfo.F_BACK_TIME = BackTime;
                cl.MyTask.dicOrderInfo.TryRemove(cdata.OrderId, out var rm);
            }
            //cl.MyTask.Sql_Send.UpdateOrderSendBack(cdata.OrderSendId, BackTime, SendFlag);


            if (flag != OrderFlag.Skip && SendFlag != SendDataState.Default)
            {
                lock (_MLockSD)
                {
                    while (cdata.SData.Count > 0)
                    {
                        var item = cdata.SData[0];
                        item.Answer = SendFlag;
                        AllSendData.Remove(item.Head.SerialNumber, out var rm);
                        cdata.SData.Remove(item);
                    }
                    if (flag == OrderFlag.Remove)
                    {
                        AllSendOrder.Remove(cdata.FirstHead.SerialNumber, out var rm);
                    }
                }
            }
        }
        /// <summary>
        /// 获取命令应答状态
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        private SendDataState GetSendOrderAnswer(ActiveSendOrder so)
        {
            var flag = false;//记录是否被重发
            foreach (var sd in so.SData)
            {
                if (sd.Answer == SendDataState.Success)//为成功时继续判断下一个
                    continue;
                else if (sd.Answer == SendDataState.Default)//初始时先判断是否超时，超时则重新发送
                {
                    if ((DateTime.Now - sd.LastSendTime).TotalSeconds > cl.MyTask.Config.TimeoutSec_Answer)//已超时 重新发送
                    {
                        var st = RSendData(sd);
                        //不成功时写入回执
                        if (st != SendDataState.Success && st != SendDataState.Default)
                        {
                            sd.Answer = st;
                            return st;
                        }
                    }
                    flag = true;
                    //return SendDataState.Default;
                }
                else
                    return sd.Answer;
            }
            if (flag)
            {
                return SendDataState.Default;
            }
            return SendDataState.Success;
        }

        #endregion

        #region 协议解析
        public void JXData(JTHeader head, byte[] bGps)
        {
            //被禁用SIM直接返回不响应
            if (cl.MyTask.Config.BanSims.Contains(head.Sim))
            {
                return;
            }
            jtdata.Is2019 = head.Is2019;
            if (cl.MyTask.Config.CanRegSecond)
            {
                LastHeart = DateTime.Now;
            }

            this.jtdata.SimKey = this.SimKey = head.Sim;
            bool flag = true;//是否进行解析
            #region 合并分包
            if (head.PackInfo != null)
            {
                lock (_MLockPack)
                {
                    if (head.PackInfo != null)
                    {
                        flag = false;
                        if (cl.MyTask.Config.CanRegSecond || head.PackInfo.Index != head.PackInfo.Sum)
                            SendDefaultAnswer(head, JTAnswer.Success, true);
                        var sid = GetFirstSerialNumber(head);
                        if (AllPackData.ContainsKey(sid))
                        {
                            var pd = AllPackData[sid];
                            if (pd.PackData.ContainsKey(head.PackInfo.Index))
                                pd.PackData[head.PackInfo.Index] = GetMsbBody(head, bGps);
                            else
                                pd.PackData.Add(head.PackInfo.Index, GetMsbBody(head, bGps));
                            pd.LastRTime = DateTime.Now;
                            if (CheckPackByPD(pd))
                                AllPackData.Remove(sid, out var rm);
                        }
                        else
                        {
                            PackDataInfo pd = new PackDataInfo
                            {
                                Head = new JTHeader
                                {
                                    Sim = head.Sim,
                                    DataEncryptType = head.DataEncryptType,
                                    MsgId = head.MsgId,
                                    MsgLen = head.MsgLen,
                                    SerialNumber = sid,
                                    Sub = head.Sub,
                                    PackInfo = new JTPackageInfo
                                    {
                                        Index = 1,
                                        Sum = head.PackInfo.Sum
                                    }
                                }
                            };
                            pd.PackData = new Dictionary<ushort, byte[]>(head.PackInfo.Sum);
                            pd.PackData.Add(head.PackInfo.Index, GetMsbBody(head, bGps));
                            AllPackData[sid] = pd;
                            pd.StartTime = pd.LastRTime = DateTime.Now;
                        }
                    }
                }
            }
            #endregion
            if (flag)
            {
                JXJTData(head, GetMsbBody(head, bGps));
            }
        }
        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="head">包头 为合并包时传递第一个包</param>
        /// <param name="bGps">消息体</param>
        private void JXJTData(JTHeader head, byte[] bGps)
        {
            int pv = 0;
            if (head.MsgId == 0x0100)//终端注册
            {
                //JXRegInfo_Test(head, bGps);
                JXRegInfo(head, bGps);
            }
            else if (head.MsgId == 0x0102)//鉴权
            {
                JXAuthority(head, bGps);
            }
            else if (IsAuthority || (pv == 5 && head.MsgId == 0x0200))
            {
                //DevInfo.ProtocolVersion==5 为出租车版本 出租车设备直接上报0x0200
                switch (head.MsgId)
                {
                    case 0x0001://通用应答
                        JXDefaultAnswer(head, bGps);
                        break;
                    case 0x0002://心跳
                        LastHeart = DateTime.Now;
                        SendDefaultAnswer(head);
                        break;
                    case 0x0003://终端注销
                        JXLogOut(head);
                        break;
                    case 0x0004://服务器时间
                        SendAnswer(jtdata.Package(0x8004, head.Sim,
                            new JTServerTimeV19
                            {
                                ServerTime = DateTime.UtcNow
                            }.GetBinaryData()));
                        break;
                    case 0x0104://查询终端参数设置应答
                        SendDefaultAnswer(head);
                        if (JTDParameters.BasePars == null)
                            JTDParameters.BasePars = cl.MyTask.DeviceParameters;
                        var pars = JTDParametersAnswer.NewEntity(bGps);
                        ActiveSendOrder sd = null;
                        if (AllSendOrder.TryGetValue(pars.ASerial, out sd))
                        {
                            UpdateOrderSendBack(sd, DateTime.Now, SendDataState.Success);
                        }
                        break;
                    case 0x0200://位置信息汇报
                        LastHeart = DateTime.Now;
                        if (!IsAuthority && DevInfo.ProtocolVersion == 5)
                        {
                            IsAuthority = true;
                            if (!DevInfo.IsReg)
                            {
                                DevInfo.IsReg = true;
                                DevInfo.RegisterDate = DateTime.Now;
                                DevInfo.IpAddress = Ip;
                            }
                        }
                        JXGps(head, bGps);
                        break;
                    case 0x0201://位置信息查询应答
                        JXGpsSelectAnswer(head, bGps);
                        break;
                    case 0x0301://事件报告
                        JXEventReport(head, bGps);
                        break;
                    case 0x0302://提问应答
                        JXQuestionAnswer(head, bGps);
                        break;
                    case 0x0303://信息点播/取消
                        JXInformationControl(head, bGps);
                        break;
                    case 0x0500://车辆控制应答
                        JXVehicleControlAnswer(head, bGps);
                        break;
                    case 0x0700://行驶记录仪数据上传
                        JXDRecorder(head, bGps);
                        break;
                    case 0x0701://电子运单上报
                        JXVehicleBill(head, bGps);
                        break;
                    case 0x0702://驾驶员信息上报
                        JXVehicleDriver(head, bGps);
                        break;
                    case 0x0800://多媒体事件信息上报
                        JXMediaEvents(head, bGps);
                        break;
                    case 0x0801://多媒体数据上传
                        JXMediaData(head, bGps);
                        break;
                    case 0x0802://存储的多媒体数据检索应答
                        JXMediaSelectBack(head, bGps);
                        break;
                    case 0x0900://数据上行透传
                        JXTransparentInfo(head, bGps);
                        break;
                    case 0x0901://数据压缩上报
                        SendDefaultAnswer(head, JTAnswer.Success);
                        break;
                    case 0x0A00://终端RSA公钥
                        SendDefaultAnswer(head, JTAnswer.Success);
                        break;
                    case 0x0F10://盲区补传数据(808B)
                        LastHeart = DateTime.Now;
                        JXGpsBlindPatch(head, bGps);
                        break;
                    case 0x0F15://程序版本信息汇报(808B)
                        JXProgramVersion(head, bGps);
                        break;
                    case 0x0107://查询终端属性应答(北斗)
                        JXTerminalPropertiesAnswer(head, bGps);
                        break;
                    case 0x0108://终端升级结果通知(北斗)
                        JXUpgradeResult(head, bGps);
                        break;
                    case 0x0704://定位数据批量上传(北斗)
                        JTPositionData(head, bGps);
                        break;
                    case 0x0705://CAN总线数据上传(北斗)
                        JTCANBusDataUpload(head, bGps);
                        break;
                    case 0x0805://摄像头立即拍摄命令应答(北斗)
                        JTImmediatelyProAnswer(head, bGps);
                        break;
                    case 0x0FF1://特征系数计算推荐值上报(北斗)
                        JTPluse(head, bGps);
                        break;

                    default:
                        if (!JXJT1077(head, bGps))
                        {
                            SendDefaultAnswer(head, JTAnswer.Unavailable);
                        }
                        break;
                }
            }
        }

        #region 单个解析
        /// <summary>
        /// 通用应答
        /// </summary>
        /// <param name="bGps"></param>
        private void JXDefaultAnswer(JTHeader head, byte[] bGps)
        {
            var answer = JTAnswerInfo.NewEntity(bGps);
            if (AllSendData.ContainsKey(answer.ASerial))
            {
                var sd = AllSendData[answer.ASerial];
                //if (answer.Result == JTAnswer.Success)
                //{
                //    sd.Answer = SendDataState.Success;
                //    CheckSendOrder(sd.Head);
                //}
                //else if (answer.Result == JTAnswer.Unavailable)
                //{
                //    sd.Answer = SendDataState.Unavailable;
                //    UpdateOrderState(sd.Head, SendDataState.Unavailable);
                //}
                //else
                //{
                //    //重发数据
                //    var st = RSendData(sd);
                //    //不成功时写入回执 成功时等待下次回执
                //    if (st != SendDataState.Success)
                //    {
                //        sd.Answer = st;
                //        UpdateOrderState(sd.Head, st);
                //    }
                //}
                switch (answer.Result)
                {
                    case JTAnswer.Success:
                        sd.Answer = SendDataState.Success;
                        CheckSendOrderByAnswer(sd.Head);
                        break;
                    case JTAnswer.Fail:
                        sd.Answer = SendDataState.Error;
                        UpdateOrderState(sd.Head, sd.Answer);
                        break;
                    case JTAnswer.Wrong:
                        sd.Answer = SendDataState.Error;
                        UpdateOrderState(sd.Head, sd.Answer);
                        break;
                    case JTAnswer.Unavailable:
                        sd.Answer = SendDataState.Unavailable;
                        UpdateOrderState(sd.Head, sd.Answer);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 解析注销
        /// </summary>
        /// <param name="head"></param>
        private void JXLogOut(JTHeader head)
        {
            if (DevInfo != null)
            {
                DevInfo.LogoutDate = DateTime.Now;
                DevInfo.IsReg = false;
                DevInfo.Online = false;

                SendDefaultAnswer(head, JTAnswer.Success);

                //鉴权前允许注销时需要从在线设备列表中寻找实例，再移除
                var nowcj = cl.MyTask.GetChejiByClientPool(head.Sim);
                if (nowcj != null)
                {
                    nowcj.cl.RemoveCheji(nowcj, SQ.Base.GW.LogOutReason.LogOut);
                }
            }
            else
            {
                //终端不存在 发送注销失败记录
                SendDefaultAnswer(head, JTAnswer.Fail);
            }
            //}
        }
        /// <summary>
        /// 解析鉴权信息
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JXAuthority(JTHeader head, byte[] bGps)
        {
            string AuthorityID;
            if (head.Is2019)
            {
                var jtAuthorityv19 = JTAuthorityV19.NewEntity(bGps);
                AuthorityID = jtAuthorityv19.AuthorityID;
            }
            else
            {
                var jtAuthority = JTAuthority.NewEntity(bGps);
                AuthorityID = jtAuthority.AuthorityID;
            }

            if (cl.MyTask.dic809VehicleBySim.TryGetValue(head.Sim, out var dev))
            {
                DevInfo = dev;
                DevInfo.IpAddress = Ip;
                PlateColor = DevInfo.F_PLATE_COLOR;
                PlateName = DevInfo.VehcileName;
            }
            else
                DevInfo = new DeviceInfo()
                {
                    IsReg = true,
                    Sim = head.Sim,
                    RegisterDate = DateTime.Now,
                    IpAddress = Ip,
                    AuthorityID = AuthorityID
                };
            //鉴权成功
            if (SendDefaultAnswer(head, JTAnswer.Success))
            {
                IsAuthority = true;
                AuthorityTime = LastHeart = DateTime.Now;
                EnterpriseCode = 0;
                SendAnswer(jtdata.Package(0x9003, head.Sim));
                
                SendOfflineCmds();
            }
            else
            {
                IsAuthority = false;
            }
        }
        /// <summary>
        /// 解析注册信息
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JXRegInfo(JTHeader head, byte[] bGps)
        {
            JTRegInfo reg;
            //终端注册解析 
            if (head.Is2019)
            {
                reg = JTRegInfoV19.NewEntity<JTRegInfoV19>(bGps);
            }
            else
            {
                reg = JTRegInfo.NewEntity(bGps);
            }

            DevInfo = new DeviceInfo
            {
                Sim = head.Sim,
                F_PLATE_COLOR = reg.PlateColor,
                VehcileName = reg.PlateName,
                AuthorityID = NewAuthorityCode,
                CityID = reg.CityID.ToString(),
                MakerID = reg.MakerID,
                ProvinceID = reg.ProvinceID.ToString(),
                CDeviceID = reg.CDeviceID,
                CDeviceType = reg.CDeviceType

            };
            PlateName = reg.PlateName;
            DevInfo.F_PLATE_COLOR = PlateColor = reg.PlateColor;

            cl.MyTask.dic809Vehicle[reg.PlateName + "_" + (byte)reg.PlateColor] = DevInfo;
            cl.MyTask.dic809VehicleBySim[head.Sim] = DevInfo;

            //下发注册应答
            SendAnswer(jtdata.Package(0x8100, head.Sim, new JTRegInfoAnswer
            {
                AuthorityID = DevInfo.AuthorityID,
                MsgSerialNumber = head.SerialNumber,
                RegResults = JTRegResults.Success
            }.GetBinaryData()));

        }

        /// <summary>
        /// GPS信息
        /// </summary>
        /// <param name="bGps"></param>
        /// <returns></returns>
        private void JXGps(JTHeader head, byte[] bGps)
        {
            var gps = JTGPSInfo.NewEntity(bGps);
            LastGpsInfo = gps;
            SendDefaultAnswer(head);
        }

        /// <summary>
        /// 盲区补传数据
        /// </summary>
        /// <param name="bGps"></param>
        /// <returns></returns>
        private void JXGpsBlindPatch(JTHeader head, byte[] bGps)
        {

            SendDefaultAnswer(head);
        }

        /// <summary>
        /// 解析位置查询应答
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JXGpsSelectAnswer(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
            var gpsans = JTGPSSelectAnswer.NewEntity(bGps);
            var AS_SelectGPS = AllSendOrder[gpsans.ASerial];
            UpdateOrderSendBack(AS_SelectGPS, DateTime.Now, SendDataState.Success);
        }
        /// <summary>
        /// 车辆控制应答
        /// </summary>
        /// <param name="bGps"></param>
        private void JXVehicleControlAnswer(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
            var vcAnswer = JTVehicleControlAnswer.NewEntity(bGps);
            var asd = AllSendOrder[vcAnswer.ASerial];
            UpdateOrderSendBack(asd, DateTime.Now, SendDataState.Success);
        }
        /// <summary>
        /// 解析多媒体信息
        /// </summary>
        /// <param name="bGps"></param>
        private void JXMediaData(JTHeader head, byte[] bGps)
        {
            var media = JTMediaData.NewEntity(bGps, ProtocolVersion: GetNewsIDVersion("0x0801"));
            SendAnswer(jtdata.Package(0x8800, SimKey,
                                new JTMediaInfoAnswer
                                {
                                    EventID = media.EventID,
                                }.GetBinaryData()));

        }
        /// <summary>
        /// 存储的多媒体检索应答
        /// </summary>
        /// <param name="bGps"></param>
        private void JXMediaSelectBack(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
            var bmedia = JTMediaSelectBack.NewEntity(bGps, ProtocolVersion: GetNewsIDVersion("0x0802"));
            var sd = AllSendOrder[bmedia.ASerial];

            UpdateOrderSendBack(sd, DateTime.Now, SendDataState.Success);
        }
        /// <summary>
        /// 解析运单信息
        /// </summary>
        /// <param name="bGps"></param>
        private void JXVehicleBill(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }

        public JTVehicleDriver DriverSingModel;

        /// <summary>
        /// 解析司机信息
        /// </summary>
        /// <param name="bGps"></param>
        private void JXVehicleDriver(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }
        /// <summary>
        /// 解析媒体事件
        /// </summary>
        /// <param name="bGps"></param>
        private void JXMediaEvents(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }
        /// <summary>
        /// 解析信息点播/取消
        /// </summary>
        /// <param name="bGps"></param>
        private void JXInformationControl(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }
        /// <summary>
        /// 解析提问应答
        /// </summary>
        /// <param name="bGps"></param>
        private void JXQuestionAnswer(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
            var ans = JTQuestionAnswer.NewEntity(bGps);
            if (AllSendOrder.ContainsKey(ans.ASerial))
            {
                var sd = AllSendOrder[ans.ASerial];
                UpdateOrderSendBack(sd, DateTime.Now, SendDataState.Success, OrderFlag.StopRSend);
            }
        }
        /// <summary>
        /// 解析事件报告
        /// </summary>
        /// <param name="bGps"></param>
        private void JXEventReport(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }
        private void JXDRecorder(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
            var dr = JTDRData.NewEntity(bGps);
            if (AllSendOrder.ContainsKey(dr.ASerial))
            {
                var AS_DRCmd = AllSendOrder[dr.ASerial];
                if (dr.IsError)
                {
                    UpdateOrderSendBack(AS_DRCmd, DateTime.Now, SendDataState.Error);
                }
                else
                {
                    //更新回复命令
                    UpdateOrderSendBack(AS_DRCmd, DateTime.Now, SendDataState.Success, OrderFlag.StopRSend);
                }
            }
        }
        /// <summary>
        /// 数据上行透传
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JXTransparentInfo(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head, JTAnswer.Success);
        }
        /// <summary>
        /// 程序版本信息汇报
        /// guozh/2012-01-10
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JXProgramVersion(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }

        /// <summary>
        /// 查询终端属性应答(北斗）
        /// guozh
        /// 2013-04-18
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JXTerminalPropertiesAnswer(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
            if (AS_BDProperties != null)
                UpdateOrderSendBack(AS_BDProperties, DateTime.Now, SendDataState.Success);
        }

        /// <summary>
        /// 终端升级结果通知（北斗）
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JXUpgradeResult(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }

        /// <summary>
        /// 定位数据批量上传（北斗）
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JTPositionData(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }

        /// <summary>
        /// CAN总线数据上传（北斗）
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JTCANBusDataUpload(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }

        /// <summary>
        /// 摄像头立即拍摄命令应答（北斗）
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JTImmediatelyProAnswer(JTHeader head, byte[] bGps)
        {
            var UpgradeResult = JTBDImmediatelyProAnswer.NewEntity(bGps);
            SendDefaultAnswer(head);
            if (AllSendOrder.ContainsKey(UpgradeResult.ASerial))
            {
                var order = AllSendOrder[UpgradeResult.ASerial];
                ushort lremain = 0;
                if (order.UserObject != null && !ushort.TryParse(order.UserObject.ToString(), out lremain))
                {
                    lremain = 1;
                }
                int remain = lremain - UpgradeResult.Items.Count;
                if (AllSendData.ContainsKey(UpgradeResult.ASerial))
                {
                    var sd = AllSendData[UpgradeResult.ASerial];
                    //sd.
                    if (sd.Answer == SendDataState.Default)
                    {
                        if (UpgradeResult.Result == 0)
                        {
                            if (remain > 0)
                            {
                                sd.LastSendTime = DateTime.Now;
                            }
                            else
                            {
                                sd.Answer = SendDataState.Success;
                            }
                        }
                        else if (UpgradeResult.Result == 1)
                        {
                            sd.Answer = SendDataState.Error;
                        }
                        else
                        {
                            sd.Answer = SendDataState.Unavailable;
                        }
                        CheckSendOrderByAnswer(sd.Head);
                    }
                }
                if (UpgradeResult.Result == 0)
                {
                    if (remain <= 0)
                    {
                        AllSendOrder.Remove(UpgradeResult.ASerial, out var rm);
                    }
                }
                else
                {
                    AllSendOrder.Remove(UpgradeResult.ASerial, out var rm);
                }

            }
        }

        /// <summary>
        /// 特征系数计算推荐值上报（北斗）
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        private void JTPluse(JTHeader head, byte[] bGps)
        {
            SendDefaultAnswer(head);
        }


        #endregion


        #endregion



        #region 逻辑 
        /// <summary>
        /// 获取第一包流水号
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        private static UInt16 GetFirstSerialNumber(JTHeader head)
        {
            UInt16 sid;
            if (head.PackInfo != null)
            {
                //计算出第一包的流水号
                sid = (UInt16)(head.SerialNumber - head.PackInfo.Index + 1);
            }
            else
            {
                sid = head.SerialNumber;
            }
            return sid;
        }
        /// <summary>
        /// 获取消息体
        /// </summary>
        /// <param name="head"></param>
        /// <param name="bGps"></param>
        /// <returns></returns>
        private byte[] GetMsbBody(JTHeader head, byte[] bGps)
        {
            int hlen = head.GetHeadLen();
            int len = bGps.Length - hlen - 1;
            var bts = new byte[len];
            Array.Copy(bGps, hlen, bts, 0, len);
            return bts;
        }

        #endregion

        #region 数据库相关

        /// <summary>
        /// 单位时间内平台报警
        /// </summary>
        public void CheckPlatAlarmData()
        {
        }

        /// <summary>
        /// 获取消息ID版本号
        /// </summary>
        /// <param name="NewsID"></param>
        /// <returns></returns>
        public int GetNewsIDVersion(string NewsID)
        {
            int NewsIDVersion = 0;
            return NewsIDVersion;
        }

        #endregion

        public GBDeviceSetting GetGBDeviceSetting()
        {
            return GetGBDeviceSetting(null);
        }
        public GBDeviceSetting GetGBDeviceSetting(RedisHelp.RedisHelper redisHelper)
        {
            try
            {
                redisHelper = redisHelper ?? cl.MyTask.RedisHelper;
                return redisHelper.StringGet<GBDeviceSetting>("GWGBDevConf:" + SimKey);
            }
            catch
            {
                return null;
            }
        }
        public void SaveGBDeviceSetting(GBDeviceSetting conf)
        {
            cl.MyTask.RedisHelper.StringSet("GWGBDevConf:" + SimKey, conf);
        }
        public void AutoStartGBCheji()
        {
            if (gbCheji == null)
            {
                cl.MyTask.schRedis.Add(new DefaultWorkItem<RedisHelp.RedisHelper>((tag, c) =>
                {
                    c.ThrowIfCancellationRequested();

                    if (IsAuthority)
                    {
                        var setting = GetGBDeviceSetting(tag);
                        if (setting != null && setting.Enable)
                        {
                            StartGBCheji(setting);
                        }
                    }
                }));
            }
        }
        public void StartGBCheji(GBDeviceSetting setting)
        {
            if (IsAuthority && setting.Enable)
            {
                gbCheji?.Stop(false);
                gbCheji = new GBCheji(this, setting, EnableTraceLogs: cl.MyTask.Config.ShowSipLog);
                gbCheji.Start();
            }
        }

        public void StopGBCheji()
        {
            if (gbCheji != null)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    if (gbCheji != null)
                    {
                        var tmp = gbCheji;
                        gbCheji = null;
                        tmp.Stop();
                    }
                });
            }
        }
        public string GetGBStatus()
        {
            if (gbCheji == null)
            {
                return "未启动";
            }
            else if (gbCheji.IsRegistered)
            {
                return "在线";
            }
            else
            {
                return "离线";
            }
        }
    }
}