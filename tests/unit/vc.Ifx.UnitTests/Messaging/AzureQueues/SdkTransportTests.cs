using Azure;
using Azure.Core.Pipeline;
using Azure.Storage.Queues;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace VisionaryCoder.Framework.Tests.Messaging.AzureQueues;

[TestClass]
public sealed class SdkTransportTests
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task RealSdkEncodesDecodesAndUsesRenewedReceiptsWithoutNetwork(bool encode)
    {
        using var handler = new QueueHttpHandler { Encode = encode };
        using var http = new HttpClient(handler);
        var options = QueueFixture.Options(encode: encode);
        var sdk = options.CreateClientOptions();
        sdk.Transport = new HttpClientTransport(http);
        using var provider = QueueFixture.Provider(new QueueClient(new Uri("https://account.queue.core.windows.net/work-items"), sdk), options);
        const string text = "hello <& \u00e9";
        provider.SendMessage(text);
        await provider.SendMessageAsync(text);
        string expected = encode ? Convert.ToBase64String(Encoding.UTF8.GetBytes(text)) : text;
        foreach (var request in handler.Requests)
        {
            Assert.AreEqual("POST", request.Method);
            Assert.IsTrue(request.Uri.Query.Contains("messagettl=-1", StringComparison.Ordinal));
            Assert.AreEqual(expected, XDocument.Parse(request.Body).Root!.Element("MessageText")!.Value);
        }
        var message = provider.ReceiveMessages(2).Single();
        Assert.AreEqual("response <& \u00e9", message.Body.ToString());
        Assert.AreEqual("received-pop", message.PopReceipt);
        Assert.AreEqual(4L, message.DequeueCount);
        Assert.AreEqual("response <& \u00e9", (await provider.ReceiveMessagesAsync()).Single().Body.ToString());
        Assert.AreEqual("response <& \u00e9", provider.PeekMessages().Single().Body.ToString());
        Assert.AreEqual("response <& \u00e9", (await provider.PeekMessagesAsync()).Single().Body.ToString());
        Assert.IsTrue(handler.Requests[2].Uri.Query.Contains("numofmessages=2", StringComparison.Ordinal));
        Assert.IsTrue(handler.Requests[2].Uri.Query.Contains("visibilitytimeout=17", StringComparison.Ordinal));
        Assert.IsTrue(handler.Requests[4].Uri.Query.Contains("peekonly=true", StringComparison.Ordinal));
        var receipt = provider.UpdateMessageWithReceipt(message.MessageId, message.PopReceipt, visibilityTimeout: TimeSpan.FromSeconds(23));
        Assert.AreEqual("renewed-pop", receipt.PopReceipt);
        Assert.AreEqual(DateTimeOffset.Parse("2026-09-09T12:00:00Z"), receipt.NextVisibleOn);
        Assert.AreEqual("", handler.Requests[^1].Body);
        Assert.IsTrue(handler.Requests[^1].Uri.Query.Contains("visibilitytimeout=23", StringComparison.Ordinal));
        receipt = await provider.UpdateMessageWithReceiptAsync(message.MessageId, receipt.PopReceipt, "");
        Assert.AreEqual("", XDocument.Parse(handler.Requests[^1].Body).Root!.Element("MessageText")!.Value);
        receipt = await provider.UpdateMessageWithReceiptAsync(message.MessageId, receipt.PopReceipt, text);
        Assert.AreEqual(expected, XDocument.Parse(handler.Requests[^1].Body).Root!.Element("MessageText")!.Value);
        provider.DeleteMessage(message.MessageId, receipt.PopReceipt);
        await provider.DeleteMessageAsync(message.MessageId, receipt.PopReceipt);
        Assert.IsTrue(handler.Requests[^1].Uri.Query.Contains("popreceipt=renewed-pop", StringComparison.Ordinal));
        Assert.IsTrue(provider.QueueExists());
        Assert.IsTrue(await provider.QueueExistsAsync());
        Assert.AreEqual(5, provider.GetMessageCount());
        Assert.AreEqual(5, await provider.GetMessageCountAsync());
        provider.ClearMessages();
        await provider.ClearMessagesAsync();
    }

    [TestMethod]
    public async Task RealSdkOwnsBoundedRetryAndMalformedBase64IsNotAcknowledged()
    {
        using var handler = new QueueHttpHandler { TransientFailures = 1 };
        using var http = new HttpClient(handler);
        var options = QueueFixture.Options();
        var sdk = options.CreateClientOptions();
        sdk.Retry.MaxRetries = 1;
        sdk.Retry.Delay = TimeSpan.Zero;
        sdk.Transport = new HttpClientTransport(http);
        using var provider = QueueFixture.Provider(new QueueClient(new Uri("https://account.queue.core.windows.net/work-items"), sdk), options);
        await provider.SendMessageAsync("once-requested");
        Assert.AreEqual(2, handler.Requests.Count);
        handler.Malformed = true;
        await Assert.ThrowsExactlyAsync<FormatException>(() => provider.ReceiveMessagesAsync());
        Assert.AreEqual(3, handler.Requests.Count);
        Assert.IsFalse(handler.Requests.Any(request => request.Method == "DELETE"));
        handler.TransientFailures = 10;
        var failure = await Assert.ThrowsExactlyAsync<RequestFailedException>(() => provider.SendMessageAsync("bounded"));
        Assert.AreEqual(503, failure.Status);
        Assert.AreEqual(5, handler.Requests.Count);
    }

    private sealed record HttpCall(string Method, Uri Uri, string Body);

    private sealed class QueueHttpHandler : HttpMessageHandler
    {
        public bool Encode { get; init; } = true;
        public bool Malformed { get; set; }
        public int TransientFailures { get; set; }
        public List<HttpCall> Requests { get; } = [];

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            => Respond(request, request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult() ?? "");
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Respond(request, request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken));

        private HttpResponseMessage Respond(HttpRequestMessage request, string body)
        {
            Requests.Add(new(request.Method.Method, request.RequestUri!, body));
            if (TransientFailures-- > 0)
                return new(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("<Error><Code>ServerBusy</Code><Message>Busy</Message></Error>") };
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("x-ms-request-id", "local-only");
            if (request.Method == HttpMethod.Post)
            {
                response.StatusCode = HttpStatusCode.Created;
                response.Content = new StringContent("<QueueMessagesList><QueueMessage><MessageId>id</MessageId><InsertionTime>Wed, 09 Sep 2026 10:00:00 GMT</InsertionTime><ExpirationTime>Wed, 16 Sep 2026 10:00:00 GMT</ExpirationTime><PopReceipt>sent-pop</PopReceipt><TimeNextVisible>Wed, 09 Sep 2026 10:00:00 GMT</TimeNextVisible></QueueMessage></QueueMessagesList>", Encoding.UTF8, "application/xml");
            }
            else if (request.RequestUri!.Query.Contains("comp=metadata", StringComparison.Ordinal))
                response.Headers.Add("x-ms-approximate-messages-count", "5");
            else if (request.Method == HttpMethod.Get)
            {
                const string value = "response <& \u00e9";
                string text = Malformed ? "%%%" : Encode ? Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) : value;
                var xml = new XElement("QueueMessagesList", new XElement("QueueMessage",
                    new XElement("MessageId", "id"), new XElement("InsertionTime", "Wed, 09 Sep 2026 10:00:00 GMT"),
                    new XElement("ExpirationTime", "Wed, 16 Sep 2026 10:00:00 GMT"), new XElement("PopReceipt", "received-pop"),
                    new XElement("TimeNextVisible", "Wed, 09 Sep 2026 10:01:00 GMT"), new XElement("DequeueCount", 4), new XElement("MessageText", text)));
                response.Content = new StringContent(xml.ToString(), Encoding.UTF8, "application/xml");
            }
            else if (request.Method == HttpMethod.Head)
                response.Headers.Add("x-ms-approximate-messages-count", "5");
            else
            {
                response.StatusCode = HttpStatusCode.NoContent;
                response.Headers.Add("x-ms-popreceipt", "renewed-pop");
                response.Headers.Add("x-ms-time-next-visible", "Wed, 09 Sep 2026 12:00:00 GMT");
            }
            return response;
        }
    }
}
