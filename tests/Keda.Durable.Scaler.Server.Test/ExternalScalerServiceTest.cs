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
            string expectedName = "foo";
            string expectedNamespace = "bar";
            var kubernetesRepositoryMock = new Mock<IKubernetesRepository>();
            var performanceMonitorRepositoryMock = new Mock<IPerformanceMonitorRepository>();

            var performanceHeartbeat = new PerformanceHeartbeat();
            var scaleRecommendation = CreateScaleRecommendation(ScaleAction.AddWorker, inputKeepWorkersAlive, "baz");
            SetScaleRecommendation(performanceHeartbeat, scaleRecommendation);

            performanceMonitorRepositoryMock.Setup(p => p.PulseAsync(inputCurrentWorkerCount)).ReturnsAsync(performanceHeartbeat);
            kubernetesRepositoryMock.Setup(p => p.GetNumberOfPodAsync(expectedName, expectedNamespace)).ReturnsAsync(inputCurrentWorkerCount);
            var loggerMock = new Mock<ILogger<ExternalScalerService>>();
            var service = new ExternalScalerService(performanceMonitorRepositoryMock.Object, kubernetesRepositoryMock.Object, loggerMock.Object);

            var request = CreateScaleObjectRef(expectedName, expectedNamespace);

            Assert.Equal(expectedIsAlive, (await service.IsActive(request, new ServerCallContextImpl())).Result);
        }

        private ScaleRecommendation CreateScaleRecommendation(ScaleAction scaleAction, bool keepWorkersAlive, string reason)
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
        public async Task GetMetricsWorksAsExpectedAsync(ScaleAction action, int currentWorkerCount, int targetCount) 
        {

        }

        private ScaledObjectRef CreateScaleObjectRef(string name, string nameSpace)
        {
            return new ScaledObjectRef
            {
                Name = name,
                Namespace = nameSpace
            };
        }

        private void SetScaleRecommendation(PerformanceHeartbeat performanceHeartbeat,
            ScaleRecommendation scaleRecommendation)
        {
            var t = typeof(PerformanceHeartbeat);
            var prop = t.GetProperty("ScaleRecommendation");
            prop.SetValue(performanceHeartbeat, scaleRecommendation);
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
