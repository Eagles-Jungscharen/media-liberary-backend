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

    public class McdStoragService {

        private CloudTable _table {get;}

        public McdStoragService(CloudTable ct) {
            _table = ct;
        }

        public async Task<List<MediaCollectionDefinition>> GetAllMediaCollectionDefinitons(FunctionRunContext frc) {
            return await _table.GetAllAsync<MediaCollectionDefinition>("mcd");
        }

        public async Task<MediaCollectionDefinition> GetMediaCollectionDefinitonById(string id, FunctionRunContext frc) {
            return await _table.GetByIdAsync<MediaCollectionDefinition>(id, "mcd");
        }

        public async Task<MediaCollectionDefinition> SaveMediaCollectionDefintion(MediaCollectionDefinition mcd, FunctionRunContext frc) {
            if (mcd.Id == "@new" || string.IsNullOrEmpty(mcd.Id)) {
                mcd.Id = Guid.NewGuid().ToString();
            }
            TableResult result = await _table.InsertOrReplaceAsync(mcd.Id, "mcd", mcd);
            return mcd;
        }

    }

}