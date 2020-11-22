using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;
using Azure.Storage.Cryptography;
using Azure.Core;
using System.Data.Common; 

namespace EaglesJungscharen.MediaLibrary.Services {

    public class  BlobStorageService {
        private BlobContainerClient _blobContainerClient;
        private StorageSharedKeyCredential _storageSharedKeyCredential;
        private string _blobContainerName;
        public BlobStorageService(string blobContainerName) {
            _blobContainerName = blobContainerName;
             string connection = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");
             _blobContainerClient = new BlobContainerClient(connection, _blobContainerName);
             _blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);
             var conBuilder = new DbConnectionStringBuilder();
            conBuilder.ConnectionString = connection;
            _storageSharedKeyCredential = new StorageSharedKeyCredential(conBuilder["AccountName"] as string, conBuilder["AccountKey"] as string);

        }
        
        public string BuildUploadUrl(string mediaItemId, string mediaItemFileName) {
            string blobUrlPart = $"{mediaItemId}/{mediaItemFileName}";
            var blobSasBuilder = new BlobSasBuilder
            {
                StartsOn = DateTime.UtcNow - TimeSpan.FromMinutes(5),
                ExpiresOn = DateTime.UtcNow.AddHours(2),
                BlobContainerName = _blobContainerName,
                BlobName = blobUrlPart
            };

            //  Defines the type of permission.
            blobSasBuilder.SetPermissions(BlobSasPermissions.Write);
            Uri _uri = new Uri(_blobContainerClient.Uri,_blobContainerName +"/"+ blobUrlPart);
            BlobUriBuilder sasUri = new BlobUriBuilder(_uri)
            {
                Query = blobSasBuilder.ToSasQueryParameters(_storageSharedKeyCredential).ToString()
            };
            return sasUri.ToUri().ToString();
        }

    }
}