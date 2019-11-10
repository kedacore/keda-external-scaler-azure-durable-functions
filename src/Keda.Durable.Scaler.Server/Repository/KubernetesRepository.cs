using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using k8s;

namespace Keda.Durable.Scaler.Server.Repository
{
    public interface IKubernetesRepository
    {  
        Task<int> GetNumberOfPodAsync(string deploymentName, string nameSpace);
    }

    public class KubernetesRepository : IKubernetesRepository
    {
        private const string KUBECONFIG = "kubeconfig";
        private IKubernetes _client;

        public KubernetesRepository()
        {
            Setup();
        }

        private void Setup()
        {
            KubernetesClientConfiguration config;
            if (File.Exists(KUBECONFIG))
            {
                config = KubernetesClientConfiguration.BuildConfigFromConfigFile(KUBECONFIG);
            }
            else
            {
                config = KubernetesClientConfiguration.InClusterConfig();
            }
            _client = new Kubernetes(config);
        }
        public async Task<int> GetNumberOfPodAsync(string deploymentName, string nameSpace)
        {
            var res = await _client.ReadNamespacedDeploymentStatusWithHttpMessagesAsync(deploymentName, nameSpace);
            res.Response.EnsureSuccessStatusCode();
            return res.Body.Status.AvailableReplicas ?? 0;
        }        
    }
}
