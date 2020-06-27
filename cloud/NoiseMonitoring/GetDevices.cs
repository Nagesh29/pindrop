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
    public static class GetDevices
    {
        public class DeviceInfo
        {
            public string DeviceId { get; set; }
            public TwinCollection thresholdProps { get; set; }
            public int minValue { get; set; }
            public int maxValue { get; set; }
            public int amberThreshold { get; set; }
            public int redThreshold { get; set; }
        }
        private static RegistryManager registryManager;
        [FunctionName("GetDevices")]
        public static async Task<Object> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                var iotHubServiceConnectionString = Environment.GetEnvironmentVariable("iotHubServiceConnectionString");
                registryManager = RegistryManager.CreateFromConnectionString(iotHubServiceConnectionString);
                string devices = "";
                var query = registryManager.CreateQuery("SELECT * FROM devices");
                while (query.HasMoreResults)
                {
                    var result = await query.GetNextAsTwinAsync();
                    List<DeviceInfo> deviceList = new List<DeviceInfo>();
                    foreach (var twin in result)
                    {
                        DeviceInfo deviceInfo = new DeviceInfo();
                        deviceInfo.DeviceId = twin.DeviceId;
                        deviceInfo.thresholdProps = twin.Properties.Desired;
                        deviceList.Add(deviceInfo);
                    }
                    devices = JsonConvert.SerializeObject(deviceList);
                }

                return (object)new OkObjectResult(devices);
            }
            catch (System.Exception e)
            {
                return (object)new ExceptionResult(e, true);
            }
        }
    }
}
