using System.Collections.Generic;
using Newtonsoft.Json;
using ReportsServer.REST.Config.Interface;

namespace ReportsServer.REST.Config
{
    internal class RestServicesConfig: IRestServicesConfig
    {
        public string Host { get; set; }
        public IReadOnlyDictionary<string, RestServiceConfig> Services { get; set; }
    }
}