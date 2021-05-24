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

    public class PictureItemStorageService {
        private CloudTable _pictureItemTable;

        public PictureItemStorageService(CloudTable ct) {
            this._pictureItemTable = ct;
        }

        public async Task<PictureItem> GetItemById(string id) {
            return await _pictureItemTable.GetByIdAsync<PictureItem>(id, "pictureitem");
        }
        public async Task<List<PictureItem>> GetAllItems() {
            return await _pictureItemTable.GetAllAsync<PictureItem>("pictureitem");
        }
        public async Task<List<PictureItem>> GetAllItemForCollection(string cid) {
            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>();
            query.Where(TableQuery.GenerateFilterCondition("PictureCollectionId",QueryComparisons.Equal,cid));
            return await _pictureItemTable.GetAllByQueryAsync<PictureItem>(query);
        }
        public async Task<List<PictureItem>> GetAllPublicItemForCollection(string cid) {
            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>();
            query.Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PictureCollectionId",QueryComparisons.Equal,cid),TableOperators.And, TableQuery.GenerateFilterConditionForBool("Published", QueryComparisons.Equal, true)));
            return await _pictureItemTable.GetAllByQueryAsync<PictureItem>(query);
        }

        public void Validate(PictureItem item, PictureCollectionDefinition mcd) {

        }

        public async Task<PictureItem> Save(PictureItem item, FunctionRunContext frc) {
            if (item.Id == "@new") {
                item.Id = Guid.NewGuid().ToString();
                item.Created = DateTime.Now;
                item.ItemDate = DateTime.Now;
            }
            TableResult res =await _pictureItemTable.InsertOrReplaceAsync(item.Id, "pictureitem",item);
            frc.Log.LogInformation(""+res.HttpStatusCode);
            return item;
        }

        public async Task<PictureItem> UpdatePictureItem(UpdatePictureItemRequest updatePictureItemRequest, FunctionRunContext frc) {
            PictureItem mi = await GetItemById(updatePictureItemRequest.PictureItemId);
            mi.OrignalPictureURL = updatePictureItemRequest.OriginalUrl;
            await Save(mi,frc);
            return mi;
        }
        public async Task<PictureItem> DeletePictureItemContent(UpdatePictureItemRequest updatePicturetemRequest, FunctionRunContext frc) {
            PictureItem mi = await GetItemById(updatePicturetemRequest.PictureItemId);
            mi.OrignalPictureURL = null;
            await Save(mi,frc);
            return mi;
        }

        public async Task<PictureItem> Publish(string id, FunctionRunContext frc) {
            PictureItem mi = await GetItemById(id);
            mi.Published = true;
            await Save(mi,frc);
            return mi;
        }
        public async Task<PictureItem> Unpublish(string id, FunctionRunContext frc) {
            PictureItem mi = await GetItemById(id);
            mi.Published = false;
            await Save(mi,frc);
            return mi;
        }
    }
}