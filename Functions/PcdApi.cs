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
    public class PcdApi
    {
        private JWTAuthService _jwtAuthService;
        private HttpClient _httpClient = new HttpClient(new HttpClientHandler(){UseCookies=false});

        public PcdApi(JWTAuthService service, HttpClient client) {
            _jwtAuthService = service;
            _httpClient = client;
        }

        [FunctionName("pcd")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "pcd/{id?}")] HttpRequest req,
            [Table("PictureCollectionDefinition")] CloudTable table, ILogger log, string id)
        {
            FunctionRunContext frc = new FunctionRunContext(req, _httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                PcdStoragService pcdStorageService = new PcdStoragService(table);
                if (req.Method == HttpMethods.Get) {
                    if (string.IsNullOrEmpty(id)) {
                        return new OkObjectResult(await pcdStorageService.GetAllPictureCollectionDefinitons(frc));
                    } else {
                        return new OkObjectResult(await pcdStorageService.GetPictureCollectionDefinitonById(id, frc));
                    }
                } else {
                    PictureCollectionDefinition data = await frc.GetPayLoad<PictureCollectionDefinition>();
                    return new OkObjectResult(await pcdStorageService.SavePictureCollectionDefintion(data, frc));
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
