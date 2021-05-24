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
    public class GetUploadUrl
    {
        private JWTAuthService _jwtAuthService;
        private HttpClient _httpClient = new HttpClient(new HttpClientHandler(){UseCookies=false});

        private MediaBlobStorageService _mediaBlobStorageService;
        private PictureBlobStorageService _pictureBlobStorageService;

        public GetUploadUrl(JWTAuthService authService, HttpClient httpClient, MediaBlobStorageService mediaBlobStorageService, PictureBlobStorageService pictureBlobStorageService) {
            this._jwtAuthService = authService;
            this._httpClient = httpClient;
            this._mediaBlobStorageService = mediaBlobStorageService;
            this._pictureBlobStorageService = pictureBlobStorageService;
        }
        [FunctionName("GetUploadUrl")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            User user = null;
            try {
                user = await _jwtAuthService.IsAuthencticated(req, _httpClient, log);
            } catch(Exception e) {
                return new BadRequestObjectResult(e);
            }
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GetUploadUrlRequest gur = JsonConvert.DeserializeObject<GetUploadUrlRequest>(requestBody);
            if (String.IsNullOrEmpty(gur.TargetMediaItemId) || String.IsNullOrEmpty(gur.MediaName) || String.IsNullOrEmpty(gur.MediaKey)) {
                return new BadRequestObjectResult(new {status = "error", error="TargetMediaItemId, MediaName and MediaKey should not be null or empty"});
            } 
            string url = _mediaBlobStorageService.BuildUploadUrl(gur.TargetMediaItemId, gur.MediaName, gur.MediaKey);
            return new OkObjectResult(new { url=url});
        }

        [FunctionName("GetPictureUploadUrl")]
        public async Task<IActionResult> RunPictureUploadUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            User user = null;
            try {
                user = await _jwtAuthService.IsAuthencticated(req, _httpClient, log);
            } catch(Exception e) {
                return new BadRequestObjectResult(e);
            }
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GetPictureUploadUrlRequest gur = JsonConvert.DeserializeObject<GetPictureUploadUrlRequest>(requestBody);
            if (String.IsNullOrEmpty(gur.TargetPictureItemId) || String.IsNullOrEmpty(gur.MediaName)) {
                return new BadRequestObjectResult(new {status = "error", error="TargetMediaItemId, MediaName and MediaKey should not be null or empty"});
            } 
            string url = _pictureBlobStorageService.BuildUploadUrl(gur.TargetPictureItemId, gur.MediaName, "original");
            return new OkObjectResult(new { url=url});
        }
    }
}
