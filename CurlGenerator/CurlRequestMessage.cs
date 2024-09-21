using System.Collections;
using System.Net.Http.Headers;

namespace CurlGenerator;

public class CurlRequestMessage(HttpRequestMessage httpRequestMessage)
{
    public HttpRequestMessage _request = httpRequestMessage;

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            request.Content.Headers.ToList().ForEach(header => clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value));
        }

        request.Options.ToList().ForEach(option => clone.Options.TryAdd(option.Key, option.Value));
        request.Headers.ToList()
            .ForEach(header => clone.Headers.TryAddWithoutValidation(header.Key, header.Value));

        return clone;
    }

    public async Task<string> BuildAsync()
    {
        if (_request == null)
        {
            throw new Exception("httpRequestMessage is null");
        }
        HttpRequestMessage request = await CloneAsync(_request);
        
        IBuilder builder = new Builder(new Settings());
        
        builder.AddMethod(request.Method.ToString(), request.Content != null);
        
        
        Dictionary<string, string> headers = new();
        ExtractHeaders(headers, request.Headers);
        builder.AddUrl(request.RequestUri?.ToString());
        if (request.Content != null)
        {
            
            string reqBody = await request.Content.ReadAsStringAsync();
            string contentType = headers.GetValueOrDefault("Content-Type", "");
            ExtractHeaders(headers, request.Content.Headers);
            builder.AddHeaders(headers);
            Mode mode = Mode.None;
            
            if (contentType.Contains("application/json"))
            {
                mode = Mode.Raw;
            } else if (contentType.Contains("multipart/form-data"))
            {
                mode = Mode.FormData;
                headers.Remove("Content-Type");
            } else if (contentType.Contains("application/x-www-form-urlencoded"))
            {
                mode = Mode.FormUrlEncoded;
            }
            
            builder.AddBody(mode, reqBody);
        }
        else
        {
            builder.AddHeaders(headers);
        }

        void ExtractHeaders(Dictionary<string, string> headers, HttpHeaders requestHeaders)
        {
            foreach (var pair in requestHeaders)
            {
                if (pair.Value == null)
                    continue;
                var pairValues = pair.Value.ToList();
                if (pairValues.Count < 1)
                    continue;
                string v = pairValues[0];
                headers[pair.Key] = v;
            }
        }

        return builder.GetSnippet();
    }
    
    private async Task ConsoleWriteAsync(HttpRequestMessage request)
    {
        string curl = await BuildAsync();
        Console.WriteLine(curl);
    }
}