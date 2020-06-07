using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.Web.Http;

namespace NoiseMonitoring
{
    public class Threshold : TableEntity
    {
        public Threshold()
        {
            PartitionKey = "Threshold";
            RowKey = DeviceID;
        }
        public string ThresholdValue { get; set; }
        public string DeviceID
        {
            get
            {
                return this.RowKey;
            }
            set
            {
                this.RowKey = value;
            }
        }
    }
    public static class SetThreshold
    {
        [FunctionName("SetThreshold")]
        public static async Task<Object> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table("threshold", Connection = "AzureTableStorageConnectionString")] CloudTable table,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function ...");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var thresholdObj = JsonConvert.DeserializeObject<Threshold>(requestBody);
                log.LogInformation($"Threshold: {thresholdObj.ThresholdValue} received");
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(thresholdObj);
                //TableOperation insertOperation = TableOperation.Insert(thresholdObj);
                var result = await table.ExecuteAsync(insertOrReplaceOperation);
                return (object)new OkObjectResult(result);
            }
            catch (System.Exception e)
            {
                return (object)new ExceptionResult(e, true);
            }

        }
    }
}
