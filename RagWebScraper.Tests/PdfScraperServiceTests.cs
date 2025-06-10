using Xunit;
using System.Net;
using System.Net.Http;
using RagWebScraper.Services;

namespace RagWebScraper.Tests;

public class PdfScraperServiceTests
{
    private class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public HttpRequestMessage? LastRequest { get; private set; }
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_handler(request));
        }
    }

    [Fact]
    public async Task GetPdfLinksAsync_ReturnsAbsoluteLinks()
    {
        // Arrange
        var html = "<a href='file.pdf'>a</a><a href='/b.pdf'>b</a>";
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html)
        });
        var service = new PdfScraperService(new HttpClient(handler));

        // Act
        var links = (await service.GetPdfLinksAsync("http://example.com/page")).ToList();

        // Assert
        Assert.Contains("http://example.com/file.pdf", links);
        Assert.Contains("http://example.com/b.pdf", links);
        Assert.Equal(2, links.Count);
    }

    [Fact]
    public async Task DownloadPdfsAsync_WritesFiles()
    {
        // Arrange
        var bytes = new byte[] {1,2,3};
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        });
        var service = new PdfScraperService(new HttpClient(handler));
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        // Act
        await service.DownloadPdfsAsync(new[] {"http://example.com/test.pdf"}, dir);

        // Assert
        var file = Path.Combine(dir, "test.pdf");
        Assert.True(File.Exists(file));
        Assert.Equal(bytes, await File.ReadAllBytesAsync(file));

        Directory.Delete(dir, true);
    }
}
