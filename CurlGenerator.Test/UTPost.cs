using FluentAssertions;

namespace CurlGenerator.Test;


using System.Text;

public class UTPost : IClassFixture<HttpClientGenerator>
{
    private readonly HttpClient _httpClient;

    public UTPost(HttpClientGenerator httpClientGenerator)
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
        // Arrange
        string url = "https://jsonplaceholder.typicode.com/posts";
        string jsonPayload = @"{""title"": ""New Post"", ""body"": ""This is the body of the new post"", ""userId"": 1}";
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        
        //Act
        var result = await _httpClient.PostAsync(url, content);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        
        //Assert
        outputCurl.Should().ContainEquivalentOf($"'{url}'");
        outputCurl.Should().ContainEquivalentOf(" -H 'Content-Type: application/json;");
        outputCurl.Should().ContainEquivalentOf(" -d '{\"title\": \"New Post\", \"body\": \"This is the body of the new post\", \"userId\": 1}'");
        outputCurl.Should().NotContainAny("GET", "PUT");

    }

    
    /*
     *     "The HTTP `POST` request method is meant to transfer data to a server 
        (and elicit a response). What data is returned depends on the implementation
        of the server.

        A `POST` request can pass parameters to the server using \"Query String 
        Parameters\", as well as the Request Body. For example, in the following request,

        > POST /hi/there?hand=wave
        >
        > <request-body>

        The parameter \"hand\" has the value \"wave\". The request body can be in multiple
        formats. These formats are defined by the MIME type of the request. The MIME 
        Type can be set using the ``Content-Type`` HTTP header. The most commonly used 
        MIME types are:

        * `multipart/form-data`
        * `application/x-www-form-urlencoded`
        * `application/json`

        This endpoint echoes the HTTP headers, request parameters, the contents of
        the request body and the complete URI requested.",

     */
    
    private const string PostFormDataWithFileCurl =
        @"curl -X POST 'https://postman-echo.com/post' -H 'Cookie: sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE' -F 'fdjks=""dsf""' -F '&^%=""helo""' -F '12=""\""23\""""' -F ''\''123'\''=""'\''\""23\\\""4\\\""\""'\''""'";
    [Theory]
    [InlineData(PostFormDataWithFileCurl)]
    public async Task PostFormDataWithFileAsync(string curl)
    {
        string uri = "https://postman-echo.com/post";
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Cookie", "sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE");
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("dsf"), "fdjks");
        content.Add(new StringContent("helo"), "&^%");
        content.Add(new StringContent("\"23\""), "12");
        content.Add(new StringContent("'\"23\\\"4\\\"\"'"), "'123'");
        request.Content = content;
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        
        outputCurl.Should().ContainEquivalentOf($"'{uri}'");
        outputCurl.Should().ContainEquivalentOf(@" -F 'fdjks=""dsf""'");
        outputCurl.Should().ContainEquivalentOf(@"-F '&^%=""helo""'");
        outputCurl.Should().ContainEquivalentOf(@"-F '12=""\""23\""""'");
        outputCurl.Should().ContainEquivalentOf(@" -F ''\''123'\''=""'\''\""23\\\""4\\\""\""'\''""'");
        outputCurl.Should().NotContainAny("GET", "PUT");
        
    }
}