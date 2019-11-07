using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableTask.AzureStorage.Monitoring;
using Microsoft.WindowsAzure.Storage;

namespace Keda.Durable.Scaler.Server.Repository
{
    public interface IPerformanceMonitorRepository
    {
        Task<PerformanceHeartbeat> PulseAsync(int currentWorkerCount);
    }
    public class PerformanceMonitorRepository : IPerformanceMonitorRepository
    {
        private DurableTaskContext _context;
        private DisconnectedPerformanceMonitor _monitor;
        public PerformanceMonitorRepository(DurableTaskContext context)
        {
            _context = context;
            SetStorageAccount(context);
        }

        private void SetStorageAccount(DurableTaskContext context)
        {
            var storageAccount = CloudStorageAccount.Parse(context.StorageAccount);
            _monitor = new DisconnectedPerformanceMonitor(storageAccount, context.TaskHub, context.MaxPollingIntervalMillisecond);
        }
        
        public Task<PerformanceHeartbeat> PulseAsync(int currentWorkerCount)
        {
            return _monitor.PulseAsync(currentWorkerCount);
        }
    }
}
