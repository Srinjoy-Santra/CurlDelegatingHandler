using FluentAssertions;

namespace CurlGenerator.Test;

public class UTPostHtml : IClassFixture<HttpClientGenerator>
{
    private readonly HttpClient _httpClient;

    public UTPostHtml(HttpClientGenerator httpClientGenerator)
    {
        _httpClient = httpClientGenerator.HttpClient;
        _httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
    }

    private const string PostXmlCurl =
        @"curl 'https://postman-echo.com/post' -H 'Content-Type: text/html' -d '<html>
  Test Test
</html>'";
    [Theory]
    [InlineData(PostXmlCurl)]
    public async Task PostXmlAsync(string curl)
    {
        string uri = "https://postman-echo.com/post";
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Cookie", "sails.sid=s%3AX7QRCwYjWljdBe9RHzb2GT8xchj6YtQd.l70DDhTERU%2Fm9%2Bfigzqoo4zdWrZMQYLMnnMuX5VyKMI");
        var content = new StringContent("<html>\n  Test Test\n</html>", null, "text/html");
        request.Content = content;
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        
        outputCurl.Should().ContainEquivalentOf($"'{uri}'");
        outputCurl.Should().ContainEquivalentOf(" -H 'Content-Type: text/html");
        outputCurl.Should().ContainEquivalentOf(@"-d '<html>\n  Test Test\n</html>'");
        outputCurl.Should().NotContainAny("GET", "PUT");
    }
}