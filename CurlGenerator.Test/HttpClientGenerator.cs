using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace CurlGenerator.Test;

public class HttpClientGenerator
{
    public HttpClient HttpClient;

    public HttpClientGenerator()
    {
        var services = new ServiceCollection();
        services.AddTransient<CurlDelegatingHandler>();
        services.AddHttpClient();
        services.ConfigureAll<HttpClientFactoryOptions>(options => 
            options.HttpMessageHandlerBuilderActions.Add(builder => 
                builder.AdditionalHandlers.Add(builder.Services.GetRequiredService<CurlDelegatingHandler>())
            ));
        
        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetService<IHttpClientFactory>();
        HttpClient = factory.CreateClient();
    }
}