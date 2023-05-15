namespace ReportsServer.Processor
{
    internal class ConnectionConfig: IConnectionConfig
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }

        public override string ToString()
        {
            return $"server={Server};database={Database};User ID={User};Password={Password}" + (!string.IsNullOrEmpty(Port) ? ";Port="+Port:"");
        }
    }
}
