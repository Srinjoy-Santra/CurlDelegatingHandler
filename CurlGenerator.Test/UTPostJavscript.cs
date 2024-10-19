using FluentAssertions;
using Xunit.Abstractions;

namespace CurlGenerator.Test;

public class UTPostJavascript : IClassFixture<HttpClientGenerator>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClient;

    public UTPostJavascript(HttpClientGenerator httpClientGenerator, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _httpClient = httpClientGenerator.HttpClient;
        _httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
    }

    private const string PostJsCurl =
        @"curl 'https://postman-echo.com/post' -H 'Content-Type: application/json' -H 'Cookie: sails.sid=s%3AX7QRCwYjWljdBe9RHzb2GT8xchj6YtQd.l70DDhTERU%2Fm9%2Bfigzqoo4zdWrZMQYLMnnMuX5VyKMI' -d '{
  ""json"": ""Test-Test""
}'";
    [Theory]
    [InlineData(PostJsCurl)]
    public async Task PostJsAsync(string curl)
    {
        string uri = "https://postman-echo.com/post";
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Cookie", "sails.sid=s%3Aa1CArYIYlK6bang1h54Bf4D4Z9GzaJ9z.AvDfVcvqx1%2BGGTPNDEWLxqkHVhtjZVzbsxbu08UdlGI");
        var content = new StringContent("var val = 6;\nconsole.log(val);", null, "application/javascript");
        request.Content = content;

        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        
        outputCurl.Should().ContainEquivalentOf($"'{uri}'");
        outputCurl.Should().ContainEquivalentOf(" -H 'Content-Type: application/javascript");
        outputCurl.Should().ContainEquivalentOf(@"-d 'var val = 6;\nconsole.log(val);'");
        outputCurl.Should().NotContainAny("GET", "PUT");
    }
}