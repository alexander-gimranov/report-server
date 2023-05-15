using ReportsServer.REST.Config.Interface;
using System.Collections.Generic;
using System.Linq;

namespace ReportsServer.REST.Config
{
    internal class RestServiceConfig : IRestServiceConfig
    {
        public string RelativeUrl { get; set; }
        public string CoockiePath { get; set; }
        public IReadOnlyDictionary<string, string> Methods { get; set; }
        public IReadOnlyDictionary<string, ExtraParameter> ExtraParameters { get; set; }

        public T GetParameterValue<T>(string name)
        {
            if (ExtraParameters != null && ExtraParameters.Any() && ExtraParameters.TryGetValue(name, out var fltCookie) &&
                fltCookie.GetValueType() == typeof(T))
                return (T) fltCookie.GetValue();
            return default(T);
        }

        public string GetMethod(string name)
        {
            return Methods.TryGetValue(name, out var action) ? action : name;
        }
    }
}