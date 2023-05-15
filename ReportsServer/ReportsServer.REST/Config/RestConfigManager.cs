using System;
using System.Collections.Generic;
using ReportsServer.REST.Config.Interface;
using ReportsServer.REST.Implementation;
using ReportsServer.REST.Interface;

namespace ReportsServer.REST.Config
{
    internal static class RestConfigManager
    {
        private static readonly IReadOnlyDictionary<string, Type> _dependencies;

        static RestConfigManager()
        {
            _dependencies = new Dictionary<string, Type>()
            {
                {"rVision", typeof(RestRVisionService)},
                {"ACL", typeof(AclService)}
            };
        }

        public static IRestService ConfigureRestService(string host, string name, IRestServiceConfig config)
        {
            if (!_dependencies.TryGetValue(name, out var value)) return null;
            var service = (IRestService) Activator.CreateInstance(value);
            service.Initialize(host, config);
            return service;
        }
    }
}