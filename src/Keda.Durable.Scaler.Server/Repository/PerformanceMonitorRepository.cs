using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableTask.AzureStorage.Monitoring;

namespace Keda.Durable.Scaler.Server.Repository
{
    public interface IPerformanceMonitorRepository
    {
        Task<PerformanceHeartbeat> PulseAsync(int currentWorkerCount);
    }
    public class PerformanceMonitorRepository : IPerformanceMonitorRepository
    {
        private DurableTaskContext _context;
        public PerformanceMonitorRepository(DurableTaskContext context)
        {
            _context = context;
        }
        public Task<PerformanceHeartbeat> PulseAsync(int currentWorkerCount)
        {
            // TODO Implement access to the DurableTask.PulseAsync
            return Task.FromResult(new PerformanceHeartbeat());
        }
    }
}
