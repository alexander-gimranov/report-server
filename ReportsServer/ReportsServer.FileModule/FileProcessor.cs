using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ReportsServer.Core;
using ReportsServer.FileModule.Excel;
using ReportsServer.FileModule.Html;
using ReportsServer.FileModule.Interfaces;

namespace ReportsServer.FileModule
{
    public class FileProcessor: IFileProcessor
    {
        private readonly IFileProcessor _processor;

        public FileProcessor(FileTypes fileType)
        {
            switch (fileType)
            {
                case FileTypes.Excel:
                    _processor = new ExcelProcessor();
                    break;
                case FileTypes.PDF:
                    throw new NotSupportedException();
                default: throw new ArgumentException(string.Format("File type {0} not supported", fileType.ToString()));
            }
        }

        public FileProcessor(ICompositeViewEngine viewEngine, ITempDataDictionary tempData, ActionContext actionContext, ViewDataDictionary viewData)
        {
            _processor = new HtmlProcessor(viewEngine, tempData, actionContext, viewData);
        }

        public string GenerateFromCollections<T>(string template, IReadOnlyDictionary<string, object> reportParams, params IEnumerable<T>[] collections)
        {
            return _processor.GenerateFromCollections(template, reportParams, collections);
        }

        public string GenerateFromCollections<T>(IReadOnlyDictionary<string, object> reportParams, params IEnumerable<T>[] collections)
        {
            return _processor.GenerateFromCollections(reportParams, collections);
        }

        public string GenerateFromCollections<T>(params IEnumerable<T>[] collections)
        {
            return _processor.GenerateFromCollections(collections);
        }

        public string GenerateFromReport(IPrepareReport report)
        {
            return _processor.GenerateFromReport(report);
        }

        public Task<string> GenerateFromReportAsync(IPrepareReport report)
        {
            return _processor.GenerateFromReportAsync(report);
        }
    }
}