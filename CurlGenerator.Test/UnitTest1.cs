namespace CurlGenerator.Test;


using System.Text;

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
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        Assert.Equal(outputCurl, curl);
    }

    private const string PostFormDataWithFileCurl =
        @"curl -X POST 'https://postman-echo.com/post' -H 'Cookie: sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE' -F 'fdjks=""dsf""' -F '&^%=""helo""' -F '12=""\""23\""""' -F ''\''123'\''=""'\''\""23\\\""4\\\""\""'\''""'";
    [Theory]
    [InlineData(PostFormDataWithFileCurl)]
    public async Task Test2Async(string curl)
    {
        _httpClient.DefaultRequestHeaders.Add(Settings.Expected, curl);
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://postman-echo.com/post");
        request.Headers.Add("Cookie", "sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE");
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("dsf"), "fdjks");
        content.Add(new StringContent("helo"), "&^%");
        content.Add(new StringContent("\"23\""), "12");
        content.Add(new StringContent("'\"23\\\"4\\\"\"'"), "'123'");
        request.Content = content;
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        Assert.Equal(outputCurl, curl);
    }
}