using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _808GW.Model;
using JX;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SQ.Base;

namespace _808GW.Controllers
{
    public class apiController : ControllerBase
    {

        public string VideoControl(string Content, bool IsSuperiorPlatformSend = false)
        {
            Program.task.WriteLog("VideoControl接口收到数据 Content=" + Content + (IsSuperiorPlatformSend ? "&IsSuperiorPlatformSend=true" : ""));
            if (string.IsNullOrWhiteSpace(Content))
            {
                return "-1";
            }
            return Program.task.Send1078ToDev(Content, IsSuperiorPlatformSend);
        }
        public string WCF0x9105(string Content)
        {
            Program.task.WriteLog("WCF0x9105接口收到数据 Content=" + Content);
            if (string.IsNullOrWhiteSpace(Content))
            {
                return "-1";
            }
            return Program.task.Send2Cheji0x9105(Content);
        }
        public string GetVehicleSim(string PlateCode, int PlateColor)
        {
            string key = PlateCode + "_" + PlateColor;
            if (Program.task.dic809Vehicle.TryGetValue(key, out var dev))
            {
                return dev.Sim;
            }
            return null;
        }
        public string SendTextMsg(string Sim, byte Flag, string Text)
        {
            return Program.task.SendTextMsgToDev(Sim, Flag, Text);
        }


        public string GetDeviceInfo(string Sim)
        {
            try
            {
                var cj = Program.task.GetChejiByClientPool(Sim);
                if (cj != null)
                    return new ApiDevInfo
                    {
                        DeviceInfo = cj.DevInfo,
                        AVParameters = cj.AvParameters,
                    }.ToJson();
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
