using System.Collections.Generic;
using System.Net;

namespace ReportsServer.REST.Classes
{
    internal class AclSuccessResult : BaseAPIResult
    {
        public string Login { get; set; }

        public string LoginStatus { get; set; }

        public string Msg { get; set; }

        public List<string> Permissions { get; set; }

        public List<Cookie> Cookies { get; set;}
        
    }
}
