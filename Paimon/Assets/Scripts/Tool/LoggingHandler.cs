using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class LoggingHandler : DelegatingHandler
{
    public LoggingHandler()
        : this(new HttpClientHandler())
    { }

    public LoggingHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string msg;
        msg = "发送内容: " + request.ToString() + Environment.NewLine;
        msg += await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        Unity.Logging.Log.Debug(msg);
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        msg = "接收内容: " + response.ToString() + Environment.NewLine;
        msg += await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Unity.Logging.Log.Debug(msg);
        return response;
    }
}
