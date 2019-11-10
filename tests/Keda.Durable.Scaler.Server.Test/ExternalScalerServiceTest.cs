using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DurableTask.AzureStorage.Monitoring;
using Grpc.Core;
using Keda.Durable.Scaler.Server.Protos;
using Keda.Durable.Scaler.Server.Repository;
using Keda.Durable.Scaler.Server.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Keda.Durable.Scaler.Server.Test
{
    public class ExternalScalerServiceTest
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task IsActiveWorksAsExpectedAsync(bool inputKeepWorkersAlive, bool expectedIsAlive)
        {
            int inputCurrentWorkerCount = 1;
            var fixture = new ServiceFixture(ScaleAction.AddWorker, inputCurrentWorkerCount, inputKeepWorkersAlive);
            var service = fixture.ExternalScaleService;
            expectedIsAlive = true; // TODO remove this line once white list implemented.
            Assert.Equal(expectedIsAlive, (await service.IsActive(fixture.ScaleObjectRef, fixture.ServerCallContext)).Result);
        }

        private static ScaleRecommendation CreateScaleRecommendation(ScaleAction scaleAction, bool keepWorkersAlive, string reason)
        {
            var s = typeof(ScaleRecommendation);
            var ctor = s.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
            return ctor.Invoke(new object[]
            {
                scaleAction,
                keepWorkersAlive,
                reason
            }) as ScaleRecommendation;
        }

        [Theory]
        [InlineData(ScaleAction.AddWorker, 1, 2)]
        [InlineData(ScaleAction.AddWorker, 2, 3)]
        [InlineData(ScaleAction.RemoveWorker, 3, 2)]
        [InlineData(ScaleAction.None, 3, 3)]
        public async Task GetMetricsWorksAsExpectedAsync(ScaleAction action, int currentWorkerCount, long targetCount)
        {
            var fixture = new ServiceFixture(action, currentWorkerCount);
            var service = fixture.ExternalScaleService;
            var response = await service.GetMetrics(fixture.GetMetricsRequest, fixture.ServerCallContext);
            Assert.Equal(targetCount, response.MetricValues.First().MetricValue_);
        }

        private static ScaledObjectRef CreateScaleObjectRef(string name, string nameSpace)
        {
            return new ScaledObjectRef
            {
                Name = name,
                Namespace = nameSpace
            };
        }

        private static void SetScaleRecommendation(PerformanceHeartbeat performanceHeartbeat,
            ScaleRecommendation scaleRecommendation)
        {
            var t = typeof(PerformanceHeartbeat);
            var prop = t.GetProperty("ScaleRecommendation");
            prop.SetValue(performanceHeartbeat, scaleRecommendation);
        }

        private class ServiceFixture
        {
            private readonly Mock<IKubernetesRepository> _kubernetesRepositoryMock;
            private readonly Mock<IPerformanceMonitorRepository> _performanceMonitorRepositoryMock;
            private readonly Mock<ILogger<ExternalScalerService>> _loggerMock;

            public ExternalScalerService ExternalScaleService => new ExternalScalerService(_performanceMonitorRepositoryMock.Object, _kubernetesRepositoryMock.Object, _loggerMock.Object);

            public string ExpectedName => "foo";
            public string ExpectedNamespace => "bar";

            public ServerCallContext ServerCallContext => new ServerCallContextImpl();

            public ScaledObjectRef ScaleObjectRef =>
                CreateScaleObjectRef(ExpectedName, ExpectedNamespace);

            public GetMetricsRequest GetMetricsRequest
            {
                get
                {
                    var request = new GetMetricsRequest();

                    request.ScaledObjectRef = this.ScaleObjectRef;
                    return request;
                }

            }

            public ServiceFixture(ScaleAction action, int currentWorkerCount, bool inputKeepWorkersAlive = true)
            {
                _kubernetesRepositoryMock = new Mock<IKubernetesRepository>();
                _performanceMonitorRepositoryMock = new Mock<IPerformanceMonitorRepository>();

                var performanceHeartbeat = new PerformanceHeartbeat();
                var scaleRecommendation = CreateScaleRecommendation(action, inputKeepWorkersAlive, "baz");
                SetScaleRecommendation(performanceHeartbeat, scaleRecommendation);
                _performanceMonitorRepositoryMock.Setup(p => p.PulseAsync(currentWorkerCount)).ReturnsAsync(performanceHeartbeat);
                _kubernetesRepositoryMock.Setup(p => p.GetNumberOfPodAsync(ExpectedName, ExpectedNamespace)).ReturnsAsync(currentWorkerCount);
                _loggerMock = new Mock<ILogger<ExternalScalerService>>();
            }
        }

        private class ServerCallContextImpl : ServerCallContext
        {
            protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
            {
                throw new NotImplementedException();
            }

            protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
            {
                throw new NotImplementedException();
            }

            protected override string MethodCore { get; }
            protected override string HostCore { get; }
            protected override string PeerCore { get; }
            protected override DateTime DeadlineCore { get; }
            protected override Metadata RequestHeadersCore { get; }
            protected override CancellationToken CancellationTokenCore { get; }
            protected override Metadata ResponseTrailersCore { get; }
            protected override Status StatusCore { get; set; }
            protected override WriteOptions WriteOptionsCore { get; set; }
            protected override AuthContext AuthContextCore { get; }
        }
    }
}
