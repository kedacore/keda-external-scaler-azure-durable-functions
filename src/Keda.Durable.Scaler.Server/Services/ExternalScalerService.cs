using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Keda.Durable.Scaler.Server.Protos;
using Keda.Durable.Scaler.Server.Repository;

namespace Keda.Durable.Scaler.Server.Services
{
    public class ExternalScalerService : ExternalScaler.ExternalScalerBase
    {
        public ExternalScalerService(IPerformanceMonitorRepository repository)
        {

        }
        public override Task<Empty> New(NewRequest request, ServerCallContext context)
        {
            return base.New(request, context);
        }

        public override Task<IsActiveResponse> IsActive(ScaledObjectRef request, ServerCallContext context)
        {
            return base.IsActive(request, context);
        }

        public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef request, ServerCallContext context)
        {
            return base.GetMetricSpec(request, context);
        }

        public override Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
        {
            var res = new GetMetricsResponse();
            var metricValue = new MetricValue();
            metricValue.MetricName = "hello";
            metricValue.MetricValue_ = 10;
            res.MetricValues.Add(metricValue);
            return Task.FromResult(res);


        }

        public override Task<Empty> Close(ScaledObjectRef request, ServerCallContext context)
        {
            return base.Close(request, context);
        }
    }
}
