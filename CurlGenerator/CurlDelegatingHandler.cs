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
        request.Headers.Remove(Settings.CanSend);
        request.Headers.Remove(Settings.Expected);
        
        CurlRequestMessage curlRequestMessage =  new CurlRequestMessage(request);
        string result = await curlRequestMessage.BuildAsync();

        HttpResponseMessage response;
        if (bool.TryParse(canSend.FirstOrDefault(), out bool send) && send)
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        else
        {
            response = new HttpResponseMessage(HttpStatusCode.Unused);
        }
        //response.Headers.Add(Settings.Match, isCurlMatched.ToString()); 
        response.Headers.Add(Settings.OutputCurl, result);
        
        return response;
    }
}