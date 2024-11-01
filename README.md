# CurlDelegatingHandler
`
This package generates curl by extending HttpClient of .NET with DelegatingHandler i.e. Custom Message Handler. 

- [Nuget release](https://www.nuget.org/packages/HttpClient.CurlDelegatingHandler)
- [Blog post](https://srinjoysantra.live/blog/curl-delegating-handler)

## How it works

Any requests made with `HttpClient` is sent through the client pipeline containing only one message handler named [`CurlDelegatingHandler`](./CurlGenerator/CurlDelegatingHandler.cs)

The curl is returned in the response header named `outputCurl`.

To prevent sending request to the network (with default `HttpClientHandler`), 
the header `CanSend: False` should be added to the request.

Currently supported HTTP verbs are as follows
- `GET`
- `POST`
- `PUT`
- `DELETE`

## How it was tested

Test cases were inspired from [Postman Code Generators](https://github.com/postmanlabs/postman-code-generators/blob/develop/codegens/curl/test/unit/fixtures/testcollection/collection.json).

### Sample console application

```cs
using System.Text;
using CurlGenerator;


string url = "https://jsonplaceholder.typicode.com/posts";
string jsonPayload = @"{""title"": ""New Post"", ""body"": ""This is the body of the new post"", ""userId"": 1}";
var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

var httpClient = new HttpClient(new CurlDelegatingHandler());
httpClient.DefaultRequestHeaders.Add(Settings.CanSend, "False");
var result = await httpClient.PostAsync(url, content);
string outputCurl = result.Headers.GetValues(Settings.OutputCurl).FirstOrDefault();
Console.WriteLine(outputCurl);
```

Output

```txt
curl -X POST 'https://jsonplaceholder.typicode.com/posts' -H 'Content-Type: application/json; charset=utf-8' -H 'Content-Length: 78' -d '{"title": "New Post", "body": "This is the body of the new post", "userId": 1}'
```

## Way Forward
- Builder methods can be decoupled further
- Support more [`Settings`](./CurlGenerator/Settings.cs) like Postman does




