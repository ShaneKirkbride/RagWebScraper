using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class CourtListenerServiceTests
{
    private class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(_handler(request));
    }

    [Fact]
    public async Task GetOpinionsAsync_YieldsResultsAcrossPages()
    {
        var page1 = new { results = new[]{ new { id = 1, case_name = "a", plain_text = "A" } }, next = "http://host/next" };
        var page2 = new { results = new[]{ new { id = 2, case_name = "b", plain_text = "B" } }, next = (string?)null };

        var handler = new StubHandler(req =>
        {
            var obj = req.RequestUri!.AbsoluteUri.Contains("next") ? page2 : page1;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(obj) };
        });
        var service = new CourtListenerService(new HttpClient(handler));

        var opinions = new List<CourtOpinion>();
        await foreach (var op in service.GetOpinionsAsync("test"))
        {
            opinions.Add(op);
        }

        Assert.Equal(2, opinions.Count);
        Assert.Equal(1, opinions[0].Id);
        Assert.Equal(2, opinions[1].Id);
    }
}
