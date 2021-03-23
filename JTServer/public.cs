using JX;
using System;
using System.Collections.Generic;
using System.Text;

namespace JTServer
{

    /// <summary>
    /// 发送状态
    /// </summary>
    public enum SendDataState
    {
        /// <summary>
        /// 初始
        /// </summary>
        Default = 0,
        /// <summary>
        /// 车辆不存在
        /// </summary>
        NoVehicle = -1,
        /// <summary>
        /// 成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 不在线
        /// </summary>
        OffLine = 2,
        /// <summary>
        /// 出错
        /// </summary>
        Error = 3,
        /// <summary>
        /// 超过重试次数
        /// </summary>
        Overrun = 4,
        /// <summary>
        /// 不支持
        /// </summary>
        Unavailable = 5
    }

    public enum OrderFlag
    {
        /// <summary>
        /// 停止重发
        /// </summary>
        StopRSend,
        /// <summary>
        /// 移除(指令生命周期结束)
        /// </summary>
        Remove,
        /// <summary>
        /// 跳过
        /// </summary>
        Skip
    }

    #region 实体类
    /// <summary>
    /// 发送数据实体类
    /// </summary>
    public class SendData
    {

        public byte[] Data { get; set; }

        public System.Net.Sockets.Socket sck { get; set; }
    }

    /// <summary>
    /// 包数据
    /// </summary>
    public class PackDataInfo
    {
        /// <summary>
        /// 包数据信息
        /// Key 包序号
        /// Value消息体
        /// </summary>
        public Dictionary<UInt16, byte[]> PackData { get; set; }
        /// <summary>
        /// 头
        /// </summary>
        public JTHeader Head { get; set; }
        /// <summary>
        /// 上一次收到数据的时间
        /// </summary>
        public DateTime LastRTime { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 重试次数
        /// </summary>
        public int RTime { get; set; }
    }


    /// <summary>
    /// 包数据
    /// </summary>
    public class PackDataInfo_SQ
    {
        /// <summary>
        /// 包数据信息
        /// Key 包序号
        /// Value消息体
        /// </summary>
        public Dictionary<UInt16, byte[]> PackData { get; set; }
        /// <summary>
        /// 头
        /// </summary>
        public JTSQHeader Head { get; set; }
        /// <summary>
        /// 上一次收到数据的时间
        /// </summary>
        public DateTime LastRTime { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 重试次数
        /// </summary>
        public int RTime { get; set; }
    }
    /// <summary>
    /// 下发的指令
    /// </summary>
    public class ActiveSendOrder
    {
        /// <summary>
        /// 命令发送ID(-1表示非命令数据)
        /// </summary>
        /// <remarks></remarks>
        public long OrderSendId { get; set; }
        /// <summary>
        /// 命令ID(-1表示非命令数据)
        /// </summary>
        public Int64 OrderId { get; set; }
        /// <summary>
        /// 上级平台下发
        /// </summary>
        public bool SendByMonitor { get; set; }
        /// <summary>
        /// 自定义用户信息的集合
        /// </summary>
        public Dictionary<string, object> ExtendedProperties { get; set; }
        /// <summary>
        /// 第一个包头
        /// </summary>
        public JTHeader FirstHead { get; set; }
        ///// <summary>
        ///// 串口部分命令
        ///// </summary>
        ///// <remarks></remarks>
        //public bool IsRS232
        //{
        //    get { return Data[15] == 0xf; }
        //}
        /// <summary>
        /// 所有发送的数据
        /// </summary>
        public List<ActiveSendData> SData { get; set; }
        /// <summary>
        /// 命令创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }
        /// <summary>
        /// 不再需要检查
        /// </summary>
        public bool NotCheck { get; set; }
        /// <summary>
        /// 用户自定义对象
        /// </summary>
        public object UserObject { set; get; }

        /// <summary>
        /// 是否强制指令
        /// </summary>
        public bool IsForceOrder { set; get; }
        /// <summary>
        /// 强制指令对象
        /// </summary>
        public Model.Force_Command FOrder { set; get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FirstHead"></param>
        /// <param name="OrderSendId">命令发送ID(-1表示非命令数据)</param>
        /// <param name="ExtendedProperties"></param>
        /// <param name="SendByMonitor"></param>
        public ActiveSendOrder(JTHeader FirstHead, long OrderSendId, Dictionary<string, object> ExtendedProperties, bool SendByMonitor)
        {
            CreateTime = DateTime.Now;
            if (ExtendedProperties == null)
            {
                ExtendedProperties = new Dictionary<string, object>();
            }
            this.ExtendedProperties = ExtendedProperties;
            this.FirstHead = FirstHead;
            this.OrderSendId = OrderSendId;
            this.SendByMonitor = SendByMonitor;
            SData = new List<ActiveSendData>();
        }
        public SendDataState GetState()
        {
            SendDataState tmp = SendDataState.Success;
            foreach (var item in SData)
            {
                if (tmp == SendDataState.Success && item.Answer == SendDataState.Success)//全部成功才返回成功
                {
                    continue;
                }
                else if (item.Answer == SendDataState.Default)//有一个Default 其他都是成功返回Default
                {
                    tmp = SendDataState.Default;
                }
                else//其他错误直接返回
                {
                    return item.Answer;
                }
            }
            return tmp;
        }
    }


    /// <summary>
    /// 下发的指令
    /// </summary>
    public class ActiveSendOrder_SQ
    {
        /// <summary>
        /// 命令发送ID(-1表示非命令数据)
        /// </summary>
        /// <remarks></remarks>
        public long OrderSendId { get; set; }
        /// <summary>
        /// 命令ID(-1表示非命令数据)
        /// </summary>
        public long OrderId { get; set; }
        /// <summary>
        /// 自定义用户信息的集合
        /// </summary>
        public Dictionary<string, object> ExtendedProperties { get; set; }
        /// <summary>
        /// 第一个包头
        /// </summary>
        public JTSQHeader FirstHead { get; set; }
        /// <summary>
        /// 所有发送的数据
        /// </summary>
        public List<ActiveSendData_SQ> SData { get; set; }
        /// <summary>
        /// 命令创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FirstHead"></param>
        /// <param name="OrderSendId">命令发送ID(-1表示非命令数据)</param>
        /// <param name="ExtendedProperties"></param>
        /// <param name="SendByMonitor"></param>
        public ActiveSendOrder_SQ(JTSQHeader FirstHead, long OrderSendId, Dictionary<string, object> ExtendedProperties)
        {
            CreateTime = DateTime.Now;
            if (ExtendedProperties == null)
            {
                ExtendedProperties = new Dictionary<string, object>();
            }
            this.ExtendedProperties = ExtendedProperties;
            this.FirstHead = FirstHead;
            this.OrderSendId = OrderSendId;
            SData = new List<ActiveSendData_SQ>();
        }

    }

    /// <summary>
    /// 主动发送数据
    /// </summary>
    public class ActiveSendData
    {
        public ActiveSendData()
        {
            LastSendTime = DateTime.Now;
        }
        /// <summary>
        /// 头
        /// </summary>
        public JTHeader Head { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <remarks></remarks>
        //public JTPDataItem JTPData { get; set; }
        /// <summary>
        /// 已重试次数
        /// </summary>
        /// <remarks></remarks>
        public int RTimes { get; set; }
        /// <summary>
        /// 回复状态
        /// </summary>
        /// <remarks></remarks>
        public SendDataState Answer { get; set; }
        /// <summary>
        /// 上次发送数据的时间
        /// </summary>
        public DateTime LastSendTime { get; set; }
    }

    /// <summary>
    /// 主动发送数据
    /// </summary>
    public class ActiveSendData_SQ
    {
        public ActiveSendData_SQ()
        {
            LastSendTime = DateTime.Now;
        }
        /// <summary>
        /// 头
        /// </summary>
        public JTSQHeader Head { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }
        /// <summary>
        /// 已重试次数
        /// </summary>
        /// <remarks></remarks>
        public int RTimes { get; set; }
        /// <summary>
        /// 回复状态
        /// </summary>
        /// <remarks></remarks>
        public SendDataState Answer { get; set; }
        /// <summary>
        /// 上次发送数据的时间
        /// </summary>
        public DateTime LastSendTime { get; set; }
    }

    /// <summary>
    /// 媒体信息
    /// </summary>
    public class MediaInfo
    {
        public MediaInfo()
        {
            StartTime = DateTime.Now;
        }
        /// <summary>
        /// 事件ID
        /// </summary>
        public UInt32 EventID { get; set; }
        /// <summary>
        /// 多媒体类型
        /// </summary>
        public JTMediaType MediaType { get; set; }
        /// <summary>
        /// 多媒体格式编码
        /// </summary>
        public JTMediaEncoding MediaEncoding { get; set; }
        /// <summary>
        /// 事件类型
        /// </summary>
        public JTEventType EventType { get; set; }
        /// <summary>
        /// 通道
        /// </summary>
        public Byte Channel { get; set; }
        /// <summary>
        /// 事件产生时间
        /// </summary>
        public DateTime EventTime { get; set; }
        /// <summary>
        /// 数据总包数
        /// </summary>
        public UInt32 Count { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public Dictionary<UInt32, byte[]> Data { get; set; }
        /// <summary>
        /// 上一次收到数据的时间
        /// </summary>
        public DateTime LastRTime { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MonSendFailGps
    {
        #region 属性
        /// <summary>
        /// 
        /// </summary>
        public Int32 RowID { get; set; }
        /// <summary>
        /// 跟踪ID
        /// </summary>
        public Int64 TrackID { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public String PlateName { get; set; }
        /// <summary>
        /// 车牌颜色
        /// </summary>
        public Int16 PlateColor { get; set; }

        #endregion

        #region 初始化数据
        public static MonSendFailGps NewItem(SQ.Base.MyDataReader reader)
        {
            var item = new MonSendFailGps();
            //
            item.RowID = reader.GetInt32("RowID");
            //跟踪ID
            item.TrackID = reader.GetInt64("TrackID");
            //车牌号
            item.PlateName = reader.GetString("PlateName");
            //车牌颜色
            item.PlateColor = reader.GetInt16("PlateColor");

            return item;
        }
        #endregion
    }
    [Serializable]
    public class SupplementGpsModel
    {
        /// <summary>
        /// 未上报ID
        /// </summary>
        public int RowID { private set; get; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string PlateName { private set; get; }
        /// <summary>
        /// 车牌颜色
        /// </summary>
        public Int16 PlateColor { private set; get; }

        /// <summary>
        /// GPS数据
        /// </summary>
        public JTSVehicleGpsMsgDatas Gnss { private set; get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reader"></param>
        public SupplementGpsModel(SQ.Base.MyDataReader reader)
        {
            RowID = reader.GetInt32("RowID");
            //车牌号
            PlateName = reader.GetString("PlateName");
            //车牌颜色
            PlateColor = reader.GetInt16("PlateColor");

            var lon = reader.GetInt32("F_LONGITUDE");
            var lat = reader.GetInt32("F_LATITUDE");
            var GpsEncrypt = (int)reader.GetInt16("F_GpsEncrypt");
            var tmpstate = reader.GetInt64("F_GPSSTATE");
            var GPSSTATE = (uint)(tmpstate & 0xFFFFFFFF);

            Gnss = new JTSVehicleGpsMsgDatas
            {
                ENCRYPT = GpsEncrypt == 1 ? JTSEncrypt.Encrypt : JTSEncrypt.NoneEncrypt,
                datetime = reader.GetDateTime("F_GPS_TIME"),
                LON = (uint)Math.Abs(lon),
                LAT = (uint)Math.Abs(lat),
                VEC1 = (UInt16)reader.GetInt16("F_SPEED"),
                VEC2 = (UInt16)reader.GetInt16("F_DSPEED"),
                VEC3 = (UInt32)reader.GetInt32("F_MILEAGE"),
                DIRECTION = (UInt16)reader.GetInt16("F_DIRECTION"),
                ALTITUDE = (UInt16)reader.GetInt16("F_HIGH"),
                STATE = new JTGPSStateInfo(GPSSTATE),// GetGpsStatus(TrackID),
                ALARM = (JTAlarmType)reader.GetInt32("F_ALARM_DATA")
            };
        }
    }
    public class YbHistoryGps
    {
        public List<int> RemoveIds { set; get; }
        public JTSYbUpExgMsgHistoryLocation GpsData { set; get; }
        public MonSendFailGps FailGpsId { set; get; }
    }

    public class HistoryGps
    {
        public List<int> RemoveIds { set; get; }
        public JTSUpExgMsgHistoryLocation GpsData { set; get; }
        public MonSendFailGps FailGpsId { set; get; }
    }

    public class CRVehicle
    {
        /// <summary>
        /// 车牌号
        /// </summary>
        public string VehicleName { set; get; }
        /// <summary>
        /// 车牌颜色
        /// </summary>
        public JTPlateColor PlateColor { set; get; }

        public static CRVehicle NewItem(SQ.Base.MyDataReader reader)
        {
            var item = new CRVehicle();
            item.VehicleName = reader.GetString("F_VIN");
            item.PlateColor = (JTPlateColor)reader.GetByte("F_VEHICLE_COLOR");
            return item;
        }

    }
    /// <summary>
    /// 未发送成功的运单信息
    /// </summary>
    public class NotSendBillInfo
    {
        #region 属性
        public string Name { get; set; }

        public byte PlateColor { get; set; }

        public string BillInfo { get; set; }
        #endregion

        #region 初始化数据
        public static NotSendBillInfo NewItem(SQ.Base.MyDataReader reader)
        {
            var item = new NotSendBillInfo();
            //车牌号
            item.Name = reader.GetString("Name");
            //车牌颜色
            item.PlateColor = reader.GetByte("PlateColor");
            //运单信息
            item.BillInfo = reader.GetString("BillInfo");

            return item;
        }
        #endregion
    }
    /// <summary>
    /// 未发送成功的报警处理结果
    /// </summary>
    [Serializable]
    public class NotSendAuditAlarmInfo
    {
        #region 属性
        /// <summary>
        /// 报警处理ID（主键）
        /// </summary>
        public Int64 ID { set; get; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 车牌颜色
        /// </summary>
        public byte PlateColor { get; set; }
        /// <summary>
        /// 报警类型
        /// </summary>
        public Byte AlarmType { get; set; }
        /// <summary>
        /// 处理状态
        /// </summary>
        public byte Status { set; get; }
        /// <summary>
        /// 报警编号
        /// </summary>
        public Int64 AlarmId { get; set; }
        #endregion

        #region 初始化数据
        public static NotSendAuditAlarmInfo NewItem(SQ.Base.MyDataReader reader)
        {

            var item = new NotSendAuditAlarmInfo();

            item.ID = reader.GetInt64("F_ID");
            item.Name = reader.GetString("F_PLATE_CODE");
            item.PlateColor = (byte)reader.GetInt16("F_VEHICLE_COLOR");
            item.AlarmType = (byte)reader.GetInt64("F_ALARM_TYPE_ID");
            //报警编号
            item.AlarmId = reader.GetInt64("F_ALARM_HISTORY_ID");
            //报警类型
            item.Status = (byte)reader.GetInt16("F_STATUS");
            //车牌颜色
            return item;
        }
        #endregion
    }

    [Serializable]
    public class T_SysVehicle
    {
        #region 属性
        /// <summary>
        /// 车辆标识号
        /// </summary>
        public long VehicleID { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 车辆颜色
        /// </summary>
        public JTPlateColor PlateColor { get; set; }

        /// <summary>
        /// 企业编码
        /// </summary>
        public String EnterpriseCode { get; set; }

        #endregion

        #region 初始化数据
        public static T_SysVehicle NewItem(SQ.Base.MyDataReader reader)
        {
            var item = new T_SysVehicle();
            //车辆标识号
            item.VehicleID = reader.GetInt64("F_ID");
            //车牌号
            if (!reader.IsDBNull("F_PLATE_CODE"))
                item.Name = reader.GetString("F_PLATE_CODE");
            //车牌颜色
            if (!reader.IsDBNull("F_PLATE_COLOR"))
                item.PlateColor = (JTPlateColor)Convert.ToByte(reader.GetString("F_PLATE_COLOR"));
            if (!reader.IsDBNull("F_ENTERPRISE_CODE"))
            {
                item.EnterpriseCode = reader.GetString("F_ENTERPRISE_CODE");
            }
            else
                item.EnterpriseCode = "";
            return item;
        }
        #endregion

    }

    //[Serializable]
    //public class T_SysVehicle
    //{
    //    #region 属性
    //    /// <summary>
    //    /// 车辆标识号
    //    /// </summary>
    //    public Int64 F_ID { get; set; }
    //    /// <summary>
    //    /// 车牌号
    //    /// </summary>
    //    public String F_PLATE_CODE { get; set; }
    //    /// <summary>
    //    /// 车牌颜色
    //    /// </summary>
    //    public JTPlateColor F_PLATE_COLOR { get; set; }
    //    #endregion

    //    #region 初始化数据
    //    public static T_SysVehicle NewItem(SQ.Base.MyDataReader reader)
    //    {
    //        var item = new T_SysVehicle();
    //        //车辆标识号
    //        item.F_ID = reader.GetInt32("F_ID");
    //        //车牌号
    //        if (!reader.IsDBNull("F_PLATE_CODE"))
    //            item.F_PLATE_CODE = reader.GetString("F_PLATE_CODE");
    //        //车牌颜色
    //        if (!reader.IsDBNull("F_PLATE_COLOR"))
    //            item.F_PLATE_COLOR = (JTPlateColor)reader.GetByte("F_PLATE_COLOR");
    //        return item;
    //    }
    //    #endregion

    //}


    ///// <summary>
    ///// 
    ///// </summary>
    //[Serializable]
    //public class DeviceInfo
    //{
    //    #region 属性
    //    /// <summary>
    //    /// 终端主键
    //    /// </summary>
    //    public Int32 DeviceID { get; set; }
    //    /// <summary>
    //    /// 省域ID
    //    /// </summary>
    //    public Int32 ProvinceID { get; set; }
    //    /// <summary>
    //    /// 市域ID
    //    /// </summary>
    //    public Int32 CityID { get; set; }
    //    /// <summary>
    //    /// 制造商ID
    //    /// </summary>
    //    public string MakerID { get; set; }
    //    /// <summary>
    //    /// 终端型号
    //    /// </summary>
    //    public string CDeviceType { get; set; }
    //    /// <summary>
    //    /// 厂商终端ID
    //    /// </summary>
    //    public string CDeviceID { get; set; }
    //    /// <summary>
    //    /// 车辆标识号
    //    /// </summary>
    //    public Int32? VehicleID { get; set; }
    //    /// <summary>
    //    /// 已注册
    //    /// </summary>
    //    public Boolean IsReg { get; set; }
    //    /// <summary>
    //    /// 鉴权码
    //    /// </summary>
    //    public String AuthorityID { get; set; }
    //    /// <summary>
    //    /// RSA公钥[e,n]中的e
    //    /// </summary>
    //    public Int32 RSAE { get; set; }
    //    /// <summary>
    //    /// RSA公钥[e,n]中的n
    //    /// </summary>
    //    public Int32 RSAN { get; set; }
    //    /// <summary>
    //    /// 手机卡号
    //    /// </summary>
    //    public String Sim { get; set; }
    //    /// <summary>
    //    /// 协议版本
    //    /// </summary>
    //    public int ProtocolVersion { get; set; }
    //    /// <summary>
    //    /// 终端厂商所在地行政区划代码
    //    /// </summary>
    //    public string MakerPCID { get; set; }
    //    /// <summary>
    //    /// 停用状态
    //    /// </summary>
    //    public bool Pause;
    //    #endregion 

    /// <summary>
    /// 设备信息
    /// </summary>
    [Serializable]
    public class DeviceInfo
    {
        #region 属性
        /// <summary>
        /// 终端ID
        /// </summary>
        public Int64 DeviceID { get; set; }
        /// <summary>
        /// 终端Code
        /// </summary>
        public String CDeviceID { get; set; }
        /// <summary>
        /// 省域ID
        /// </summary>
        public String ProvinceID { get; set; }
        /// <summary>
        /// 市域ID
        /// </summary>
        public String CityID { get; set; }
        /// <summary>
        /// 终端厂商所在地行政区划代码
        /// </summary>
        public String MakerPCID { get; set; }
        /// <summary>
        /// 制造商ID
        /// </summary>
        public String MakerID { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public String VehcileName { set; get; }

        /// <summary>
        /// 终端型号
        /// </summary>
        public String CDeviceType { get; set; }
        /// <summary>
        /// 车牌颜色
        /// </summary>
        public JTPlateColor F_PLATE_COLOR { get; set; }
        /// <summary>
        /// 手机号F_MOBILE_CODE
        /// </summary>
        public String Sim { get; set; }
        /// <summary>
        /// 车辆IDF_VEHICLE_ID 车辆标识号
        /// </summary>
        public Int64? VehicleID { get; set; }
        /// <summary>
        /// 企业编号
        /// </summary>
        public long F_ENTERPRISE_CODE { get; set; }
        /// <summary>
        /// 已注册
        /// </summary>
        public Boolean IsReg { get; set; }
        /// <summary>
        /// 鉴权码
        /// </summary>
        public String AuthorityID { get; set; }
        ///// <summary>
        ///// RSA公钥[e,n]中的e
        ///// </summary>
        //public Int32 RSAE { get; set; }
        ///// <summary>
        ///// RSA公钥[e,n]中的n
        ///// </summary>
        //public Int32 RSAN { get; set; }
        /// <summary>
        /// 协议版本
        /// </summary>
        public int ProtocolVersion { get; set; }
        /// <summary>
        /// 停用状态
        /// </summary>
        public bool Pause { set; get; }

        /// <summary>
        /// 终端注册ID
        /// </summary>
        public Int64? DeviceRegID { set; get; }

        /// <summary>
        /// 网关ID
        /// </summary>
        public Int64? GatewayID { set; get; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime? RegisterDate { set; get; }

        /// <summary>
        /// 终端鉴权时间
        /// </summary>
        public DateTime? AuthDate { set; get; }

        /// <summary>
        /// 注销时间
        /// </summary>
        public DateTime? LogoutDate { set; get; }

        /// <summary>
        /// IP地址
        /// </summary>
        public String IpAddress { set; get; }

        /// <summary>
        /// 端口
        /// </summary>
        public Int32 Port { set; get; }

        /// <summary>
        /// 是否在线
        /// </summary>
        public Boolean Online { set; get; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? CreateDate { set; get; }

        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? UpdateDate { set; get; }

        /// <summary>
        /// 转发组
        /// </summary>
        public List<string> TGroup { get; set; }

        public List<long> TGroupIDs { set; get; }

        public string Car_Color { set; get; }

        /// <summary>
        /// 终端使用的CAN数据配置版本
        /// </summary>
        public long CANCFGVersion { get; set; }
        /// <summary>
        /// 车辆分组ID
        /// </summary>
        public Int64 F_DP_ID { get; set; }
        /// <summary>
        /// 是否启用救援
        /// </summary>
        public bool RescueEnabled { set; get; }
        /// <summary>
        /// 救援服务商
        /// </summary>
        public short RescueServiceProvider { set; get; }

        /// <summary>
        /// 车辆VIN码
        /// </summary>
        public string VIN { get; set; }
        /// <summary>
        /// 油补信息
        /// </summary>
        public DeviceYb YbInfo { get; set; }

        #endregion

        #region 初始化数据
        public static DeviceInfo NewItem(SQ.Base.MyDataReader reader)
        {
            var item = new DeviceInfo();
            // 终端ID
            item.DeviceID = reader.GetInt64("F_ID");
            //终端Code
            if (!reader.IsDBNull("F_CODE"))
                item.CDeviceID = reader.GetString("F_CODE");
            //省域ID
            if (!reader.IsDBNull("F_PROVINCE"))
            {
                item.ProvinceID = reader.GetString("F_PROVINCE").Trim();
                if (item.ProvinceID.Length > 2)
                {
                    item.ProvinceID = item.ProvinceID.Substring(0, 2);
                }
                //区域ID
                while (item.ProvinceID.Length < 2)
                {
                    item.ProvinceID = "0" + item.ProvinceID;
                }
            }
            else
            {
                item.ProvinceID = "00";
            }
            //市域ID
            if (!reader.IsDBNull("F_CITY_ID"))
            {
                item.CityID = reader.GetString("F_CITY_ID").Trim();
                if (item.CityID.Length > 2)
                {
                    item.CityID = item.CityID.Substring(0, 2);
                }
                //区域ID
                while (item.CityID.Length < 2)
                {
                    item.CityID = "0" + item.CityID;
                }
            }
            else
            {
                item.CityID = "00";
            }
            item.MakerPCID = item.ProvinceID + item.CityID + "00";
            //if (!reader.IsDBNull("F_AREA_ID"))
            //    item.MakerPCID = reader.GetString("F_AREA_ID");
            //制造商ID
            if (!reader.IsDBNull("F_MANUFACTURER_ID"))
                item.MakerID = reader.GetString("F_MANUFACTURER_ID");
            //终端型号
            if (!reader.IsDBNull("F_EQUIPMENT_MODE"))
                item.CDeviceType = reader.GetString("F_EQUIPMENT_MODE");
            //车牌颜色
            if (!reader.IsDBNull("F_PLATE_COLOR"))
                item.F_PLATE_COLOR = (JTPlateColor)Convert.ToByte(reader.GetString("F_PLATE_COLOR"));
            if (!reader.IsDBNull("F_PLATE_CODE"))
            {
                item.VehcileName = reader.GetString("F_PLATE_CODE");
            }
            //手机号
            if (!reader.IsDBNull("F_MOBILE_CODE"))
                item.Sim = reader.GetString("F_MOBILE_CODE");
            //车辆ID
            if (!reader.IsDBNull("F_VEHICLE_ID"))
                item.VehicleID = reader.GetInt64("F_VEHICLE_ID");
            //企业编号
            //if (!reader.IsDBNull("F_ENTERPRISE_CODE"))
            //    item.F_ENTERPRISE_CODE = reader.GetString("F_ENTERPRISE_CODE");
            //else
            //    item.F_ENTERPRISE_CODE = string.Empty;
            //注册状态
            if (reader.HasCol("F_IS_LOGOUT") && !reader.IsDBNull("F_IS_LOGOUT"))
            {
                item.IsReg = reader.GetInt16("F_IS_LOGOUT") == 0;
            }
            else
            {
                item.IsReg = false;
            }
            //鉴权码
            if (reader.HasCol("F_AUTH_CODE") && !reader.IsDBNull("F_AUTH_CODE"))
                item.AuthorityID = reader.GetString("F_AUTH_CODE");
            //协议版本 F_PROTOCOL_VERSION
            if (!reader.IsDBNull("F_PROTOCOL_VERSION"))
                item.ProtocolVersion = reader.GetInt32("F_PROTOCOL_VERSION");
            //终端使用的CAN数据配置版本
            if (!reader.IsDBNull("F_CAN_CFG_VERSION"))
                item.CANCFGVersion = reader.GetInt64("F_CAN_CFG_VERSION");
            if (reader.HasCol("F_PAUSE") && !reader.IsDBNull("F_PAUSE"))
            {
                item.Pause = reader.GetInt16("F_PAUSE") == 1;
            }
            else
            {
                item.Pause = false;
            }
            if (reader.HasCol("DeviceRegID") && !reader.IsDBNull("DeviceRegID"))
            {
                item.DeviceRegID = reader.GetInt64("DeviceRegID");
            }

            if (reader.HasCol("F_GATEWAY_ID") && !reader.IsDBNull("F_GATEWAY_ID"))
            {
                item.GatewayID = reader.GetInt64("F_GATEWAY_ID");
            }

            if (reader.HasCol("F_AUTH_CODE") && !reader.IsDBNull("F_AUTH_CODE"))
            {
                item.AuthorityID = reader.GetString("F_AUTH_CODE");
            }
            if (reader.HasCol("F_REGISTER_DATE") && !reader.IsDBNull("F_REGISTER_DATE"))
            {
                item.RegisterDate = reader.GetDateTime("F_REGISTER_DATE");
            }
            if (reader.HasCol("F_AUTH_DATE") && !reader.IsDBNull("F_AUTH_DATE"))
            {
                item.AuthDate = reader.GetDateTime("F_AUTH_DATE");
            }
            if (reader.HasCol("F_LOGOUT_DATE") && !reader.IsDBNull("F_LOGOUT_DATE"))
            {
                item.LogoutDate = reader.GetDateTime("F_LOGOUT_DATE");
            }
            if (reader.HasCol("F_IP_ADDRESS") && !reader.IsDBNull("F_IP_ADDRESS"))
            {
                item.IpAddress = reader.GetString("F_IP_ADDRESS");
            }
            if (reader.HasCol("F_PORT") && !reader.IsDBNull("F_PORT"))
            {
                item.Port = reader.GetInt32("F_PORT");
            }
            if (reader.HasCol("F_ONLINE") && !reader.IsDBNull("F_ONLINE"))
            {
                item.Online = reader.GetInt16("F_ONLINE") == 1;
            }
            else
            {
                item.Online = false;
            }
            if (reader.HasCol("F_CREATE_DATE") && !reader.IsDBNull("F_CREATE_DATE"))
            {
                item.CreateDate = reader.GetDateTime("F_CREATE_DATE");
            }
            if (reader.HasCol("F_UPDATE_DATE") && !reader.IsDBNull("F_UPDATE_DATE"))
            {
                item.UpdateDate = reader.GetDateTime("F_UPDATE_DATE");
            }
            if (reader.HasCol("F_CAR_COLOR") && !reader.IsDBNull("F_CAR_COLOR"))
            {
                item.Car_Color = reader["F_CAR_COLOR"].ToString();
            }
            else
            {
                item.Car_Color = "未知";
            }
            //是否启用救援
            if (reader.HasCol("F_RESCUE_ENABLED") && !reader.IsDBNull("F_RESCUE_ENABLED"))
            {
                item.RescueEnabled = reader.GetInt16("F_RESCUE_ENABLED") == 1;
            }
            else
            {
                item.RescueEnabled = false;
            }
            //救援服务商
            if (reader.HasCol("F_RESCUE_SP") && !reader.IsDBNull("F_RESCUE_SP"))
            {
                item.RescueServiceProvider = reader.GetInt16("F_RESCUE_SP");
            }
            else
            {
                item.RescueServiceProvider = 0;
            }
            //if (reader.HasCol("F_DP_ID") && !reader.IsDBNull("F_DP_ID"))
            //{
            //    item.F_DP_ID = reader.GetInt64("F_DP_ID");
            //}
            //else
            //{
            //    item.F_DP_ID = 0;
            //}
            if (reader.HasCol("F_CAR_VIN") && !reader.IsDBNull("F_CAR_VIN"))
                item.VIN = reader.GetString("F_CAR_VIN");

            return item;
        }
        #endregion
    }

    /// <summary>
    /// 油补所需信息
    /// </summary>
    public class DeviceYb
    {
        /// <summary>
        /// 车辆ID
        /// </summary>
        public Int64 VehicleID { get; set; }
        /// <summary>
        /// 油补车辆ID
        /// </summary>
        public string YbVehicleID { get; set; }
        /// <summary>
        /// 车牌
        /// </summary>
        public string YbVehicleName { get; set; }
        /// <summary>
        /// 车牌颜色
        /// </summary>
        public string YbVehicleColor { get; set; }
        /// <summary>
        /// 车辆VIN码
        /// </summary>
        public string VIN { get; set; }
        /// <summary>
        /// 省编号
        /// </summary>
        public string Provice { get; set; }
        /// <summary>
        /// 市编号
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 公司编号
        /// </summary>
        public long CompanyCode { get; set; }
        /// <summary>
        /// 子公司编号
        /// </summary>
        public string ZiCompanyCode { get; set; }
        /// <summary>
        /// 路线标识码
        /// </summary>
        public string LineMark { get; set; }

        public static DeviceYb NewItem(SQ.Base.MyDataReader reader)
        {
            var item = new DeviceYb();
            if (!reader.IsDBNull("F_VEHICLE_ID"))
                item.VehicleID = reader.GetInt64("F_VEHICLE_ID");
            if (!reader.IsDBNull("F_VEHICLE_CODE"))
                item.YbVehicleID = reader.GetString("F_VEHICLE_CODE");
            if (!reader.IsDBNull("F_PLATE_CODE"))
                item.YbVehicleName = reader.GetString("F_PLATE_CODE");
            if (!reader.IsDBNull("F_PLATE_COLOR"))
                item.YbVehicleColor = reader.GetString("F_PLATE_COLOR");
            if (!reader.IsDBNull("F_VEHICLE_FRAME_NO"))
                item.VIN = reader.GetString("F_VEHICLE_FRAME_NO");
            if (!reader.IsDBNull("F_SHENG"))
                item.Provice = reader.GetString("F_SHENG");
            else
                item.Provice = "";
            if (!reader.IsDBNull("F_SHI"))
                item.City = reader.GetString("F_SHI");
            else
                item.City = "9";
            //if (!reader.IsDBNull("F_ENTERPRISE_CODE"))
            //    item.CompanyCode = reader.GetString("F_ENTERPRISE_CODE");
            //else
            //    item.CompanyCode = "";
            item.ZiCompanyCode = "";
            if (!reader.IsDBNull("F_LINE_NO"))
                item.LineMark = reader.GetString("F_LINE_NO");
            else
                item.LineMark = "";
            return item;
        }
    }

    #endregion


}
