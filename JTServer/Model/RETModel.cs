using System;
using System.Collections.Generic;
using System.Text;

namespace JTServer.Model
{
    /// <summary>
    /// RETModel
    /// </summary>
    public class RETModel
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public StateCode Code { get; set; }
    }
    /// <summary>
    /// RTP推送任务
    /// </summary>
    public class SendRTPTask : RETModel
    {
        /// <summary>
        /// RTP推送任务唯一id，用此id区分多次任务
        /// </summary>
        public string TaskID { get; set; }
        /// <summary>
        /// 本地IP
        /// </summary>
        public string LocIP { get; set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        public int LocPort { get; set; }
    }
    /// <summary>
    /// 状态码定义
    /// </summary>
    public enum StateCode
    {
        /// <summary>
        /// 服务端内部错误
        /// </summary>
        InternalError = -1000,
        /// <summary>
        /// 未找到对应任务
        /// </summary>
        NotFoundTask = -3,
        /// <summary>
        /// 超出限制
        /// </summary>
        OverLimit = -2,
        /// <summary>
        /// 失败
        /// </summary>
        Fail = -1,
        /// <summary>
        /// 成功
        /// </summary>
        Success = 1,
    }
}
