using System.Collections.Generic;
using System.Linq;

namespace ReportsServer.Core
{
    public class PrepareReport: Report, IPrepareReport
    {
        private IReadOnlyCollection<IReadOnlyCollection<IReadOnlyDictionary<string, object>>> _collections;

        public PrepareReport(IReport report): base(report)
        {
        }

        public IReadOnlyCollection<IReadOnlyDictionary<string, object>>[] Collections
        {
            get => _collections.ToArray();
            set => _collections = value?.ToList().AsReadOnly();
        }

        public string Template { get; set; }
    }
}