using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Keda.Durable.Scaler.Server.Protos;

namespace Keda.Durable.Scaler.Server.Services
{
    public class ExternalScalerService : ExternalScaler.ExternalScalerBase
    {
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
            return base.GetMetrics(request, context);
        }

        public override Task<Empty> Close(ScaledObjectRef request, ServerCallContext context)
        {
            return base.Close(request, context);
        }
    }
}
