using Microsoft.AspNetCore.Mvc;
using ReportsServer.FileModule;

namespace ReportsServer.Processor.Helpers
{
    public interface IDownloadHelper
    {
        string GetContentTypeByExt(string extension);
        string GetContentTypeByFileName(string fileName);
        string GetFileName(params object[] args);
        string GetPeriodFileName(params object[] args);
        FileContentResult GetFileContentResult(string path, string downloadFileName);
        DownloadFile GetFileInfo(string path);
    }
}