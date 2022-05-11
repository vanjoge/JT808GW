using GB28181;
using GB28181.Client;
using GB28181.XML;
using SIPSorcery.SIP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQ.Base;
using JTServer.Model;
using System.Collections.Concurrent;

namespace JTServer.GW
{
    public class GBCheji : GB28181SipClient
    {
        class fromTagCache
        {
            public string TaskId;

            public SDP28181 sdp;
        }
        JTCheji cj;

        Dictionary<string, int> ditChannel = new Dictionary<string, int>();
        /// <summary>
        /// KEY fromTag
        /// </summary>
        ConcurrentDictionary<string, fromTagCache> ditFromTagCache = new ConcurrentDictionary<string, fromTagCache>();
        public GBCheji(JTCheji cj, GBDeviceSetting setting, int expiry = 7200, string UserAgent = "rtvs v1", bool EnableTraceLogs = false) :
           base(setting.GetServerSipStr(), setting.ServerId, setting.GetDeviceInfo(), setting.GetDeviceList(), setting.Password, expiry, UserAgent, EnableTraceLogs)
        {
            this.cj = cj;
            foreach (var item in setting.Channels)
            {
                ditChannel[item.ID] = item.Channel;
            }
        }
        protected override async Task<bool> On_ACK(string fromTag, global::SIPSorcery.SIP.SIPRequest sipRequest)
        {
            try
            {
                if (ditFromTagCache.TryGetValue(fromTag, out var item))
                {
                    var str = await SQ.Base.HttpHelperByHttpClient.HttpRequestHtml(cj.cl.MyTask.Config.RTVSAPI + $"StartRealPlay?TaskId={item.TaskId}&SSRC={item.sdp.SSRC}", false, CancellationToken.None);
                    var res = str.ParseJSON<RETModel>();
                    return res.Code == StateCode.Success;
                }
            }
            catch (Exception)
            {

            }
            return false;
        }

        protected override async Task<bool> On_BYE(string fromTag, global::SIPSorcery.SIP.SIPRequest sipRequest)
        {
            try
            {
                if (ditFromTagCache.TryGetValue(fromTag, out var item))
                {
                    var str = await SQ.Base.HttpHelperByHttpClient.HttpRequestHtml(cj.cl.MyTask.Config.RTVSAPI + $"Stop?TaskId={item.TaskId}", false, CancellationToken.None);
                    var res = str.ParseJSON<RETModel>();
                    return res.Code == StateCode.Success || res.Code == StateCode.NotFoundTask;
                }
            }
            catch (Exception)
            {

            }
            return false;
        }

        protected override async Task<SDP28181> On_INVITE(string fromTag, SDP28181 sdp, SIPRequest sipRequest)
        {
            try
            {
                var did = sipRequest.Header.To.ToURI.User;
                if (ditChannel.TryGetValue(did, out var Channel))
                {
                    var str = await SQ.Base.HttpHelperByHttpClient.HttpRequestHtml(cj.cl.MyTask.Config.RTVSAPI + $"CreateSendRTPTask?Protocol={(cj.jtdata.Is2019 ? "1" : "0")}&Sim={cj.SimKey}&Channel={Channel}&RTPServer={sdp.RtpIp}&RTPPort={sdp.RtpPort}&UseUdp={(sdp.NetType == SDP28181.RTPNetType.TCP ? "false" : "true")}", false, CancellationToken.None);

                    var res = str.ParseJSON<SendRTPTask>();
                    if (res.Code == StateCode.Success)
                    {
                        ditFromTagCache[fromTag] = new fromTagCache
                        {
                            TaskId = res.TaskId,
                            sdp = sdp
                        };
                        var ans = sdp.AnsSdp(did, res.LocIP, res.LocIP, res.LocPort);
                        //RTVS暂只支持TCP推RTP 暂限定为TCP
                        ans.NetType = SDP28181.RTPNetType.TCP;
                        return ans;
                    }
                }
            }
            catch (Exception)
            {

            }
            return null;
        }

        protected override async Task<RecordInfo> On_RECORDINFO(RecordInfoQuery res, global::SIPSorcery.SIP.SIPRequest sipRequest)
        {
            return null;
        }
    }
}
