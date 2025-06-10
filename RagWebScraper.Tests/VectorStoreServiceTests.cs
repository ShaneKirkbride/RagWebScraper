using Xunit;
using System.Net;
using System.Net.Http.Json;
using RagWebScraper.Services;

namespace RagWebScraper.Tests;

public class VectorStoreServiceTests
{
    private class CaptureHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public HttpRequestMessage? Request { get; private set; }
        public string? Content { get; private set; }
        public CaptureHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            if (request.Content != null)
                Content = await request.Content.ReadAsStringAsync();
            return _handler(request);
        }
    }

    [Fact]
    public async Task UpsertVectorAsync_SendsPayload()
    {
        // Arrange
        var handler = new CaptureHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new VectorStoreService(new HttpClient(handler));
        var data = new VectorData { Id = "1", Embedding = new List<float> { 0.1f }, Metadata = new { a = 1 } };

        // Act
        await service.UpsertVectorAsync(data);

        // Assert
        Assert.Equal(HttpMethod.Put, handler.Request?.Method);
        Assert.Contains("/collections/rag/points", handler.Request?.RequestUri?.ToString());
        Assert.Contains("\"id\":\"1\"", handler.Content);
    }

    [Fact]
    public async Task QueryAsync_ParsesResults()
    {
        // Arrange
        var json = "{ \"result\": [ { \"score\": 0.5, \"payload\": { \"doc\": \"x\" } } ] }";
        var handler = new CaptureHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        var service = new VectorStoreService(new HttpClient(handler));

        // Act
        var result = await service.QueryAsync(new List<float> { 0.1f });

        // Assert
        Assert.Single(result);
        Assert.Equal(0.5f, result[0].score);
        Assert.Equal("x", result[0].payload["doc"].ToString());
    }
}
