using Microsoft.Extensions.DependencyInjection;

namespace CurlGenerator.Test;


using System.Text;
using Microsoft.Extensions.Http;


public class UnitTest1 : IClassFixture<HttpClientGenerator>
{
    private readonly HttpClient _httpClient;

    public UnitTest1(HttpClientGenerator httpClientGenerator)
    {
        _httpClient = httpClientGenerator.HttpClient;
        _httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
    }

    private const string PlaceHolderPostCurl =
        "curl -X POST 'https://jsonplaceholder.typicode.com/posts' -H 'Content-Type: application/json; charset=utf-8' -d '{\"title\": \"New Post\", \"body\": \"This is the body of the new post\", \"userId\": 1}'";
    [Theory]
    [InlineData(PlaceHolderPostCurl)]
    public async Task Test1Async(string curl)
    {
        _httpClient.DefaultRequestHeaders.Add(Settings.Expected, curl);

        string url = "https://jsonplaceholder.typicode.com/posts";
        string jsonPayload = @"{""title"": ""New Post"", ""body"": ""This is the body of the new post"", ""userId"": 1}";
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var result = await _httpClient.PostAsync(url, content);
        string? match = result.Headers.GetValues(Settings.Match).FirstOrDefault();
        Assert.True(bool.TryParse(match, out bool isMatched) && isMatched);
    }

    private const string PostFormDataWithFileCurl =
        @"curl -L 'https://postman-echo.com/post' -H 'Cookie: sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE' -F 'fdjks=""dsf""' -F '&^%=""helo""' -F '12=""\""23\""""' -F ''\''123'\''=""'\''\""23\\\""4\\\""\""'\''""'";
    [Theory]
    [InlineData(PostFormDataWithFileCurl)]
    public async Task Test2Async(string curl)
    {
        _httpClient.DefaultRequestHeaders.Add(Settings.Expected, curl);
        
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://postman-echo.com/post");
        request.Headers.Add("Cookie", "sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE");
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("dsf"), "fdjks");
        content.Add(new StringContent("helo"), "&^%");
        content.Add(new StringContent("\"23\""), "12");
        content.Add(new StringContent("'\"23\\\"4\\\"\"'"), "'123'");
        request.Content = content;
        var result = await _httpClient.SendAsync(request);
        string? match = result.Headers.GetValues(Settings.Match).FirstOrDefault();
        Assert.True(bool.TryParse(match, out bool isMatched) && isMatched);
    }
}

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