namespace ReportsServer.REST.Classes
{
    internal class APIFailResult : BaseAPIResult
    {
        public string StatusCode { get; set; }

        public string StatusMsg { get; set; }
    }
}
