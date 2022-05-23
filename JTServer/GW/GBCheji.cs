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
            public string TaskID;

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
                    string url;
                    switch (item.sdp.SType)
                    {
                        case SDP28181.PlayType.Play:
                            if (item.sdp.Media == SDP28181.MediaType.audio)
                                url = $"{cj.cl.MyTask.Config.RTVSAPI}StartRealPlay?TaskID={item.TaskID}&SSRC={item.sdp.SSRC}&DataType=3";
                            else
                                url = $"{cj.cl.MyTask.Config.RTVSAPI}StartRealPlay?TaskID={item.TaskID}&SSRC={item.sdp.SSRC}";
                            break;
                        case SDP28181.PlayType.Playback:
                        case SDP28181.PlayType.Download:
                            url = $"{cj.cl.MyTask.Config.RTVSAPI}StartPlayback?TaskID={item.TaskID}&SSRC={item.sdp.SSRC}&StartTime={item.sdp.TStart.UNIXtoDateTime()}&EndTime={item.sdp.TEnd.UNIXtoDateTime()}";
                            break;
                        case SDP28181.PlayType.Talk:
                            url = $"{cj.cl.MyTask.Config.RTVSAPI}StartRealPlay?TaskID={item.TaskID}&SSRC={item.sdp.SSRC}&DataType=2";
                            break;
                        default:
                            return false;
                    }
                    var str = await SQ.Base.HttpHelperByHttpClient.HttpRequestHtml(url, false, CancellationToken.None);
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
                    var str = await SQ.Base.HttpHelperByHttpClient.HttpRequestHtml(cj.cl.MyTask.Config.RTVSAPI + $"Stop?TaskID={item.TaskID}", false, CancellationToken.None);
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
                            TaskID = res.TaskID,
                            sdp = sdp
                        };
                        var ans = sdp.AnsSdp(did, res.LocIP, res.LocIP, res.LocPort);
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
