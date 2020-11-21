using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EaglesJungscharen.MediaLibrary
{
    public static class GetUploadUrl
    {
        [FunctionName("GetUploadUrl")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        public static async Task SharedAccessSignatureAuthAsync()
        {
            // Create a service level SAS that only allows reading from service
            // level APIs
            AccountSasBuilder sas = new AccountSasBuilder
            {
                // Allow access to blobs
                Services = AccountSasServices.Blobs,

                // Allow access to the service level APIs
                ResourceTypes = AccountSasResourceTypes.Service,

                // Access expires in 1 hour!
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };
            // Allow read access
            sas.SetPermissions(AccountSasPermissions.Read);

            // Create a SharedKeyCredential that we can use to sign the SAS token
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(StorageAccountName, StorageAccountKey);

            // Build a SAS URI
            UriBuilder sasUri = new UriBuilder(StorageAccountBlobUri);
            sasUri.Query = sas.ToSasQueryParameters(credential).ToString();

            // Create a client that can authenticate with the SAS URI
            BlobServiceClient service = new BlobServiceClient(sasUri.Uri);

            // Make a service request to verify we've successfully authenticated
            await service.GetPropertiesAsync();

            // Try to create a new container (which is beyond our
            // delegated permission)
            RequestFailedException ex =
                Assert.ThrowsAsync<RequestFailedException>(
                    async () => await service.CreateBlobContainerAsync(Randomize("sample-container")));
            Assert.AreEqual(403, ex.Status);
        }
    }
}
