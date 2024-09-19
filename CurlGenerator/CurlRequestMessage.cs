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
        builder.AddUrl(request?.RequestUri?.ToString());
        if (request.Content != null)
        {
            Dictionary<string, string> headers = new();
            foreach(var pair in request.Content.Headers)
            {
                if(pair.Value == null)
                    continue;
                var pairValues = pair.Value.ToList();
                if(pairValues.Count < 1)
                    continue;
                string v = pairValues[0];
                headers[pair.Key] = v;
            }
            builder.AddHeaders(headers);
            
            string reqBody = await request.Content.ReadAsStringAsync();
            string contentType = headers.GetValueOrDefault("Content-Type", "");
            string mode = contentType.Contains("application/json") ? "raw": "";
            builder.AddBody(mode, reqBody);
        }

        return builder.GetSnippet();
    }
    
    private async Task ConsoleWriteAsync(HttpRequestMessage request)
    {
        string curl = await BuildAsync();
        Console.WriteLine(curl);
    }
}