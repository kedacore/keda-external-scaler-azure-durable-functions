using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace durable_keda
{
    public static class LoadOrchestration
    {
        [FunctionName("LoadOrchestration")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("LoadOrchestration_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("LoadOrchestration_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("LoadOrchestration_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("LoadOrchestration_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("LoadOrchestration", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
        [FunctionName("LoadOrchestration_Hello")]
        public static async Task<string> SayHelloAsync([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation("Now Stopping 10 sec.");
            await Task.Delay(10 * 1000);
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }
    }
}