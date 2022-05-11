using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using _808GW.Models;
using JTServer.Model;
using Microsoft.AspNetCore.Http;

namespace _808GW.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(int start = 0, int pagesize = 20)
        {
            if (pagesize <= 0)
            {
                pagesize = 20;
            }
            var list = Program.task.ChejiList.Values;

            if (start > list.Count - 1)
            {
                start = list.Count;
            }
            PagingCJView paging = new PagingCJView()
            {
                Sum = list.Count,
                Start = start
            };
            paging.Data = list.Skip(start).Take(pagesize).ToArray().ToList();


            return View(paging);
        }


        public IActionResult GBConf(string Sim)
        {
            var cj = Program.task.GetChejiByClientPool(Sim);
            if (cj != null)
            {
                GBDeviceSetting conf = cj.GetGBDeviceSetting();
                if (conf == null)
                {
                    conf = new GBDeviceSetting();
                    conf.DeviceID = Sim.PadLeft(20, '0');
                    conf.Channels = new List<GBDeviceSetting.ChannelItem>();
                    var strhead = conf.DeviceID.Substring(0, 8);
                    var count = 0;
                    if (cj.AvParameters != null)
                    {
                        count = cj.AvParameters.VideoMaxChannels;
                    }
                    for (int i = 1; i <= count; i++)
                    {
                        conf.Channels.Add(new GBDeviceSetting.ChannelItem((byte)i, strhead + "011320" + i.ToString().PadLeft(6, '0')));
                    }
                }

                return View(conf);
            }
            return Error();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GBConf(string Sim, GBDeviceSetting conf) //IFormCollection collection)
        {
            var cj = Program.task.GetChejiByClientPool(Sim);
            if (cj != null)
            {
                var collection = HttpContext.Request.Form;
                for (int i = 0; ; i++)
                {
                    var id = collection["Channels[" + i + "].ID"];
                    var channel = collection["Channels[" + i + "].Channel"];
                    if (id.Count == 0 || channel.Count == 0)
                    {
                        break;
                    }
                    conf.Channels.Add(new GBDeviceSetting.ChannelItem(Convert.ToByte(channel), id));
                }
                cj.SaveGBDeviceSetting(conf);
                if (conf.Enable)
                {
                    cj.StartGBCheji(conf);
                }
                else
                {
                    cj.StopGBCheji();
                }
                return RedirectToAction(nameof(Index));
            }
            return Error();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
