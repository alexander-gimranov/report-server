using System.IO;

namespace ReportsServer.FileModule
{
    public class DownloadFile
    {
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string TempData { get; set; }
        public string FileUid { get; set; }

        public byte[] GetAsBytes()
        {
            return File.Exists(TempData) ? File.ReadAllBytes(TempData) : null;
        }
    }
}