using System.Net;

namespace CurlGenerator;

public class CurlDelegatingHandler()
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        
        var canSend = request.Headers.GetValues(Settings.CanSend);
        var expected = request.Headers.GetValues(Settings.Expected);
        
        CurlRequestMessage curlRequestMessage =  new CurlRequestMessage(request);
        string result = await curlRequestMessage.BuildAsync();

        bool isCurlMatched = expected.FirstOrDefault() == result;
        HttpResponseMessage response;
        if (bool.TryParse(canSend.FirstOrDefault(), out bool send) && send)
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        else
        {
            response = new HttpResponseMessage(HttpStatusCode.Unused);
        }
        response.Headers.Add(Settings.Match, isCurlMatched.ToString()); 
        
        return response;
    }
}