namespace CurlGenerator.Test;

public class GetTests: IClassFixture<HttpClientGenerator>
{
    private readonly HttpClient _httpClient;

    public GetTests(HttpClientGenerator httpClientGenerator)
    {
        _httpClient = httpClientGenerator.HttpClient;
        _httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
    }

    private const string GetHeadersDataCurl =
        @"curl -X GET 'https://postman-echo.com/headers' -H 'my-sample-header: Lorem ipsum dolor sit amet' -H 'not-disabled-header: ENABLED' -H 'Cookie: sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE'";
    [Theory]
    [InlineData(GetHeadersDataCurl)]
    public async Task Test1Async(string curl)
    {
        _httpClient.DefaultRequestHeaders.Add(Settings.Expected, curl);
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://postman-echo.com/headers");
        request.Headers.Add("my-sample-header", "Lorem ipsum dolor sit amet");
        request.Headers.Add("not-disabled-header", "ENABLED");
        request.Headers.Add("Cookie", "sails.sid=s%3A3FymUYozeUuwzv6Znh8kdcuExLGNH2BC.jd0jX3p2HRPfY0PieRxdd6HJSqIeGarBi616trRmoyE");
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        Assert.Equal(outputCurl, curl);
    }
}
