using SQ.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace JTServer.Model
{
    [Serializable]
    public class ConfigModel
    {
        public ConfigModel()
        {
            CanRegSecond = true;
            ShowNetLog = true;
            HexAddSpace = true;
            ConnectionString = "Data Source=.;Initial Catalog=DRecorder;Persist Security Info=True;User ID=sa;Password=123456";
            PortClient = 9300;
            UDPPortClient = 9301;
            PortMonitor = 9988;
            MonitorIP = "10.10.10.22";
            OrderTimerInterval = 30000;
            InformationTimerInterval = 60000;
            ChkSckTimerInterval = 5000;
            SckTimeoutSec = 120;
            ComPort = "COM4";
            BaudRate = 9600;
            MSG_GNSSCENTERID = 88888888;
            TimeoutSec_Answer = 60;
            MediaPath = "E:\\Test\\";
            MaxGpsTimes = 2;
            RTimes = 3;
            PackTimeoutSec = 60;
            HeartTimeoutSec = 300;
            KeepMainSec = 60;
            MinorTimeoutSec = 180;
            SmsTimeoutSec = 300;
            PLATFORM_ID = "123456";
            AutoSendGPSSec = 10000;

            PortClient_SQ = 9400;
            OrderTimerInterval_SQ = 30000;
            ChkSckTimerInterval_SQ = 5000;
            SckTimeoutSec_SQ = 120;
            RTimes_SQ = 3;
            TimeoutSec_Answer_SQ = 60;

            M1 = 10000000U;
            IA1 = 20000000U;
            IC1 = 30000000U;

            TestSimStr = "013100";

            JXSchedulerCount = 3;
            ReceivedSchedulerCount = 5;
            DBSchedulerCount = 5;
            DBSchExecuteTimeoutMSec = 60000;


            StartScheduler = true;
            StartDBScheduler = true;

            SaveGPSInterval = 1000;
            SaveLastTrackInterval = 10000;

            CloseTimeOutSocket = true;

            SckTimeoutSecNoAuthorityID = 10;

            GatewayID = 1;
            Token = "TOKEN.10.10.10.230";
            RelayServerIP = "10.10.10.230";
            RelayServerPort = 5001;
            RelayServerPortTwo = 5002;
            ConnectSim = "000000000000";
            HeartbeatInterval = 20;
            Reconnection = 120;
            RecCountSetSendId = 5;
            SendGpsToMon = false;
            OwerID = "340011500002";
            OwerTel = "13482761837";
            PlatformCode = "15010000001";
            EnabledRoadRescue = false;

            SpecialVersion = 0;
            SubContract = 0;
            ShieldImpulseEnts = string.Empty;

            GateWayNo = 1;
            ClusterWCF = "http://127.0.0.1:2535/Wcf/CService.svc";
            IsNoticeCluster = false;

            FilePath = "E:\\CvnaviData\\";

            IsEnableTtyMakeID = true;
            TtyMakeID = "98765";
            Before = 3;
            After = 1.5;
            RestoreOnlineInterval = 10;
            MileageThreshold = 800000;

            IsEnableGpsToWs = false;
            Enterprises = "11111111";
            IsCatchRelayData = false;

            Svr809IdsString = string.Empty;
            DataTimeout = 5;
            Mobiles = string.Empty;
            GatewayIP = "116.228.114.206";
            ExThreshold = 1000;
            SchThreshold = 10000;

            BanSims = "";
        }
        /// <summary>
        /// 发送GPS和报警到上级平台
        /// </summary>
        public bool SendGpsToMon { get; set; }

        #region 短信猫
        /// <summary>
        /// 短信猫端口
        /// </summary>
        public string ComPort { get; set; }
        /// <summary>
        /// 短信猫波特率
        /// </summary>
        public int BaudRate { get; set; }
        #endregion

        #region 转发服务器//2013-03-01//guozh 
        /// <summary>
        /// 网关唯一ID
        /// </summary>
        public ushort GatewayID { get; set; }
        /// <summary>
        /// 网关连接转发服务器令牌
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 转发服务器IP808
        /// </summary>
        public string RelayServerIP { get; set; }
        /// <summary>
        /// 转发服务器端口808
        /// </summary>
        public int RelayServerPort { get; set; }
        /// <summary>
        /// 转发服务器端口809
        /// </summary>
        public int RelayServerPortTwo { get; set; }
        /// <summary>
        /// 808通道发送鉴权sim号
        /// </summary>
        public string ConnectSim { get; set; }
        /// <summary>
        /// 心跳间隔(秒)
        /// </summary>
        public int HeartbeatInterval { get; set; }
        /// <summary>
        /// 连接重连间隔时间(秒）
        /// </summary>
        public int Reconnection { get; set; }
        /// <summary>
        /// 重连一定次数后，未成功则重置SendID
        /// </summary>
        public int RecCountSetSendId { get; set; }
        #endregion

        #region 集群代理配置

        /// <summary>
        /// 网关编号
        /// </summary>
        public int GateWayNo { get; set; }
        /// <summary>
        /// 集群代理WCF地址,多个用,号分隔
        /// </summary>
        public string ClusterWCF { get; set; }
        /// <summary>
        /// 车辆上下线是否通知集群代理
        /// </summary>
        public bool IsNoticeCluster { get; set; }

        #endregion

        #region 数据库
        /// <summary>
        /// 数据连接字串
        /// </summary>
        public string ConnectionString { get; set; }
        #endregion


        #region 车机监听
        /// <summary>
        /// 监听端口
        /// </summary>
        public int PortClient { get; set; }
        /// <summary>
        /// UDP监听端口
        /// </summary>
        public int UDPPortClient { get; set; }
        /// <summary>
        /// 命令检查间隔(毫秒)
        /// </summary>
        public int OrderTimerInterval { get; set; }
        /// <summary>
        /// 释放空闲连接检查间隔(毫秒)
        /// </summary>
        public int ChkSckTimerInterval { get; set; }
        /// <summary>
        /// 释放空闲连接阀值(秒)
        /// </summary>
        public int SckTimeoutSec { get; set; }
        /// <summary>
        /// 重发次数
        /// </summary>
        public int RTimes { get; set; }
        /// <summary>
        /// 重发时间间隔
        /// </summary>
        public int TimeoutSec_Answer { get; set; }
        /// <summary>
        /// 媒体文件存放路径
        /// </summary>
        public string MediaPath { get; set; }
        /// <summary>
        /// GPS允许大于系统当前时间最大值,单位：小时
        /// </summary>
        public int MaxGpsTimes { set; get; }
        #endregion
        #region SQ客户端监听
        /// <summary>
        /// 监听端口
        /// </summary>
        public int PortClient_SQ { get; set; }
        /// <summary>
        /// 命令检查间隔(毫秒)
        /// </summary>
        public int OrderTimerInterval_SQ { get; set; }
        /// <summary>
        /// 释放空闲连接检查间隔(毫秒)
        /// </summary>
        public int ChkSckTimerInterval_SQ { get; set; }
        /// <summary>
        /// 释放空闲连接阀值(秒)
        /// </summary>
        public int SckTimeoutSec_SQ { get; set; }
        /// <summary>
        /// 重发次数
        /// </summary>
        public int RTimes_SQ { get; set; }
        /// <summary>
        /// 重发时间间隔
        /// </summary>
        public int TimeoutSec_Answer_SQ { get; set; }
        #endregion

        #region 监管平台
        /// <summary>
        /// 从链路监听端口
        /// </summary>
        public UInt16 PortMonitor { get; set; }
        /// <summary>
        /// 从链路监听IP地址
        /// </summary>
        public string MonitorIP { get; set; }
        /// <summary>
        /// 主链路监听服务器
        /// </summary>
        public string MonitorServer { get; set; }
        /// <summary>
        /// 主链路监听端口
        /// </summary>
        public int PortMonitorMain { get; set; }
        /// <summary>
        /// 监管服务器连接ID
        /// </summary>
        public uint USERID { get; set; }
        /// <summary>
        /// 监管服务器连接密码
        /// </summary>
        public string PASSWORD { get; set; }
        /// <summary>
        /// 平台唯一编码
        /// </summary>
        public string PLATFORM_ID { get; set; }
        /// <summary>
        /// 亮度
        /// </summary>
        public byte Brightness { get; set; }
        /// <summary>
        /// 色度
        /// </summary>
        public byte Chromaticity { get; set; }
        /// <summary>
        /// 对比度
        /// </summary>
        public byte Contrast { get; set; }
        /// <summary>
        /// 图像/视频质量
        /// </summary>
        public byte Quality { get; set; }
        /// <summary>
        /// 饱和度
        /// </summary>
        public byte Saturation { get; set; }
        #endregion
        /// <summary>
        /// 分包超时时间(秒)
        /// </summary>
        public int PackTimeoutSec { get; set; }
        /// <summary>
        /// 心跳超时时间(秒)
        /// </summary>
        public int HeartTimeoutSec { get; set; }
        /// <summary>
        /// 保持主链路发送时间间隔(秒)
        /// </summary>
        public int KeepMainSec { get; set; }
        /// <summary>
        /// 从链路超时时间(秒)
        /// </summary>
        public int MinorTimeoutSec { get; set; }
        /// <summary>
        /// SMS超时时间(秒)
        /// </summary>
        public int SmsTimeoutSec { get; set; }
        /// <summary>
        /// 上级平台分配的唯一ID
        /// </summary>
        public uint MSG_GNSSCENTERID { get; set; }
        /// <summary>
        /// 自动向上级平台发送信息间隔时间(毫秒)
        /// </summary>
        public int AutoSendGPSSec { get; set; }

        public uint M1 { get; set; }

        public uint IA1 { get; set; }

        public uint IC1 { get; set; }

        /// <summary>
        /// 测试SIM字符串头
        /// </summary>
        public string TestSimStr { get; set; }

        public int JXSchedulerCount { get; set; }

        public int ReceivedSchedulerCount { get; set; }

        public bool StartScheduler { get; set; }

        public int InformationTimerInterval { get; set; }

        public bool StartDBScheduler { get; set; }

        /// <summary>
        /// GPS处理线程数
        /// </summary>
        public int DBSchedulerCount { get; set; }
        /// <summary>
        /// 单个GPS任务执行超时时间(毫秒)
        /// </summary>
        public int DBSchExecuteTimeoutMSec { get; set; }
        /// <summary>
        /// 保存GPS数据间隔(默认1000ms)
        /// </summary>
        public int SaveGPSInterval { get; set; }
        /// <summary>
        /// 同步最后一次缓存数据间隔(默认10000ms)
        /// </summary>
        public int SaveLastTrackInterval { get; set; }
        /// <summary>
        /// 转换报警配置
        /// </summary>
        public SerializableDictionary<byte, byte> ListCovAlarm = new SerializableDictionary<byte, byte>();
        /// <summary>
        /// 模式（为false时为严格模式 过检专用 不允许多次注册、严格心跳间隔等）
        /// </summary>
        public bool CanRegSecond { get; set; }
        /// <summary>
        /// 断开超时连接
        /// </summary>
        public bool CloseTimeOutSocket { get; set; }
        /// <summary>
        /// 释放长时间无一鉴权成功的连接时间
        /// </summary>
        public int SckTimeoutSecNoAuthorityID { get; set; }
        /// <summary>
        /// 显示通信日志
        /// </summary>
        public bool ShowNetLog { get; set; }
        /// <summary>
        /// HEX加空格
        /// </summary>
        public bool HexAddSpace { get; set; }
        /// <summary>
        /// 显示SIP日志
        /// </summary>
        public bool ShowSipLog { get; set; }
        /// <summary>
        /// RTVSAPI地址
        /// </summary>
        public string RTVSAPI { get; set; }


        /// <summary>
        /// 发送命令等待业务应答后再移除的命令字
        /// </summary>
        public readonly UInt16[] NowRemove = new ushort[] { 0x8104, 0x8201, 0x8302, 0x8500, 0x8700, 0x8701, 0x8802, 0x8801 };

        /// <summary>
        /// 业户ID
        /// </summary>
        public string OwerID { set; get; }
        /// <summary>
        /// 业户联系电话
        /// </summary>
        public string OwerTel { set; get; }


        public string PlatformCode { set; get; }

        public bool EnabledRoadRescue { set; get; }

        /// <summary>
        /// 客户定制版本
        /// 0：普通版本，1:昆明平台版本（只验证制造商ID，厂商ID）
        /// 2：河南中交通信科技版本（只验证SIM卡号）
        /// </summary>
        public int SpecialVersion { get; set; }
        /// <summary>
        /// 总里程阀值
        /// </summary>
        public int MileageThreshold { get; set; }

        /// <summary>
        /// 制造商ID（用于设备注册信息制造商ID为空）
        /// </summary>
        public string TtyMakeID { get; set; }
        /// <summary>
        /// 是否启用验证制造商ID（用于设备注册信息制造商ID为空）
        /// </summary>
        public bool IsEnableTtyMakeID { get; set; }
        /// <summary>
        /// 默认为0，补传分包1，补传分包与RSA下发2
        /// </summary>
        public int SubContract { get; set; }

        /// <summary>
        /// 屏蔽脉冲的企业
        /// </summary>
        public string ShieldImpulseEnts { set; get; }
        /// <summary>
        /// 大数据文件路径
        /// </summary>
        public string FilePath { set; get; }
        /// <summary>
        /// 常规数据文件
        /// </summary>
        public string NormalFilePath { get; set; } = "E:\\cvanvi_normal_data\\";
        /// <summary>
        /// 数据有效天数 今天以前
        /// </summary>
        public double Before { get; set; }
        /// <summary>
        /// 数据有效天数 今天以后
        /// </summary>
        public double After { get; set; }
        /// <summary>
        /// 修复在线状态间隔
        /// </summary>
        public int RestoreOnlineInterval { get; set; }

        #region GPS发送给WebService配置

        /// <summary>
        /// 是否启用GPS发送到Ws
        /// </summary>
        public bool IsEnableGpsToWs { get; set; }
        /// <summary>
        /// 需要转发到Ws的企业编号列表(逗号分隔）
        /// </summary>
        public string Enterprises { get; set; }
        /// <summary>
        /// 是否缓存给转发服务器的数据
        /// </summary>
        public bool IsCatchRelayData { get; set; }
        /// <summary>
        /// 数据超时时间（分钟）
        /// </summary>
        public int DataTimeout { get; set; }
        /// <summary>
        /// 自动连接809ID列表
        /// </summary>
        public string Svr809IdsString { get; set; }
        /// <summary>
        /// 接收短信手机号码
        /// </summary>
        public string Mobiles { get; set; }
        /// <summary>
        /// 网关IP
        /// </summary>
        public string GatewayIP { get; set; }
        /// <summary>
        /// 异常计数阈值
        /// </summary>
        public int ExThreshold { get; set; }
        /// <summary>
        /// 调度器积压阈值
        /// </summary>
        public int SchThreshold { get; set; }
        /// <summary>
        /// 单位时间阈值
        /// </summary>
        public int OffLineThreshold { get; set; } = 100;

        #endregion
        #region Redis
        public string RedisConnectString { set; get; } = "10.10.11.16:7000,10.10.11.16:7001,10.10.11.16:7002,10.10.11.16:7003,10.10.11.16:7004,10.10.11.16:7005";
        /// <summary>
        /// 是否启用停滞报警
        /// </summary>
        public bool EnabledStagnation { get; set; } = true;
        /// <summary>
        /// 数据访问接口URL
        /// </summary>
        public string InterfaceUrl { get; set; } = "http://10.10.11.103:7831/api/GetNetGateData/";
        /// <summary>
        /// 是否连接油补平台
        /// </summary>
        public bool ConnectToYb { get; set; } = false;

        /// <summary>
        /// Zmq连接地址
        /// </summary>
        public string ZmqAddress { get; set; } = "tcp://127.0.0.1:5557";

        /// <summary>
        /// 是否启用Zmq推送报警，车辆上下线
        /// </summary>
        public bool EnabledZeroMQ { set; get; } = true;

        #endregion
        
        /// <summary>
        /// 禁止SIM
        /// </summary>
        public string BanSims { get; set; }
    }
}
