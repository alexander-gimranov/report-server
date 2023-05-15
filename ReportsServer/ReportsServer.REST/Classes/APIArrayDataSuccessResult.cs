using System.Collections.Generic;

namespace ReportsServer.REST.Classes
{
    internal class APIDataCollectionSuccessResult : BaseAPIResult
    {
        public IEnumerable<DataJson> Data { get; set; }
    }
}