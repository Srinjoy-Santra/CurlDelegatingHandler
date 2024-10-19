using FluentAssertions;
using Xunit.Abstractions;

namespace CurlGenerator.Test;

public class UTPostUrlEncoded : IClassFixture<HttpClientGenerator>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClient;

    public UTPostUrlEncoded(HttpClientGenerator httpClientGenerator, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _httpClient = httpClientGenerator.HttpClient;
        _httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
    }

    private const string PostUrlEncodedDataCurl =
        @"curl -X POST 'https://postman-echo.com/post/?hardik=%22me%22' -H 'Cookie: sails.sid=s%3A0SHeDli8jrGSnQT0gfxgn7-PzSG_mryL.7i6ST7KW8PXSQ3tyV1qf7M6WEl8G0QN5pV0uEsMFqbo' -H 'Content-Type: application/x-www-form-urlencoded' -d '1=a' -d '2=b' -d '%22%2212%22%22=%2223%22' -d ''\''1%222%5C%22%223'\''='\''1%2223%224'\'''";
    [Theory]
    [InlineData(PostUrlEncodedDataCurl)]
    public async Task Test1Async(string curl)
    {
        string uri = "https://postman-echo.com/post/?hardik=\"me\"";
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Cookie", "sails.sid=s%3A0SHeDli8jrGSnQT0gfxgn7-PzSG_mryL.7i6ST7KW8PXSQ3tyV1qf7M6WEl8G0QN5pV0uEsMFqbo");
        var collection = new List<KeyValuePair<string, string>>();
        collection.Add(new("1", "a"));
        collection.Add(new("2", "b"));
        collection.Add(new("\"\"12\"\"", "\"23\""));
        collection.Add(new("'1\"2\\\"\"3'", "'1\"23\"4'"));
        var content = new FormUrlEncodedContent(collection);
        request.Content = content;
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        
        outputCurl.Should().ContainEquivalentOf("'https://postman-echo.com/post/?hardik=%22me%22'");
        outputCurl.Should().ContainEquivalentOf(" -H 'Content-Type: application/x-www-form-urlencoded'");
        outputCurl.Should().ContainEquivalentOf(" -d '1=a'");
        outputCurl.Should().ContainEquivalentOf(" -d '2=b'");
        outputCurl.Should().ContainEquivalentOf(" -d '%22%2212%22%22=%2223%22'");
        outputCurl.Should().ContainEquivalentOf(@" -d ''\''1%222%5C%22%223'\''='\''1%2223%224'\'''");
        outputCurl.Should().NotContainAny("GET", "PUT");
    }

    private const string PostResolveUrlCurl =
        @"curl 'https://postman-echo.com/post' -H 'Content-Type: application/x-www-form-urlencoded' -d 'Duis posuere augue vel cursus pharetra. In luctus a ex nec pretium. Praesent neque quam, tincidunt nec leo eget, rutrum vehicula magna.
Maecenas consequat elementum elit, id semper sem tristique et. Integer pulvinar enim quis consectetur interdum volutpat.'";
    [Theory]
    [InlineData(PostResolveUrlCurl)]
    public async Task PostResolveUrlAsync(string curl)
    {
        string uri = "https://postman-echo.com/post";
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        var content = new StringContent(@"Duis posuere augue vel cursus pharetra. In luctus a ex nec pretium. Praesent neque quam, tincidunt nec leo eget, rutrum vehicula magna.
Maecenas consequat elementum elit, id semper sem tristique et. Integer pulvinar enim quis consectetur interdum volutpat.", null, "application/x-www-form-urlencoded");
        request.Content = content;
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        
        outputCurl.Should().ContainEquivalentOf($"'{uri}'");
        outputCurl.Should().ContainEquivalentOf(" -H 'Content-Type: application/x-www-form-urlencoded");
        outputCurl.Should().ContainEquivalentOf(@"-d 'Duis posuere augue vel cursus pharetra. In luctus a ex nec pretium. Praesent neque quam, tincidunt nec leo eget, rutrum vehicula magna.\nMaecenas consequat elementum elit, id semper sem tristique et. Integer pulvinar enim quis consectetur interdum volutpat.'");
        outputCurl.Should().NotContainAny("GET", "PUT");
    }
}