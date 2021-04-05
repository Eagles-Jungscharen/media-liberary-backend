
using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EaglesJungscharen.MediaLibrary.Services;
using EaglesJungscharen.MediaLibrary.Models;
using System.Net.Http;


namespace EaglesJungscharen.MediaLibrary.Utils
{
    public class FunctionRunContext {
        private HttpRequest _request;
        private bool _authenticated;
        public User User {set;get;}
        public ILogger Log {get;}
        public HttpClient Client {get;}
        public string Environment {get;}
        public FunctionRunContext(HttpRequest request, HttpClient client, ILogger log) {
            _request = request;
            Log = log;
            Environment = GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
        }

        public async Task CheckAuthentication(JWTAuthService service) {
            User = await service.IsAuthencticated(_request, Client, Log);
            _authenticated = true;
        }
        public async Task<T> GetPayLoad<T>() {
            string requestBody = await new StreamReader(_request.Body).ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(requestBody);
        }

        public bool IsAuthenticated() {
            return _authenticated;
        }

        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

    }    
}