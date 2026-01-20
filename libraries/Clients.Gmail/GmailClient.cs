using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Integrations.Clients.Gmail;

public sealed class GmailClient
{
    private readonly HttpClient _httpClient;

    public GmailClient(HttpClient httpClient, string? functionKey = null)
    {
        _httpClient = httpClient;

        if (string.IsNullOrWhiteSpace(functionKey)) return;

        _httpClient.DefaultRequestHeaders.Remove(Defaults.FunctionKeyHeaderName);
        _httpClient.DefaultRequestHeaders.Add(Defaults.FunctionKeyHeaderName, functionKey);
    }

    public Task SendEmailAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendEmailInternalAsync(request, cancellationToken);
    }

    public Task SendEmailAsync(
        string? to,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        var request = new SendEmailRequest(to, subject, body, isHtml);
        return SendEmailInternalAsync(request, cancellationToken);
    }

    private async Task SendEmailInternalAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            Defaults.DefaultSendEmailPath,
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
