using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ImageResizeWebApp.Models;
using Microsoft.Extensions.Options;

namespace ImageResizeWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AzStorageConfig storageConfig;

        public HomeController(ILogger<HomeController> logger, IOptions<AzStorageConfig> config)
        {
            _logger = logger;
            storageConfig = config.Value;
        }

        public IActionResult Index()
        {
            return View(storageConfig);
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
