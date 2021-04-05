using System;
using System.Collections.Generic;
using WebGate.Azure.CloudTableUtils.CloudTableExtension;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using EaglesJungscharen.MediaLibrary.Models;
using EaglesJungscharen.MediaLibrary.Utils;
using System.Linq;

namespace EaglesJungscharen.MediaLibrary.Services {

    public class MediaItemStorageService {
        private CloudTable _mediaItemTable;

        public MediaItemStorageService(CloudTable ct) {
            this._mediaItemTable = ct;
        }

        public async Task<MediaItem> GetItemById(string id) {
            return await _mediaItemTable.GetByIdAsync<MediaItem>(id, "mediaitem");
        }
        public async Task<List<MediaItem>> GetAllItemForCollection(string cid) {
            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>();
            query.Where(TableQuery.GenerateFilterCondition("MediaCollectionId",QueryComparisons.Equal,cid));
            return await _mediaItemTable.GetAllByQueryAsync<MediaItem>(query);
        }
        public async Task<List<MediaItem>> GetAllPublicItemForCollection(string cid) {
            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>();
            query.Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("MediaCollectionId",QueryComparisons.Equal,cid),TableOperators.And, TableQuery.GenerateFilterConditionForBool("Public", QueryComparisons.Equal, true)));
            return await _mediaItemTable.GetAllByQueryAsync<MediaItem>(query);
        }

        public void Validate(MediaItem item, MediaCollectionDefinition mcd) {

        }

        public async Task<MediaItem> Save(MediaItem item, FunctionRunContext frc) {
            if (item.Id == "@new") {
                item.Id = Guid.NewGuid().ToString();
                item.Entries.ForEach(entry=> entry.MediaItemId = item.Id);
            }
            await _mediaItemTable.InsertOrReplaceAsync(item.Id, "mediaitem",item);
            return item;
        }
    }
}