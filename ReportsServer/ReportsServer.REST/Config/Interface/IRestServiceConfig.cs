using System.Collections.Generic;

namespace ReportsServer.REST.Config.Interface
{
    public interface IRestServiceConfig
    {
        string RelativeUrl { get; set; }
        string CoockiePath { get; set; }
        IReadOnlyDictionary<string, string> Methods { get; set; }
        IReadOnlyDictionary<string, ExtraParameter> ExtraParameters { get; set; }
        T GetParameterValue<T>(string name);
        string GetMethod(string name);
    }
}