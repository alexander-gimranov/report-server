using System.Collections.Generic;
using System.Net;

namespace ReportsServer.Core
{
    public class RequestReport : Report, IRequestReport
    {
        public RequestReport(IReport report) : base(report)
        {
        }

        public string Login { get; set; }
        public IReadOnlyDictionary<string, string> Cookies { get; set; }
    }
}