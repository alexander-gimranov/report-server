using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ReportsServer.Core;
using ReportsServer.FileModule.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace ReportsServer.FileModule.Html
{
    internal class HtmlProcessor: IFileProcessor
    {
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ActionContext _actionContext;
        private readonly ViewDataDictionary _viewData;
        private readonly ITempDataDictionary _tempData;

        public HtmlProcessor(ICompositeViewEngine viewEngine, ITempDataDictionary tempData, ActionContext actionContext, ViewDataDictionary viewData)
        {
            _viewEngine = viewEngine;
            _actionContext = actionContext;
            _viewData = viewData;
            _tempData = tempData;
        }

        public string GenerateFromCollections<T>(string template, IReadOnlyDictionary<string, object> reportParams, params IEnumerable<T>[] collections)
        {
            throw new NotImplementedException();
        }

        public string GenerateFromCollections<T>(IReadOnlyDictionary<string, object> reportParams, params IEnumerable<T>[] collections)
        {
            throw new NotImplementedException();
        }

        public string GenerateFromCollections<T>(params IEnumerable<T>[] collections)
        {
            throw new NotImplementedException();
        }

        public string GenerateFromReport(IPrepareReport report)
        {
            return GenerateFromReportAsync(report).GetAwaiter().GetResult();
        }

        public async Task<string> GenerateFromReportAsync(IPrepareReport report)
        {
            _viewData.Model = report;

            using (var writer = new StringWriter())
            {
                var viewResult = GetViewResult(report);

                var viewContext = new ViewContext(
                    _actionContext,
                    viewResult.View,
                    _viewData,
                    _tempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return HttpUtility.HtmlDecode(writer.GetStringBuilder().ToString());
            }
        }

        private ViewEngineResult GetViewResult(IPrepareReport report)
        {
            var viewName = report.Name.ToString();
            if (TryExists(viewName, out var viewEng)) return viewEng;
            viewName = report.Template;
            if (TryExists(viewName, out viewEng)) return viewEng;
            viewName = "Default";
            TryExists(viewName, out viewEng);
            return viewEng;
        }

        private bool TryExists(string viewName, out ViewEngineResult viewEngineResult)
        {
            var appDomain = AppDomain.CurrentDomain;
            var basePath = appDomain.RelativeSearchPath ?? appDomain.BaseDirectory;
            viewEngineResult = _viewEngine.GetView(basePath + "Views\\", viewName + ".cshtml", false);
            return viewEngineResult != null && viewEngineResult.Success;
        }

    }
}