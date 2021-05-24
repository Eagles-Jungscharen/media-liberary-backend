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
    public class PictureItemApi
    {
        private JWTAuthService _jwtAuthService;
        private HttpClient _httpClient;
        private PictureBlobStorageService _pictureBlobStorageService;

        public PictureItemApi(JWTAuthService authService, HttpClient httpClient, PictureBlobStorageService pictureBlobStorageService) {
            this._jwtAuthService = authService;
            this._httpClient = httpClient;
            this._pictureBlobStorageService = pictureBlobStorageService;

        }

        [FunctionName("pictureitem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "pictureitem/{id?}")] HttpRequest req,
            [Table("PictureCollectionDefinition")] CloudTable table,[Table("PictureItem")] CloudTable miTable, ILogger log, string id)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                PcdStoragService mcdStorageService = new PcdStoragService(table);
                PictureItemStorageService miStorageService = new PictureItemStorageService(miTable);

                if (req.Method == HttpMethods.Get) {
                    if (string.IsNullOrEmpty(id)) {
                        return new OkObjectResult(await miStorageService.GetAllItems());
                    } 
                    if (id == "@collection") {
                        string collectionId = req.Query["pid"];
                        if (string.IsNullOrEmpty(collectionId)) {
                            return new BadRequestObjectResult(new {error="Missing parameter pid"});
                        }

                        return new OkObjectResult(await miStorageService.GetAllItemForCollection(collectionId));
                    }
                    return new OkObjectResult(await miStorageService.GetItemById(id));
                } else {
                    PictureItem item = await frc.GetPayLoad<PictureItem>();
                    if (string.IsNullOrEmpty(item.PictureCollectionId)) {
                        return new BadRequestObjectResult(new {error= "No PictureCollecitonId provided!"});
                    }
                    PictureCollectionDefinition mcd = await mcdStorageService.GetPictureCollectionDefinitonById(item.PictureCollectionId,frc);
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
        [FunctionName("publishPictureItem")]
        public async Task<IActionResult> Publish(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publishPictureItem")] HttpRequest req,
            [Table("PictureItem")] CloudTable miTable, ILogger log)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                PictureItemStorageService miStorageService = new PictureItemStorageService(miTable);

                PictureItem item = await frc.GetPayLoad<PictureItem>();
                if (string.IsNullOrEmpty(item.Id)) {
                    return new BadRequestObjectResult(new {error= "No PictureId provided!"});
                }
                return new OkObjectResult(await miStorageService.Publish(item.Id,frc));
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
        [FunctionName("unpublishPictureItem")]
        public async Task<IActionResult> Unpublish(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "unpublishPictureItem")] HttpRequest req,
            [Table("PictureItem")] CloudTable miTable, ILogger log)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                PictureItemStorageService miStorageService = new PictureItemStorageService(miTable);

                PictureItem item = await frc.GetPayLoad<PictureItem>();
                if (string.IsNullOrEmpty(item.Id)) {
                    return new BadRequestObjectResult(new {error= "No Picture.Id provided!"});
                }
                return new OkObjectResult(await miStorageService.Unpublish(item.Id,frc));
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
        [FunctionName("updatePictureItem")]
        public async Task<IActionResult> UpdateMediaItemRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "updatePictureItem")] HttpRequest req,
            [Table("PictureItem")] CloudTable miTable, ILogger log)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                PictureItemStorageService miStorageService = new PictureItemStorageService(miTable);

                UpdatePictureItemRequest updateRequest = await frc.GetPayLoad<UpdatePictureItemRequest>();
                if (string.IsNullOrEmpty(updateRequest.PictureItemId)) {
                    return new BadRequestObjectResult(new {error= "No Picture.ID provided!"});
                }
                return new OkObjectResult(await miStorageService.UpdatePictureItem(updateRequest,frc));
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
        [FunctionName("deletePictureItemContent")]
        public async Task<IActionResult> DeleteRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "deletePictureItemContent")] HttpRequest req,
            [Table("PictureItem")] CloudTable miTable, ILogger log)
        {
            FunctionRunContext frc = new FunctionRunContext(req,_httpClient, log);
            try {
                await frc.CheckAuthentication(_jwtAuthService);
                PictureItemStorageService miStorageService = new PictureItemStorageService(miTable);

                UpdatePictureItemRequest updateRequest = await frc.GetPayLoad<UpdatePictureItemRequest>();
                if (string.IsNullOrEmpty(updateRequest.PictureItemId)) {
                    return new BadRequestObjectResult(new {error= "No PictureItemId provided!"});
                }
                bool result = _pictureBlobStorageService.DeleteMediaItemContent(updateRequest.PictureItemId, updateRequest.OriginalUrl, "original");
                frc.Log.LogInformation("Blob Deletion Result: "+result);
                return new OkObjectResult(await miStorageService.DeletePictureItemContent(updateRequest,frc));
            } catch(AuthenticationException e) {
                return new UnauthorizedObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
             catch(Exception e) {
                return new BadRequestObjectResult(new {error=e.Message, trace=e.StackTrace});
            }
        }
       [FunctionName("publicpicture")]
        public async Task<IActionResult> PublicMediaRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "public/collection/{id?}")] HttpRequest req,
            [Table("PictureItem")] CloudTable miTable, ILogger log, string id)
        {
            PictureItemStorageService miStorageService = new PictureItemStorageService(miTable);
            return new OkObjectResult(await miStorageService.GetAllPublicItemForCollection(id));
        }
   }
}
