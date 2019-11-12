using System;
using System.Threading.Tasks;
using DurableTask.AzureStorage.Monitoring;
using Grpc.Core;
using Grpc.Net.Client;
using Keda.Durable.Scaler.Integration.Protos;
using Keda.Durable.Scaler.Server.Protos;
using Keda.Durable.Scaler.Server.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using ExternalScaler = Keda.Durable.Scaler.Integration.Protos.ExternalScaler;
using GetMetricsRequest = Keda.Durable.Scaler.Integration.Protos.GetMetricsRequest;
using NewRequest = Keda.Durable.Scaler.Integration.Protos.NewRequest;
using ScaledObjectRef = Keda.Durable.Scaler.Integration.Protos.ScaledObjectRef;

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
                var request = new NewRequest();
                var scaleObjectRef = new ScaledObjectRef();
                scaleObjectRef.Namespace = "foo";
                scaleObjectRef.Name = "bar";

                var reply = client.New(request, new CallOptions());
                Assert.NotNull(reply);  // Make sure gRPC call works. 

                var isActive = client.IsActive(scaleObjectRef, new CallOptions());
                Assert.True(isActive.Result);
            }
        }



        private class TestSimpleStartup : TestStartupBase
        {
            public override void ConfigureServices(IServiceCollection services) 
            {
                base.ConfigureServices(services);
                services.AddGrpc();
                services.AddSingleton<IPerformanceMonitorRepository>(new MockSimplePerformanceMonitorRepository());
                services.AddSingleton<IKubernetesRepository>(new MockKubernetesRepository());
            }
        }
        private class MockSimplePerformanceMonitorRepository : IPerformanceMonitorRepository
        {
            public Task<PerformanceHeartbeat> PulseAsync(int currentWorkerCount)
            {
                return Task.FromResult(new PerformanceHeartbeat());
            }
        }

        private class MockKubernetesRepository : IKubernetesRepository
        {
            public Task<int> GetNumberOfPodAsync(string deploymentName, string nameSpace)
            {
                return Task.FromResult(1);
            }
        }
    }
}
