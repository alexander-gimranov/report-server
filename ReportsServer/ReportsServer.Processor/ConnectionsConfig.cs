using System.Collections.Generic;
using ReportsServer.Core;

namespace ReportsServer.Processor
{
    internal class ConnectionsConfig
    {
        public IReadOnlyDictionary<DAL.DatabaseType, ConnectionConfig> Connections { get; set; }
    }
}
