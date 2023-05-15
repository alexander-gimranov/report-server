using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using ReportsServer.Core;
using ReportsServer.FileModule;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReportsServer.API.Controllers
{
    [Route("api")]
    public class BaseController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(BaseController));
        private readonly ICompositeViewEngine _viewEngine;
        protected readonly Processor.IProcessor _processor;

        public BaseController(ICompositeViewEngine viewEngine)
        {
            var appDomain = AppDomain.CurrentDomain;
            var basePath = appDomain.RelativeSearchPath ?? appDomain.BaseDirectory;
            var configDir = Path.Combine(basePath, "config");
            _processor = new Processor.Processor(log4net.LogManager.GetLogger(typeof(Processor.Processor)), configDir);
            _viewEngine = viewEngine;
        }

        [HttpGet]
        public ContentResult Get()
        {
            Log.Debug("Get");
            return new ContentResult
            {
                ContentType = "application/json",
                StatusCode = (int) HttpStatusCode.OK,
                Content = _processor.GetReports()
            };
        }

        [HttpPost("{fileType}")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 0)]
        public async Task<IActionResult> Post(FileTypes fileType, [FromBody] RequestReport report)
        {
            Log.Debug($"Post {fileType} | [{JsonConvert.SerializeObject(report)}]");
            if (report == null) return BadRequest();
            report.Cookies = Request.Cookies.ToDictionary(k => k.Key, v => v.Value);
            if (fileType == FileTypes.Html)
            {
                var html = await _processor.GenerateReportHtml(report, _viewEngine, TempData, ControllerContext, ViewData);
                if (string.IsNullOrEmpty(html)) return Json(new { status = "error" });
                return Content(html, "text/html", Encoding.UTF8);
            }
            var downloadFile = await _processor.GenerateReportFile(report, fileType);
            return File(downloadFile.GetAsBytes(), downloadFile.ContentType ?? "text/*", downloadFile.FileName);
        }
    }
}