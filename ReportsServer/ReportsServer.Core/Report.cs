using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace ReportsServer.Core
{
    public class Report: IReport
    {
        public Report()
        {
        }

        public Report(IReport report)
        {
            if (report == null) return;
            Id = report.Id;
            Name = report.Name;
            Params = report.Params;
        }

        public int Id { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReportNames Name { get; set; }
        public IReadOnlyDictionary<string, object> Params { get; set; }
    }
}