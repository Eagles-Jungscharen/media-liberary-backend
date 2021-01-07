using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using WebGate.Azure.CloudTableUtils.CloudTableExtension;

using EaglesJungscharen.MediaLibrary.Models;

namespace EaglesJungscharen.MediaLibrary.Services {

    public class McdStoragServcie {

        public async Task<List<MediaCollectionDefinition>> GetAllMediaCollectionDefinitons(CloudTable table) {
            return await table.GetAllAsync<MediaCollectionDefinition>("mcd");
        }

        public async Task<MediaCollectionDefinition> GetMediaCollectionDefinitonById(CloudTable table, string id) {
            return await table.GetByIdAsync<MediaCollectionDefinition>(id, "mcd");
        }

        public async Task<MediaCollectionDefinition> SaveMediaCollectionDefintion(CloudTable table, MediaCollectionDefinition mcd, ILogger logger) {
            if (mcd.Id == "@new" || string.IsNullOrEmpty(mcd.Id)) {
                mcd.Id = Guid.NewGuid().ToString();
            }
            
            logger.LogInformation("ID ist nun: "+ mcd.Id);
            TableResult result = await table.InsertOrReplaceAsync(mcd.Id, "mcd", mcd);
            logger.LogInformation("HTTP Status Code:"+result.HttpStatusCode);
            return mcd;
        }

    }

}