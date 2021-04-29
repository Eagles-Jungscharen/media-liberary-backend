using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EaglesJungscharen.MediaLibrary.Models;
using EaglesJungscharen.MediaLibrary.Services;

namespace EaglesJungscharen.MediaLibrary
{
    public static class GetUploadUrl
    {
        private static JWTAuthService _jwtAuthService;
        private static HttpClient httpClient = new HttpClient(new HttpClientHandler(){UseCookies=false});
        [FunctionName("GetUploadUrl")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            if (_jwtAuthService == null) {
                _jwtAuthService = new JWTAuthService(System.Environment.GetEnvironmentVariable("IDP_URL"),System.Environment.GetEnvironmentVariable("ADMIN_SCOPE"),System.Environment.GetEnvironmentVariable("CONTRIBUTOR_SCOPE"));
            }
            User user = null;
            try {
                user = await _jwtAuthService.IsAuthencticated(req, httpClient, log);
            } catch(Exception e) {
                return new BadRequestObjectResult(e);
            }
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GetUploadUrlRequest gur = JsonConvert.DeserializeObject<GetUploadUrlRequest>(requestBody);
            if (String.IsNullOrEmpty(gur.TargetMediaItemId) || String.IsNullOrEmpty(gur.MediaName) || String.IsNullOrEmpty(gur.MediaKey)) {
                return new BadRequestObjectResult(new {status = "error", error="TargetMediaItemId, MediaName and MediaKey should not be null or empty"});
            } 
            BlobStorageService service = new BlobStorageService("media");
            string url = service.BuildUploadUrl(gur.TargetMediaItemId, gur.MediaName, gur.MediaKey);
            return new OkObjectResult(new { url=url});
        }
    }
}
