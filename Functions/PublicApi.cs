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
    public static class PublicApi
    {
        [FunctionName("media")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "public/collection/{id?}")] HttpRequest req,
            [Table("MediaItem")] CloudTable miTable, ILogger log, string id)
        {
            MediaItemStorageService miStorageService = new MediaItemStorageService(miTable);
            return new OkObjectResult(await miStorageService.GetAllPublicItemForCollection(id));
        }
    }
}
