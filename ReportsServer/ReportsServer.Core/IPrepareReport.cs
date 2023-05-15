using System.Collections.Generic;

namespace ReportsServer.Core
{
    public interface IPrepareReport: IReport
    {
        IReadOnlyCollection<IReadOnlyDictionary<string, object>>[] Collections { get; set; }
        string Template { get; set; }
    }
}