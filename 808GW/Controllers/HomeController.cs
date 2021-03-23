using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using _808GW.Models;

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
