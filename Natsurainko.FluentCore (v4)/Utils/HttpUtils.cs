using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;

namespace Nrk.FluentCore.Utils;

internal static class HttpUtils
{
    public static readonly HttpClient HttpClient = new();

    public static HttpResponseMessage HttpPost(string url, string content, string contentType = "application/json")
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        using var httpContent = new StringContent(content);

        httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        requestMessage.Content = httpContent;

        return HttpClient.Send(requestMessage);
    }

    public static HttpResponseMessage HttpGet(string url, Tuple<string, string> authorization = default, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        if (authorization != null) requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorization.Item1, authorization.Item2);

        var responseMessage = HttpClient.Send(requestMessage, httpCompletionOption, CancellationToken.None);

        if (responseMessage.StatusCode.Equals(HttpStatusCode.Found))
        {
            string redirectUrl = responseMessage.Headers.Location.AbsoluteUri;

            responseMessage.Dispose();
            GC.Collect();

            return HttpGet(redirectUrl, authorization, httpCompletionOption);
        }

        return responseMessage;
    }

    public static string ReadAsString(this HttpContent content)
    {
        using var stream = content.ReadAsStream();
        using var streamReader = new StreamReader(stream);

        return streamReader.ReadToEnd();
    }
}
