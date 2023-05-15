using System.Collections.Generic;
using System.Threading.Tasks;
using ReportsServer.Core;

namespace ReportsServer.FileModule.Interfaces
{
    internal interface IFileProcessor
    {
        string GenerateFromCollections<T>(string template, IReadOnlyDictionary<string, object> reportParams, params IEnumerable<T>[] collections);
        string GenerateFromCollections<T>(IReadOnlyDictionary<string, object> reportParams, params IEnumerable<T>[] collections);
        string GenerateFromCollections<T>(params IEnumerable<T>[] collections);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        string GenerateFromReport(IPrepareReport report);

        Task<string> GenerateFromReportAsync(IPrepareReport report);
    }
}
