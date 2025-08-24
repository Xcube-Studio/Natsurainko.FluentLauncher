using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils;

internal class CustomHttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory) : IHttpMessageHandlerBuilderFilter
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return (builder) =>
        {
            // Run other configuration first, we want to decorate.
            next(builder);

            var outerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");
            builder.AdditionalHandlers.Insert(0, new CustomLoggingScopeHttpMessageHandler(outerLogger));
        };
    }
}

public partial class CustomLoggingScopeHttpMessageHandler(ILogger logger) : DelegatingHandler
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await base.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            _logger.ProcessingHttpRequestFailed(request.Method, request.RequestUri, response.StatusCode);

        return response;
    }
}

internal static partial class CustomLoggingScopeHttpMessageHandlerLoggers
{
    [LoggerMessage(LogLevel.Error, "Processing HTTP request {httpMethod} {uri} failed - {statusCode}")]
    public static partial void ProcessingHttpRequestFailed(this ILogger logger, HttpMethod httpMethod, Uri? uri, HttpStatusCode statusCode);
}