using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Keda.Durable.Scaler.Server.Repository
{
    public class DurableTaskContext
    {
        public string TaskHub { get; set; }
        public string StorageAccount { get; set; }
        public int? MaxPollingIntervalMillisecond { get; set; } = 5000;
    }
}
