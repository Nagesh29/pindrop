using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using NoiseMonitoring;
using Microsoft.Azure.Devices;
using System.Text;
using Newtonsoft.Json;

namespace Company.Function
{
    public static class SendThreshold
    {
        static ServiceClient serviceClient;

        [FunctionName("SendThreshold")]
        public static async void Run(
            [ServiceBusTrigger("noise-monitor-topic", "noise-monitor-subscription", Connection = "ServiceBusNamespaceConnectionString")] Microsoft.Azure.ServiceBus.Message messageObj,
            [Table("threshold", "Threshold", "{userProperties.iothub-connection-device-id}", Connection = "AzureTableStorageConnectionString")] Threshold thresholdObj,
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {thresholdObj}");
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {messageObj}");

            var iotHubServiceConnString = Environment.GetEnvironmentVariable("iotHubServiceConnectionString");
            serviceClient = ServiceClient.CreateFromConnectionString(iotHubServiceConnString);

            var jsonObj = JsonConvert.SerializeObject(thresholdObj);
            var thresholdMessage = new Microsoft.Azure.Devices.Message(Encoding.UTF8.GetBytes(jsonObj));
            thresholdMessage.ContentEncoding = "utf-8";
            thresholdMessage.ContentType = "application/json";

            await serviceClient.SendAsync(messageObj.UserProperties["iothub-connection-device-id"].ToString(), thresholdMessage);
        }
    }
}
