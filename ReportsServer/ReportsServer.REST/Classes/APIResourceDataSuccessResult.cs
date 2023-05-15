using System.Collections.Generic;

namespace ReportsServer.REST.Classes
{
    internal class APIResourceDataSuccessResult : BaseAPIResult
    {
        internal class SimpleValueData
        {
            public long ResourceId { get; set; }
            public IEnumerable<DataJson> PropertiesData { get; set; }
        }

        public IEnumerable<SimpleValueData> Data { get; set; }
    }
}