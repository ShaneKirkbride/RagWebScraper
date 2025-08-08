using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RagWebScraper.Pages;
using Xunit;

namespace RagWebScraper.Tests;

public class DocumentUploadPageTests
{
    private class TrackingStream : MemoryStream
    {
        public bool IsDisposed { get; private set; }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
        }
    }

    private class StubBrowserFile : IBrowserFile
    {
        private readonly TrackingStream _stream;
        public StubBrowserFile(string name, int size)
        {
            Name = name;
            _stream = new TrackingStream();
            _stream.SetLength(size);
        }

        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public string Name { get; }
        public long Size => _stream.Length;
        public string ContentType => "application/pdf";
        public bool Disposed => _stream.IsDisposed;

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            _stream.Position = 0;
            return _stream;
        }
    }

    private class StubHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }
        public StubHandler(HttpResponseMessage response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(_response);
        }
    }

    private class NavStub : NavigationManager
    {
        public NavStub(string baseUri) => Initialize(baseUri, baseUri);
        protected override void NavigateToCore(string uri, bool forceLoad) { }
    }

    private static void SetPrivateField(object obj, string name, object? value)
        => obj.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(obj, value);

    private static Task InvokePrivateMethod(object obj, string name)
    {
        var method = obj.GetType().GetMethod(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (Task)method.Invoke(obj, Array.Empty<object>())!;
    }

    [Fact]
    public async Task UploadFiles_DisposesStreams()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var page = new DocumentUpload();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        page.GetType().GetProperty("Http", flags)!.SetValue(page, new HttpClient(handler));
        page.GetType().GetProperty("Navigation", flags)!.SetValue(page, new NavStub("http://base/"));

        var files = new List<IBrowserFile>
        {
            new StubBrowserFile("a.pdf", 150_000),
            new StubBrowserFile("b.pdf", 160_000)
        };
        SetPrivateField(page, "selectedFiles", files);

        await InvokePrivateMethod(page, "UploadFiles");

        Assert.NotNull(handler.Request); // request was sent
        Assert.All(files, f => Assert.True(((StubBrowserFile)f).Disposed));
    }
}
