using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using EaglesJungscharen.MediaLibrary.Services;
using EaglesJungscharen.MediaLibrary.Models;

namespace EaglesJungscharen.MediaLibrary
{
    public static class Mcd
    {
        private static JWTAuthService _jwtAuthService;
        private static HttpClient httpClient = new HttpClient(new HttpClientHandler(){UseCookies=false});

        [FunctionName("mcd")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "mcd/{id?}")] HttpRequest req,
            [Table("MediaCollectionDefinition")] CloudTable table, ILogger log, string id)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            if (_jwtAuthService == null) {
                _jwtAuthService = new JWTAuthService(System.Environment.GetEnvironmentVariable("IDP_URL"));
            }
            McdStoragServcie mcdStorageService = new McdStoragServcie();
            User user = null;
            try {
                user = await _jwtAuthService.IsAuthencticated(req, httpClient, log);
            } catch(Exception e) {
                return new BadRequestObjectResult(e);
            }

            if (req.Method == HttpMethods.Get) {
                if (string.IsNullOrEmpty(id)) {
                    return new OkObjectResult(await mcdStorageService.GetAllMediaCollectionDefinitons(table));
                } else {
                    return new OkObjectResult(await mcdStorageService.GetMediaCollectionDefinitonById(table, id));
                }
            } else {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                MediaCollectionDefinition data = JsonConvert.DeserializeObject<MediaCollectionDefinition>(requestBody);
                log.LogInformation("Deser has worked...");
                return new OkObjectResult(await mcdStorageService.SaveMediaCollectionDefintion(table, data, log));
            }
        }
    }
}
