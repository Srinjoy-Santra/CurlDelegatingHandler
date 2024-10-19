using FluentAssertions;

namespace CurlGenerator.Test;

public class UTPut: IClassFixture<HttpClientGenerator>
{
    private readonly HttpClient _httpClient;

    public UTPut(HttpClientGenerator httpClientGenerator)
    {
        _httpClient = httpClientGenerator.HttpClient;
        _httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
    }
    
    /*
     *  "The HTTP `PUT` request method is similar to HTTP `POST`. It too is meant to
     *  transfer data to a server (and elicit a response). What data is returned depends on the implementation
     *  of the server.
     *  A `PUT` request can pass parameters to the server using \"Query String \nParameters\", as well as the Request Body. For example, in the following
     *  raw HTTP request,
     * > PUT /hi/there?hand=wave
     * > <request-body>",
     */
    private const string PutRequestDataCurl =
        @"curl -X PUT 'https://postman-echo.com/put' -H 'Content-Type: text/plain' -H 'Cookie: sails.sid=s%3AX7QRCwYjWljdBe9RHzb2GT8xchj6YtQd.l70DDhTERU%2Fm9%2Bfigzqoo4zdWrZMQYLMnnMuX5VyKMI' -d 'Etiam mi lacus, cursus vitae felis et, blandit pellentesque neque. Vestibulum eget nisi a tortor commodo dignissim.
Quisque ipsum ligula, faucibus a felis a, commodo elementum nisl. Mauris vulputate sapien et tincidunt viverra. Donec vitae velit nec metus.'";
    [Theory]
    [InlineData(PutRequestDataCurl)]
    public async Task TestPutRequestAsync(string curl)
    {
        string uri = "https://postman-echo.com/put";
        var request = new HttpRequestMessage(HttpMethod.Put, uri);
        request.Headers.Add("Cookie", "sails.sid=s%3AX7QRCwYjWljdBe9RHzb2GT8xchj6YtQd.l70DDhTERU%2Fm9%2Bfigzqoo4zdWrZMQYLMnnMuX5VyKMI");
        var content = new StringContent("Etiam mi lacus, cursus vitae felis et, blandit pellentesque neque. Vestibulum eget nisi a tortor commodo dignissim.\nQuisque ipsum ligula, faucibus a felis a, commodo elementum nisl. Mauris vulputate sapien et tincidunt viverra. Donec vitae velit nec metus.", null, "text/plain");
        request.Content = content;
        
        var result = await _httpClient.SendAsync(request);
        string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
        
        outputCurl.Should().ContainEquivalentOf($"'{uri}'");
        outputCurl.Should().ContainEquivalentOf(" -H 'Content-Type: text/plain");
        outputCurl.Should().ContainEquivalentOf(" -d 'Etiam mi lacus, cursus vitae felis et, blandit pellentesque neque. Vestibulum eget nisi a tortor commodo dignissim.\\nQuisque ipsum ligula, faucibus a felis a, commodo elementum nisl. Mauris vulputate sapien et tincidunt viverra. Donec vitae velit nec metus.'");
        outputCurl.Should().NotContainAny("GET", "POST");

    }
}