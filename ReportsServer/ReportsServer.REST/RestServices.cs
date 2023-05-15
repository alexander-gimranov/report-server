using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ReportsServer.Core;
using ReportsServer.REST.Config;
using ReportsServer.REST.Config.Interface;
using ReportsServer.REST.Interface;

namespace ReportsServer.REST
{
    public class RestServices
    {
        private readonly IRestServicesConfig _config;

        public RestServices(string pathConfig)
        {
            _config = CreateHelper.Create<RestServicesConfig>(pathConfig);
        }

        public IRestService Service(string name)
        {
            return _config.Services.TryGetValue(name, out var serviceConfig)
                ? RestConfigManager.ConfigureRestService(_config.Host, name, serviceConfig)
                : null;
        }
    }
}