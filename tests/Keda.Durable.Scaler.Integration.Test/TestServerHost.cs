using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Keda.Durable.Scaler.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Keda.Durable.Scaler.Integration.Test
{
    public class TestServerHost : IDisposable
    {
        private IHost _host;
        private Type _startUpClass;
        public TestServerHost(Type startUpClass)
        {
            _startUpClass = startUpClass;
            _host = CreateHostBuilder(new string[] { }).Build();
        }

        public void Start()
        {
            _host.Start();
        }

        public Task RunAsync()
        {
            return _host.RunAsync();
        }

        public Task StopAsync()
        {
            return _host.StopAsync();
        }

        public IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(_startUpClass);
                });

        public void Dispose()
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }
    }
}
