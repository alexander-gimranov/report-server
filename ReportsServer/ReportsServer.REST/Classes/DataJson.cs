using System.Collections.Generic;

namespace ReportsServer.REST.Classes
{
    internal class DataJson
    {
        public long Order { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<string> Values { get; set; }
    }
}