using System;
using System.IO;
using System.Resources;
using Microsoft.AspNetCore.Mvc;
using ReportsServer.FileModule;
using ReportsServer.Processor.Recources;

namespace ReportsServer.Processor.Helpers
{
    public class DownloadHelper : IDownloadHelper
    {
        private readonly ResourceManager _commonResource;

        public DownloadHelper()
        {
            _commonResource = new ResourceManager(typeof(CommonResource));
        }

        public string GetContentTypeByExt(string extension)
        {
            return _commonResource.GetString("ContentType" + extension.Replace(".", "4").ToUpper());
        }

        public string GetContentTypeByFileName(string fileName)
        {
            return GetContentTypeByExt(Path.GetExtension(fileName));
        }

        public string GetFileName(params object[] args)
        {
            return string.Format(_commonResource.GetString("FileNameFormat"), args);
        }

        public string GetPeriodFileName(params object[] args)
        {
            return string.Format(_commonResource.GetString("PeriodFileNameFormat"), args);
        }

        public FileContentResult GetFileContentResult(string path, string downloadFileName)
        {
            var fileResult =
                new FileContentResult(File.ReadAllBytes(path), GetContentTypeByFileName(path))
                {
                    FileDownloadName = downloadFileName
                };
            return fileResult;
        }

        public DownloadFile GetFileInfo(string path)
        {
            var ext = Path.GetExtension(path);
            return new DownloadFile()
            {
                ContentType = GetContentTypeByExt(ext),
                FileName = "download__" + ext,
                TempData = path,
                FileUid = Guid.NewGuid().ToString()
            };
        }
    }
}