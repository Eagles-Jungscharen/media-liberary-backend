using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using WebGate.Azure.CloudTableUtils.CloudTableExtension;

using EaglesJungscharen.MediaLibrary.Models;
using EaglesJungscharen.MediaLibrary.Utils;

namespace EaglesJungscharen.MediaLibrary.Services {

    public class PcdStoragService {

        private CloudTable _table {get;}

        public PcdStoragService(CloudTable ct) {
            _table = ct;
        }

        public async Task<List<PictureCollectionDefinition>> GetAllPictureCollectionDefinitons(FunctionRunContext frc) {
            return await _table.GetAllAsync<PictureCollectionDefinition>("pcd");
        }

        public async Task<PictureCollectionDefinition> GetPictureCollectionDefinitonById(string id, FunctionRunContext frc) {
            return await _table.GetByIdAsync<PictureCollectionDefinition>(id, "pcd");
        }

        public async Task<PictureCollectionDefinition> SavePictureCollectionDefintion(PictureCollectionDefinition pcd, FunctionRunContext frc) {
            if (pcd.Id == "@new" || string.IsNullOrEmpty(pcd.Id)) {
                pcd.Id = Guid.NewGuid().ToString();
            }
            TableResult result = await _table.InsertOrReplaceAsync(pcd.Id, "pcd", pcd);
            return pcd;
        }

    }

}