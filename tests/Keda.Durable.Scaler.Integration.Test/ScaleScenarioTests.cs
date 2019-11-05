using System;
using System.Threading.Tasks;
using DurableTask.AzureStorage.Monitoring;
using Grpc.Net.Client;
using Keda.Durable.Scaler.Integration.Protos;
using Keda.Durable.Scaler.Server.Protos;
using Keda.Durable.Scaler.Server.Repository;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ExternalScaler = Keda.Durable.Scaler.Integration.Protos.ExternalScaler;
using GetMetricsRequest = Keda.Durable.Scaler.Integration.Protos.GetMetricsRequest;

namespace Keda.Durable.Scaler.Integration.Test
{
    public class ScaleScenarioTests
    {
        [Fact]
        public async Task SimpleGRpcCall()
        {
            using (var host = new TestServerHost(typeof(TestSimpleStartup)))
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                host.Start();
                var channel = GrpcChannel.ForAddress("http://localhost:5000");
                var client = new ExternalScaler.ExternalScalerClient(channel);
                var request = new GetMetricsRequest();

                var reply = client.GetMetrics(request);
                Assert.Equal("hello", reply.MetricValues[0].MetricName);
            }
        }



        private class TestSimpleStartup : TestStartupBase
        {
            public override void ConfigureServices(IServiceCollection services) 
            {
                base.ConfigureServices(services);
                services.AddSingleton<IPerformanceMonitorRepository>(new MockSimplePerformanceMonitorRepository());
            }
        }
        private class MockSimplePerformanceMonitorRepository : IPerformanceMonitorRepository
        {
            public Task<PerformanceHeartbeat> PulseAsync(int currentWorkerCount)
            {
                return Task.FromResult(new PerformanceHeartbeat());
            }
        }
    }
}
