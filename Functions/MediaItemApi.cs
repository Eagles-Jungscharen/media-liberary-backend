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
using EaglesJungscharen.MediaLibrary.Utils;

namespace EaglesJungscharen.MediaLibrary
{
    public class MediaItemApi
    {
        private JWTAuthService _jwtAuthService;
        private HttpClient _httpClient;

        public MediaItemApi(JWTAuthService authService, HttpClient httpClient) {
            this._jwtAuthService = authService;
            this._httpClient = httpClient;
        }

        [FunctionName("mediaitem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "mediaitem/{id?}")] HttpRequest req,
            [Table("MediaCollectionDefinition")] CloudTable table,[Table("MediaItem")] CloudTable miTable, ILogger log, string id)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                McdStoragService mcdStorageService = new McdStoragService(table);
                MediaItemStorageService miStorageService = new MediaItemStorageService(miTable);

                if (req.Method == HttpMethods.Get) {
                    if (string.IsNullOrEmpty(id)) {
                        return new OkObjectResult(await miStorageService.GetAllItems());
                    } 
                    if (id == "@collection") {
                        string collectionId = req.Query["cid"];
                        if (string.IsNullOrEmpty(collectionId)) {
                            return new BadRequestObjectResult(new {error="Missing parameter cid"});
                        }
                        return new OkObjectResult(await miStorageService.GetAllItemForCollection(collectionId));
                    }
                    return new OkObjectResult(await miStorageService.GetItemById(id));
                } else {
                    MediaItem item = await frc.GetPayLoad<MediaItem>();
                    if (string.IsNullOrEmpty(item.MediaCollectionId)) {
                        return new BadRequestObjectResult(new {error= "No MediaCollecitonId provided!"});
                    }
                    MediaCollectionDefinition mcd = await mcdStorageService.GetMediaCollectionDefinitonById(item.MediaCollectionId,frc);
                    miStorageService.Validate(item,mcd);
                    return new OkObjectResult(await miStorageService.Save(item,frc));
                }
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
    }
}
