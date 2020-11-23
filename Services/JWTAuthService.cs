using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System;




namespace EaglesJungscharen.MediaLibrary.Services {
    public class JWTAuthenticationService {
        private string _CTIDEPUrl;

        private IDictionary<string,JsonWebKey> _publicKeys =new Dictionary<string,JsonWebKey>();
        public JWTAuthenticationService(string ctIDPUrl) {
            _CTIDEPUrl = ctIDPUrl;
        }

        public async Task<bool> IsAuthencticated(HttpRequest request, HttpClient client, ILogger log) {
            string authentication = request.Headers["Authorization"];
            if (String.IsNullOrEmpty(authentication)) {
                return false;
            }
            if (!authentication.StartsWith("Bearer", StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }
            string jwtTokenPart = authentication.Substring(7);
            log.LogInformation("TOKEN: "+ jwtTokenPart);
            return await CheckJWTToken(jwtTokenPart, client, log);
        }

        public async Task<bool> CheckJWTToken(string token, HttpClient client, ILogger log) {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadToken(token);
            string kid = jwtToken.Header["kid"];
            if (String.IsNullOrEmpty(kid)) {
                log.LogInformation("No kid found in token");
                return false;
            }
            JsonWebKey jwk = await GetJsonWebKeyAsync(kid, client);
            if (jwk == null) {
                return false;
            }
            return false;
        }

        public async Task<JsonWebKey> GetJsonWebKeyAsync(string id, HttpClient client) {
            return null;
        }
    }
}