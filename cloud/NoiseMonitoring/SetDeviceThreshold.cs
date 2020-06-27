using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System.Collections.Generic;
using System.Web.Http;

namespace NoiseMonitoring
{
    public class DesiredPropertyRequest
    {
        public string deviceId { get; set; }
        public Dictionary<string, Object> DesiredProperties { get; set; }
    }
    public static class SetDeviceThreshold
    {
        private static RegistryManager registryManager;
        [FunctionName("SetDeviceThreshold")]
        public static async Task<object> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var desiredPropertyRequestObj = JsonConvert.DeserializeObject<DesiredPropertyRequest>(requestBody);

                string deviceId = desiredPropertyRequestObj.deviceId;

                var iotHubServiceConnectionString = Environment.GetEnvironmentVariable("iotHubServiceConnectionString");
                registryManager = RegistryManager.CreateFromConnectionString(iotHubServiceConnectionString);
                Twin deviceObj = await registryManager.GetTwinAsync(deviceId);
                foreach (var item in desiredPropertyRequestObj.DesiredProperties.Keys)
                {
                    deviceObj.Properties.Desired[item] = desiredPropertyRequestObj.DesiredProperties[item];
                }

                Twin updatedObj = await registryManager.UpdateTwinAsync(deviceId, deviceObj, deviceObj.ETag);

                return (object)new OkObjectResult(deviceObj.Properties.Desired);
            }
            catch (System.Exception e)
            {
                return (object)new ExceptionResult(e, true);
            }

        }
    }
}
