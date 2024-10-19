using FluentAssertions;
using Xunit.Abstractions;

namespace CurlGenerator.Test;

public class UTPostXml : IClassFixture<HttpClientGenerator>
{
    private readonly HttpClient _httpClient;

    public UTPostXml(HttpClientGenerator httpClientGenerator, ITestOutputHelper testOutputHelper)
    {
        _httpClient = httpClientGenerator.HttpClient;
        _httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
    }

    private const string PostXmlCurl =
        @"curl 'https://postman-echo.com/post' -H 'Content-Type: text/xml' -d '<xml>
  Test Test
</xml>'";
    [Theory]
    [InlineData(PostXmlCurl)]
    public async Task PostXmlAsync(string curl)
    {
        string uri = "https://postman-echo.com/post";
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
       // request.Headers.Add("Cookie", "sails.sid=s%3AX7QRCwYjWljdBe9RHzb2GT8xchj6YtQd.l70DDhTERU%2Fm9%2Bfigzqoo4zdWrZMQYLMnnMuX5VyKMI");
        var content = new StringContent("<xml>\n  Test Test\n</xml>", null, "text/xml");
        request.Content = content;
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        
        outputCurl.Should().ContainEquivalentOf($"'{uri}'");
        outputCurl.Should().ContainEquivalentOf(" -H 'Content-Type: text/xml");
        outputCurl.Should().ContainEquivalentOf(@"-d '<xml>\n  Test Test\n</xml>'");
        outputCurl.Should().NotContainAny("GET", "PUT");
    }
}