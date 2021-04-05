using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using EaglesJungscharen.MediaLibrary.Services;
using EaglesJungscharen.MediaLibrary.Models;
using EaglesJungscharen.MediaLibrary.Utils;

namespace EaglesJungscharen.MediaLibrary
{
    public class McdApi
    {
        private JWTAuthService _jwtAuthService;
        private HttpClient _httpClient = new HttpClient(new HttpClientHandler(){UseCookies=false});

        public McdApi(JWTAuthService service, HttpClient client) {
            _jwtAuthService = service;
            _httpClient = client;
        }

        [FunctionName("mcd")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "mcd/{id?}")] HttpRequest req,
            [Table("MediaCollectionDefinition")] CloudTable table, ILogger log, string id)
        {
            FunctionRunContext frc = new FunctionRunContext(req, _httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                McdStoragService mcdStorageService = new McdStoragService(table);
                if (req.Method == HttpMethods.Get) {
                    if (string.IsNullOrEmpty(id)) {
                        return new OkObjectResult(await mcdStorageService.GetAllMediaCollectionDefinitons(frc));
                    } else {
                        return new OkObjectResult(await mcdStorageService.GetMediaCollectionDefinitonById(id, frc));
                    }
                } else {
                    MediaCollectionDefinition data = await frc.GetPayLoad<MediaCollectionDefinition>();
                    return new OkObjectResult(await mcdStorageService.SaveMediaCollectionDefintion(data, frc));
                }
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
            catch(Exception e) {
                return new BadRequestObjectResult(e);
            }
        }
    }
}
