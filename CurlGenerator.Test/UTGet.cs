namespace CurlGenerator.Test;

public class UTGet: IClassFixture<HttpClientGenerator>
{
    private readonly HttpClient _httpClient;

    public UTGet(HttpClientGenerator httpClientGenerator)
    {
        _httpClient = httpClientGenerator.HttpClient;
        _httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
    }

    /*
     * "A `GET` request to this endpoint returns the list of all request headers as part of the response JSON.
     * In Postman, sending your own set of headers through the
     * [Headers tab](https://www.getpostman.com/docs/requests#headers?source=echo-collection-app-onboarding)
     * will reveal the headers as part of the response.",
     */
    private const string GetHeadersDataCurl =
        @"curl -X GET 'https://postman-echo.com/headers' -H 'my-sample-header: Lorem ipsum dolor sit amet' -H 'not-disabled-header: ENABLED' -H 'Cookie: sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE'";
    [Theory]
    [InlineData(GetHeadersDataCurl)]
    public async Task TestGetHeadersAsync(string curl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://postman-echo.com/headers");
        request.Headers.Add("my-sample-header", "Lorem ipsum dolor sit amet");
        request.Headers.Add("not-disabled-header", "ENABLED");
        request.Headers.Add("Cookie", "sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE");
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        Assert.Equal(outputCurl, curl);
    }
    
    /*
     * "The HTTP `GET` request method is meant to retrieve data from a server. The data
     *  is identified by a unique URI (Uniform Resource Identifier).
     *
     *  A `GET` request can pass parameters to the server using \"Query String \nParameters\". For example, in the following request,
     * > http://example.com/hi/there?hand=wave
     * The parameter \"hand\" has the value \"wave\".
     * This endpoint echoes the HTTP headers, request parameters and the complete\nURI requested.",
     */

    private const string GetQueryDataCurl =
        @"curl -X GET 'https://postman-echo.com/get?test=123&anotherone=232' -H 'Cookie: sails.sid=s%3AX7QRCwYjWljdBe9RHzb2GT8xchj6YtQd.l70DDhTERU%2Fm9%2Bfigzqoo4zdWrZMQYLMnnMuX5VyKMI'";
    
    [Theory]
    [InlineData(GetQueryDataCurl)]
    public async Task TestGetQueryAsync(string curl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://postman-echo.com/get?test=123&anotherone=232");
        request.Headers.Add("Cookie", "sails.sid=s%3AX7QRCwYjWljdBe9RHzb2GT8xchj6YtQd.l70DDhTERU%2Fm9%2Bfigzqoo4zdWrZMQYLMnnMuX5VyKMI");
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        Assert.Equal(outputCurl, curl);
    }
    

}

