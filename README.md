# CurlDelegatingHandler

This package generates curl by extending HttpClient of .NET with DelegatingHandler i.e. Custom Message Handler. 

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


## Way Forward
- Release it as Nuget package
- Builder methods can be decoupled further
- Support more [`Settings`](./CurlGenerator/Settings.cs) like Postman does


