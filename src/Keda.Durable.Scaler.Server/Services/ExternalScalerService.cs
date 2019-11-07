using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableTask.AzureStorage.Monitoring;
using Dynamitey.Internal.Optimization;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Keda.Durable.Scaler.Server.Protos;
using Keda.Durable.Scaler.Server.Repository;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Keda.Durable.Scaler.Server.Services
{
    public class ExternalScalerService : ExternalScaler.ExternalScalerBase
    {
        private const string ScaleRecommendation = "ScaleRecommendation";
        private IPerformanceMonitorRepository _performanceMonitorRepository;
        private IKubernetesRepository _kubernetesRepository;
        private readonly ILogger<ExternalScalerService> _logger;

        public ExternalScalerService(IPerformanceMonitorRepository performanceMonitorRepository, IKubernetesRepository kubernetesRepository, ILogger<ExternalScalerService> logger)
        {
            _performanceMonitorRepository = performanceMonitorRepository;
            _kubernetesRepository = kubernetesRepository;
            _logger = logger;
        }
        public override Task<Empty> New(NewRequest request, ServerCallContext context)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var requestOjbect = JsonConvert.SerializeObject(request, settings);
            var contextObject = JsonConvert.SerializeObject(context, settings);
            _logger.LogInformation("******* requestObject");
            _logger.LogInformation(requestOjbect);
            _logger.LogInformation("***** contextObject");
            _logger.LogInformation(contextObject);
            // We don't need to do something in here. 
            return Task.FromResult(new Empty());
        }

        public override async Task<IsActiveResponse> IsActive(ScaledObjectRef request, ServerCallContext context)
        {
            // True or false if the deployment work in progress. 
            var heartbeat = await _performanceMonitorRepository.PulseAsync(await GetCurrentWorkerCountAsync());
            var response = new IsActiveResponse();
            response.Result = true;
            return response;
        }

        public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef request, ServerCallContext context)
        {
            var response = new GetMetricSpecResponse();
            var fields = new RepeatedField<MetricSpec>();
            fields.Add(new MetricSpec()
            {
                MetricName = ScaleRecommendation,
                TargetSize = 0
            });
            return base.GetMetricSpec(request, context);
        }

        public override async Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
        {
            var heartbeat = await _performanceMonitorRepository.PulseAsync(await GetCurrentWorkerCountAsync());
            int targetSize = 0;
            switch (heartbeat.ScaleRecommendation.Action)
            {
                case ScaleAction.AddWorker:
                    targetSize = 1;
                    break;
                case ScaleAction.RemoveWorker:
                    targetSize = -1;
                    break;
                default:
                    break;
            }
            var res = new GetMetricsResponse();
            var metricValue = new MetricValue
            {
                MetricName = ScaleRecommendation,
                MetricValue_ = targetSize
            };
            res.MetricValues.Add(metricValue);
            return res;
            // Return the value that is 
            //var res = new GetMetricsResponse();
            //var metricValue = new MetricValue();
            //metricValue.MetricName = "hello";
            //metricValue.MetricValue_ = 10;
            //res.MetricValues.Add(metricValue);
            //return Task.FromResult(res);
        }

        public override Task<Empty> Close(ScaledObjectRef request, ServerCallContext context)
        {
            // We don't need to do something in here. 
            return Task.FromResult(new Empty());
        }

        private Task<int> GetCurrentWorkerCountAsync()
        {
            return _kubernetesRepository.GetNumberOfPodAsync("durable-keda", "default");
        }

    }
}
