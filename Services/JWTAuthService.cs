using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using EaglesJungscharen.MediaLibrary.Models;




namespace EaglesJungscharen.MediaLibrary.Services {
    public class JWTAuthService {
        private string _CTIDEPUrl;
        private string _AdminScope;
        private string _ContributorScope;

        private IDictionary<string,SecurityKey> _publicKeys =new Dictionary<string,SecurityKey>();
        public JWTAuthService(string ctIDPUrl, string adminScope, string contributorScope) {
            _CTIDEPUrl = ctIDPUrl;
            _AdminScope = adminScope;
            _ContributorScope = contributorScope;
        }

        public async Task<User> IsAuthencticated(HttpRequest request, HttpClient client, ILogger log) {
            string authentication = request.Headers["Authorization"];
            if (String.IsNullOrEmpty(authentication)) {
                throw new AuthenticationException("No Authentication");
            }
            if (!authentication.StartsWith("Bearer", StringComparison.InvariantCultureIgnoreCase)) {
                throw new AuthenticationException("Wrong Tokentype, needs Bearer");
            }
            string jwtTokenPart = authentication.Substring(7);
            return await CheckJWTToken(jwtTokenPart, client, log);
        }

        public async Task<User> CheckJWTToken(string token, HttpClient client, ILogger log) {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            string kid = jwtToken.Header["kid"] as string;
            if (String.IsNullOrEmpty(kid)) {
                log.LogInformation("No kid found in token");
                throw new AuthenticationException("No kid in JWT found");
            }
            SecurityKey securityKey = await GetJsonWebKeyAsync(kid, client, log);
            if (securityKey == null) {
                throw new AuthenticationException("kid = "+ kid + " not found in IDP");
            }
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = "CT_IDP",
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateLifetime= true
            };

            handler.InboundClaimTypeMap.Clear();
            SecurityToken secToken;
            ClaimsPrincipal claimsPrincipal = handler.ValidateToken(token, parameters, out secToken);
            JwtSecurityToken jwt = (JwtSecurityToken)secToken;
            User user = new User() {
                FirstName = jwt.Claims.FirstOrDefault(claim=>claim.Type=="firstname")?.Value ?? string.Empty,
                LastName = jwt.Claims.FirstOrDefault(claim=>claim.Type=="lastname")?.Value ?? string.Empty,
                EMail = jwt.Claims.FirstOrDefault(claim=>claim.Type=="email")?.Value ?? string.Empty,
                Scopes = jwt.Claims.Where(claim=>claim.Type=="scopes").Select(fclaim=>fclaim.Value).ToList()
            };
            user.IsAdmin = user.Scopes.Contains(_AdminScope);
            user.IsContributor = user.Scopes.Contains(_ContributorScope);
            return user;
        }

        public async Task<SecurityKey> GetJsonWebKeyAsync(string id, HttpClient client, ILogger log) {
            if (_publicKeys.ContainsKey(id)) {
                return _publicKeys[id];
            }
            try {
                HttpResponseMessage response = await client.GetAsync(_CTIDEPUrl);
                response.EnsureSuccessStatusCode();
                JsonWebKeyContainer result = await response.Content.ReadAsAsync<JsonWebKeyContainer>();
                List<JsonWebKey> keys = result.Keys;
                JsonWebKey jwk = keys.Find(k=>k.Kid == id);
                if (jwk == null) {
                    return null;
                }
                SecurityKey securityKey = BuildSecurityKeyFromJsonWebKey(jwk);
                if (!_publicKeys.ContainsKey(id)) {
                    _publicKeys.Add(id, securityKey);
                }
                return securityKey;
            } catch(Exception e) {
                throw new AuthenticationException("Access to IDP Keys failed: "+ _CTIDEPUrl +" Reason: "+e.Message +" -> "+e.Source, e);
            }
        }
        SecurityKey BuildSecurityKeyFromJsonWebKey(JsonWebKey jwk) {
            byte[] exponent = Base64UrlEncoder.DecodeBytes(jwk.E);
            byte[] modulus = Base64UrlEncoder.DecodeBytes(jwk.N);
            var rsaParameters = new RSAParameters
            {
                Exponent = exponent,
                Modulus = modulus
            };

            var rsaSecurityKey = new RsaSecurityKey(rsaParameters)
            {
                KeyId = jwk.Kid
            };
            return rsaSecurityKey;
        }
    }
}