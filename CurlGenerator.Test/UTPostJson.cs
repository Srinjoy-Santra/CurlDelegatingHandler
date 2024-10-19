using FluentAssertions;
using Xunit.Abstractions;

namespace CurlGenerator.Test;

public class UTPostJson : IClassFixture<HttpClientGenerator>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClient;

    public UTPostJson(HttpClientGenerator httpClientGenerator, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _httpClient = httpClientGenerator.HttpClient;
        _httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
    }

    private const string PostJsonCurl =
        @"curl 'https://postman-echo.com/post' -H 'Content-Type: application/json' -H 'Cookie: sails.sid=s%3AX7QRCwYjWljdBe9RHzb2GT8xchj6YtQd.l70DDhTERU%2Fm9%2Bfigzqoo4zdWrZMQYLMnnMuX5VyKMI' -d '{
  ""json"": ""Test-Test""
}'";
    [Theory]
    [InlineData(PostJsonCurl)]
    public async Task PostJsonAsync(string curl)
    {
        string uri = "https://postman-echo.com/post";
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Cookie", "sails.sid=s%3AX7QRCwYjWljdBe9RHzb2GT8xchj6YtQd.l70DDhTERU%2Fm9%2Bfigzqoo4zdWrZMQYLMnnMuX5VyKMI");
        var content = new StringContent("{\n  \"json\": \"Test-Test\"\n}", null, "application/json");
        request.Content = content;
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        
        outputCurl.Should().ContainEquivalentOf($"'{uri}'");
        outputCurl.Should().ContainEquivalentOf(" -H 'Content-Type: application/json");
        outputCurl.Should().ContainEquivalentOf(@"-d '{\n  ""json"": ""Test-Test""\n}'");
        outputCurl.Should().NotContainAny("GET", "PUT");
    }
}