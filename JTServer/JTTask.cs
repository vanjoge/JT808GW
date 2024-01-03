using JTServer.GW;
using JTServer.Model;
using JX;
using RedisHelp;
using SQ.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace JTServer
{
    public class JTTask
    {
        #region 委托
        public delegate void dlgClientChange(JTClient CJuser);
        public delegate void dlgChejiChange(JTCheji cheji);
        #endregion
        #region 事件
        /// <summary>
        /// 客户端连接
        /// </summary>
        public event dlgClientChange CJAdd;
        /// <summary>
        /// 客户端连接注销
        /// </summary>
        public event dlgClientChange CJRemove;
        /// <summary>
        /// 终端上线(鉴权成功)
        /// </summary>
        public event dlgChejiChange ChejiAdd;
        /// <summary>
        /// 终端离线(超时)
        /// </summary>
        public event dlgChejiChange ChejiRemove;
        #endregion


        #region 属性
        ThreadWhile<object> thCheck;

        object lck_NowOrderID = new object();
        private long NowOrderID = DateTime.Now.Ticks;
        GW.JTServer jTServer;
        public object ChejiListLock = new object();
        /// <summary>
        /// 车机列表
        /// </summary>
        public ConcurrentDictionary<string, JTCheji> ChejiList = new ConcurrentDictionary<string, JTCheji>();
        public SQ.Base.Queue.Scheduler<RedisHelper> schRedis;
        public RedisHelper RedisHelper;
        public ConfigModel Config { get; set; }
        public DateTime LastReciveTime { get; internal set; }
        public List<DeviceParameters> DeviceParameters { get; internal set; }

        public ConcurrentDictionary<long, JX.OrderInfo> dicOrderInfo = new ConcurrentDictionary<long, JX.OrderInfo>();


        public ConcurrentDictionary<string, DeviceInfo> dic809Vehicle = new ConcurrentDictionary<string, DeviceInfo>();
        public ConcurrentDictionary<string, DeviceInfo> dic809VehicleBySim = new ConcurrentDictionary<string, DeviceInfo>();

        /// <summary>
        /// 离线指令
        /// TODO:未保存，重启服务会丢失
        /// Key:Sim
        /// 二级Key:离线指令ID
        /// </summary>
        public ConcurrentDictionary<string, Dictionary<int, OfflineCmd>> dicOfflineCmds = new ConcurrentDictionary<string, Dictionary<int, OfflineCmd>>();

        #endregion


        #region 逻辑

        public long GetOrderID()
        {
            lock (lck_NowOrderID)
            {
                return NowOrderID++;
            }

        }
        public string Send1078ToDev(string Hex, bool IsSuperiorPlatformSend)
        {
            try
            {

                var bts = ByteHelper.HexStringToBytes(Hex);
                var head = JTHeader.NewEntity(bts);
                var cj = GetChejiByClientPool(head.Sim);
                if (cj == null)
                {
                    return "0";
                }
                var bGps = new byte[bts.Length - head.HeadLen];
                Array.Copy(bts, head.HeadLen, bGps, 0, bGps.Length);
                return cj.Send1078ToDev(IsSuperiorPlatformSend, head, bGps);
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("Send1078ToDev", ex);
                return "-1";
            }
        }
        public string SendTextMsgToDev(string Sim, byte Flag, string Text, int OffId)
        {
            var ret = SendTextMsgToDev(Sim, Flag, Text);
            if (ret != "1" && OffId > 0)
            {
                var dit = dicOfflineCmds.GetOrAdd(Sim, p =>
                {
                    return new Dictionary<int, OfflineCmd>();
                });
                dit[OffId] = new OfflineCmd
                {
                    MsgId = 0x8300,
                    JTData = new JTSendTextMsg
                    {
                        Flag = (JTTextFlag)Flag,
                        TextInfo = Text
                    }
                };
                return "1001";
            }
            return ret;
        }

        private string SendTextMsgToDev(string Sim, byte Flag, string Text)
        {
            try
            {
                var cj = GetChejiByClientPool(Sim);
                if (cj == null)
                {
                    return "0";
                }
                return cj.SendTextMsg(Flag, Text) ? "1" : "-1";
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("Send1078ToDev", ex);
                return "-1";
            }
        }


        public string Send2Cheji0x9105(string Content)
        {
            try
            {
                var jt9105 = Content.ParseJSON<JT0x9105SimItem[]>();

                foreach (var item in jt9105)
                {
                    var cj = GetChejiByClientPool(item.Sim);

                    cj?.Send0x9105(item.NotifyList);

                }
                return "1";
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("Send1078ToDev", ex);
                return "-1";
            }

        }

        public string getClientKey()
        {
            return DateTime.Now.Ticks.ToString();
        }
        public void OnCJAdd(JTClient mCj)
        {
            if (CJAdd != null)
                CJAdd(mCj);
        }
        public void OnCJRemove(JTClient mCj)
        {
            if (CJRemove != null)
                CJRemove(mCj);
        }

        /// <summary>
        /// 根据SIM获取车机实例
        /// </summary>
        /// <param name="Sim"></param>
        /// <returns></returns>
        public JTCheji GetChejiByClientPool(string Sim)
        {
            if (Sim != null && ChejiList.TryGetValue(Sim, out JTCheji cj))
            {
                return cj;
            }
            return null;
        }


        /// <summary>
        /// 触发车机上线事件
        /// </summary>
        /// <param name="CJuser"></param>
        public void OnChejiAdd(JTCheji CJuser)
        {
            try
            {
                lock (ChejiListLock)
                {
                    ChejiList[CJuser.Key] = CJuser;
                }
                ChejiAdd?.Invoke(CJuser);
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("Task.OnChejiAdd:", ex);
            }

        }
        /// <summary>
        /// 触发车机下线事件
        /// </summary>
        /// <param name="CJuser"></param>
        public void OnChejiRemove(JTCheji CJuser, bool Force = false)
        {
            try
            {
                lock (ChejiListLock)
                    ChejiList.TryRemove(CJuser.Key, out var rm);
                ChejiRemove?.Invoke(CJuser);

            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("Task.OnChejiRemove:", ex);
            }
        }
        #endregion


        public void Start()
        {
            Stop();
            ByteHelper.RegisterGBKEncoding();

            if (!ReadConfig())
            {
                Config = new ConfigModel();
            }
            this.RedisHelper = new RedisHelper();
            var len = 3;
            schRedis = new SQ.Base.Queue.Scheduler<RedisHelper>(len);
            schRedis.Tags = new RedisHelper[len];
            for (int i = 0; i < len; i++)
            {
                schRedis.Tags[i] = new RedisHelper();
            }
            schRedis.Start();
            jTServer = new GW.JTServer(this);
            jTServer.Start();
            thCheck = new ThreadWhile<object>();
            thCheck.SleepMs = Config.ChkSckTimerInterval;
            thCheck.StartIfNotRun(Check, null, "CheckTh");

        }

        private void Check(object tag, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            jTServer.Check();
        }

        public void Stop()
        {
            thCheck?.Stop();
            jTServer?.Stop();
            schRedis?.StopSafe();
        }
        /// <summary>
        /// 读取配置信息
        /// </summary>
        /// <returns></returns>
        public bool ReadConfig()
        {
            try
            {
                var path = SQ.Base.FileHelp.GetMyConfPath() + "SettingConfig.xml";
                if (!File.Exists(path))
                {
                    Log.WriteLog4("配置文件不存在，已加载默认配置");
                    return false;
                }
                Config = SerializableHelper.DeserializeSetting<ConfigModel>(path);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog4("ReadConfig:" + ex.ToString(), LOGTYPE.ERRORD);
                return false;
            }
        }

        public void WriteLog(string log, LOGTYPE logTYPE = LOGTYPE.DEBUG)
        {
            if (Config.ShowNetLog)
                Log.WriteLog4(log, logTYPE);
        }
        public void WriteBytesLog(string head, byte[] bts, int offset, int count, LOGTYPE logTYPE = LOGTYPE.DEBUG)
        {
            if (Config.ShowNetLog)
            {
                Log.WriteLog4(head + ByteHelper.BytesToHexString(bts, offset, count, Config.HexAddSpace), LOGTYPE.DEBUG);
            }
        }
    }
}
