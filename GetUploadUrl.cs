using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EaglesJungscharen.MediaLibrary.Models;
using EaglesJungscharen.MediaLibrary.Services;

namespace EaglesJungscharen.MediaLibrary
{
    public static class GetUploadUrl
    {
        [FunctionName("GetUploadUrl")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GetUploadUrlRequest gur = JsonConvert.DeserializeObject<GetUploadUrlRequest>(requestBody);
            if (String.IsNullOrEmpty(gur.TargetMediaItemId) || String.IsNullOrEmpty(gur.MediaName)) {
                return new BadRequestObjectResult(new {status = "error", error="TargetMediaItemId and MediaName should not be null or empty"});
            } 
            BlobStorageService service = new BlobStorageService("media");
            string url = service.BuildUploadUrl(gur.TargetMediaItemId, gur.MediaName);
            return new OkObjectResult(new { url=url});
        }
    }
}
