using System.Collections.Generic;
using System.Net;

namespace ReportsServer.Core
{
    public interface IRequestReport: IReport
    {
        string Login { get; set; }
        IReadOnlyDictionary<string, string> Cookies { get; set; }
    }

}