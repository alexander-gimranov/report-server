using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ReportsServer.REST;

namespace ReportsServer.API
{
    public class Program
    {
        private static string basePath;
        public static void Main(string[] args)
        {
            var appDomain = AppDomain.CurrentDomain;
            basePath = appDomain.RelativeSearchPath ?? appDomain.BaseDirectory;
            var log4NetConfig = new XmlDocument();
            log4NetConfig.Load(File.OpenRead(Path.Combine(basePath, "config", "log4net.config")));
            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, log4NetConfig["log4net"]);
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(Path.Combine(basePath, "config", "hosting.json"), optional: true)
                    .Build()
                )
                .UseStartup<Startup>()
                .Build();
    }
}
