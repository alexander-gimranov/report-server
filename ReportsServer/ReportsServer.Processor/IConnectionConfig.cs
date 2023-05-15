namespace ReportsServer.Processor
{
    internal interface IConnectionConfig
    {
        string Server { get; set; }
        string Database { get; set; }
        string User { get; set; }
        string Password { get; set; }
        string Port { get; set; }
    }
}
