using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace ReportsServer.Core
{
    public interface IReport
    {
        int Id { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        ReportNames Name { get; set; }
        IReadOnlyDictionary<string, object> Params { get; set; }
    }

}