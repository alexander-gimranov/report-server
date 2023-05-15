using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ReportsServer.Core;
using ReportsServer.FileModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReportsServer.Processor
{
    public interface IProcessor
    {
        string GetReports();
        string GetJson(object obj);
        IPrepareReport GetFillPrepareReport(IRequestReport report);
        FileContentResult GenerateReportFile(string json, FileTypes fileType);
        FileContentResult GenerateReportFile(string json, string fileType);
        FileContentResult GenerateReportFile(IPrepareReport report, FileTypes fileType);
        bool TryGenerateReportFile(IRequestReport report, FileTypes fileType, out DownloadFile file);
        Task<DownloadFile> GenerateReportFile(IRequestReport report, FileTypes fileType);

        Task<string> GenerateReportHtml(IRequestReport report, ICompositeViewEngine viewEngine,
            ITempDataDictionary tempData, ActionContext actionContext, ViewDataDictionary viewData);
    }
}