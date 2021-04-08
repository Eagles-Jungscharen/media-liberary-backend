using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using EaglesJungscharen.MediaLibrary.Services;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(EaglesJungscharen.MediaLibrary.Startup))]

namespace EaglesJungscharen.MediaLibrary
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient<MediaItemApi>().ConfigureHttpClient(config => new HttpClientHandler
                {
                    UseCookies=false
                });
            builder.Services.AddHttpClient<McdApi>().ConfigureHttpClient(config => new HttpClientHandler
                {
                    UseCookies=false
                });
            
            
            builder.Services.AddSingleton<JWTAuthService>((s) => {
                return new JWTAuthService(System.Environment.GetEnvironmentVariable("IDP_URL"));
            });

        }
    }
}