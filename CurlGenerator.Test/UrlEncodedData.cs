using Xunit.Abstractions;

namespace CurlGenerator.Test;

public class UrlEncodedData : IClassFixture<HttpClientGenerator>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClient;

    public UrlEncodedData(HttpClientGenerator httpClientGenerator, ITestOutputHelper testOutputHelper)
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
        _httpClient.DefaultRequestHeaders.Add(Settings.Expected, curl);
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://postman-echo.com/post/?hardik=\"me\"");
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
        Assert.Equal(outputCurl, curl);
    }
}