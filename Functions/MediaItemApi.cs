using System;
using System.IO;
using System.Collections.Generic;
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
        private MediaBlobStorageService _mediaBlobStorageService;

        public MediaItemApi(JWTAuthService authService, HttpClient httpClient, MediaBlobStorageService mediaBlobStorageService) {
            this._jwtAuthService = authService;
            this._httpClient = httpClient;
            this._mediaBlobStorageService = mediaBlobStorageService;

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
                        return new OkObjectResult(EnrichMediaItemList(await miStorageService.GetAllItems()));
                    } 
                    if (id == "@collection") {
                        string collectionId = req.Query["cid"];
                        if (string.IsNullOrEmpty(collectionId)) {
                            return new BadRequestObjectResult(new {error="Missing parameter cid"});
                        }

                        return new OkObjectResult(EnrichMediaItemList(await miStorageService.GetAllItemForCollection(collectionId)));
                    }
                    return new OkObjectResult(EnrichMediaItem(await miStorageService.GetItemById(id)));
                } else {
                    MediaItem item = await frc.GetPayLoad<MediaItem>();
                    if (string.IsNullOrEmpty(item.MediaCollectionId)) {
                        return new BadRequestObjectResult(new {error= "No MediaCollecitonId provided!"});
                    }
                    MediaCollectionDefinition mcd = await mcdStorageService.GetMediaCollectionDefinitonById(item.MediaCollectionId,frc);
                    miStorageService.Validate(item,mcd);
                    return new OkObjectResult(EnrichMediaItem(await miStorageService.Save(item,frc)));
                }
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
        [FunctionName("publishMediaItem")]
        public async Task<IActionResult> Publish(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publishMediaItem")] HttpRequest req,
            [Table("MediaItem")] CloudTable miTable, ILogger log)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                MediaItemStorageService miStorageService = new MediaItemStorageService(miTable);

                MediaItem item = await frc.GetPayLoad<MediaItem>();
                if (string.IsNullOrEmpty(item.Id)) {
                    return new BadRequestObjectResult(new {error= "No MediaCollecitonId provided!"});
                }
                return new OkObjectResult(EnrichMediaItem(await miStorageService.Publish(item.Id,frc)));
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
         [FunctionName("unpublishMediaItem")]
        public async Task<IActionResult> Unpublish(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "unpublishMediaItem")] HttpRequest req,
            [Table("MediaItem")] CloudTable miTable, ILogger log)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                MediaItemStorageService miStorageService = new MediaItemStorageService(miTable);

                MediaItem item = await frc.GetPayLoad<MediaItem>();
                if (string.IsNullOrEmpty(item.Id)) {
                    return new BadRequestObjectResult(new {error= "No MediaCollecitonId provided!"});
                }
                return new OkObjectResult(EnrichMediaItem(await miStorageService.Unpublish(item.Id,frc)));
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
        [FunctionName("updateMediaItem")]
        public async Task<IActionResult> UpdateMediaItemRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "updateMediaItem")] HttpRequest req,
            [Table("MediaItem")] CloudTable miTable, ILogger log)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                MediaItemStorageService miStorageService = new MediaItemStorageService(miTable);

                UpdateMediaItemRequest updateRequest = await frc.GetPayLoad<UpdateMediaItemRequest>();
                if (string.IsNullOrEmpty(updateRequest.MediaItemId)) {
                    return new BadRequestObjectResult(new {error= "No MediaItemId provided!"});
                }
                return new OkObjectResult(EnrichMediaItem(await miStorageService.UpdateMediaItem(updateRequest,frc)));
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
        [FunctionName("deleteMediaItemContent")]
        public async Task<IActionResult> DeleteRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "deleteMediaItemContent")] HttpRequest req,
            [Table("MediaItem")] CloudTable miTable, ILogger log)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                MediaItemStorageService miStorageService = new MediaItemStorageService(miTable);

                UpdateMediaItemRequest updateRequest = await frc.GetPayLoad<UpdateMediaItemRequest>();
                if (string.IsNullOrEmpty(updateRequest.MediaItemId)) {
                    return new BadRequestObjectResult(new {error= "No MediaItemId provided!"});
                }
                bool result = _mediaBlobStorageService.DeleteMediaItemContent(updateRequest.MediaItemId, updateRequest.MediaName, updateRequest.MediaKey);
                frc.Log.LogInformation("Blob Deletion Result: "+result);
                return new OkObjectResult(EnrichMediaItem(await miStorageService.DeleteMediaItemContent(updateRequest,frc)));
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
       [FunctionName("publicmedia")]
        public async Task<IActionResult> PublicMediaRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "public/collection/{id?}")] HttpRequest req,
            [Table("MediaItem")] CloudTable miTable, ILogger log, string id)
        {
            MediaItemStorageService miStorageService = new MediaItemStorageService(miTable);
            List<MediaItem> items = EnrichMediaItemList(await miStorageService.GetAllPublicItemForCollection(id));
            items.Sort((item1,item2)=>item2.ItemDate.CompareTo(item1.ItemDate));
            return new OkObjectResult(items);
        }
 
        [FunctionName("me")]
        public async Task<IActionResult> MeRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me")] HttpRequest req, ILogger log)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                return new OkObjectResult(frc.User);
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
 

        private MediaItemEntry EnrichMediaItemEntry (MediaItemEntry entry) {
            if (!String.IsNullOrEmpty(entry.Value)) {
                entry.DownloadUrl = _mediaBlobStorageService.BuildDownloadUrl(entry.MediaItemId, entry.Value,entry.CollectionItemKey);
            }
            return entry;
        }
        private MediaItem EnrichMediaItem(MediaItem item) {
            item.Entries.ForEach(entry=>EnrichMediaItemEntry(entry));
            return item;
        }
        private List<MediaItem> EnrichMediaItemList(List<MediaItem> items) {
            items.ForEach(item=>EnrichMediaItem(item));
            return items;
        }
   }
}
