using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReportsServer.REST.Config.Interface
{
    internal interface IRestServicesConfig
    {
        string Host { get; set; }
        IReadOnlyDictionary<string, RestServiceConfig> Services { get; set; }
    }
}