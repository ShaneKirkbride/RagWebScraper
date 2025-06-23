using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RagWebScraper.Pages;
using Xunit;

namespace RagWebScraper.Tests;

public class JsonIngestPageTests
{
    private class StubBrowserFile : IBrowserFile
    {
        private readonly MemoryStream _stream;
        public long? LastMaxSize { get; private set; }

        public StubBrowserFile(string name, string content)
        {
            Name = name;
            _stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        }

        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public string Name { get; }
        public long Size => _stream.Length;
        public string ContentType => "application/json";

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            LastMaxSize = maxAllowedSize;
            _stream.Position = 0;
            return _stream;
        }
    }

    private class StubHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;
        public List<HttpRequestMessage> Requests { get; } = new();
        public List<string> Bodies { get; } = new();

        public StubHandler(IEnumerable<HttpResponseMessage> responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (request.Content != null)
            {
                var body = await request.Content.ReadAsStringAsync(cancellationToken);
                Bodies.Add(body);
            }
            else
            {
                Bodies.Add(string.Empty);
            }

            var resp = _responses.Count > 0 ? _responses.Dequeue() : new HttpResponseMessage(HttpStatusCode.OK);
            return resp;
        }
    }

    private class NavStub : NavigationManager
    {
        public NavStub(string baseUri) => Initialize(baseUri, baseUri);
        protected override void NavigateToCore(string uri, bool forceLoad) { }
    }

    private static object? GetPrivateField(object obj, string name)
        => obj.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(obj);

    private static void SetPrivateField(object obj, string name, object? value)
        => obj.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(obj, value);

    private static Task InvokePrivateMethod(object obj, string name)
    {
        var method = obj.GetType().GetMethod(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (Task)method.Invoke(obj, Array.Empty<object>())!;
    }

    [Fact]
    public async Task StartUpload_UploadsEachFileWithLimit()
    {
        var handler = new StubHandler(new[] { new HttpResponseMessage(HttpStatusCode.OK), new HttpResponseMessage(HttpStatusCode.OK) });
        var page = new JsonIngest();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        page.GetType().GetProperty("Http", flags)!.SetValue(page, new HttpClient(handler));
        page.GetType().GetProperty("Nav", flags)!.SetValue(page, new NavStub("http://base/"));

        var files = new List<IBrowserFile>
        {
            new StubBrowserFile("a.json", "{}"),
            new StubBrowserFile("b.json", "{}")
        };
        SetPrivateField(page, "_selectedFiles", files);

        await InvokePrivateMethod(page, "StartUpload");

        Assert.Equal(2, handler.Requests.Count);
        Assert.All(files.Cast<StubBrowserFile>(), f => Assert.Equal(1_073_741_824, f.LastMaxSize));
        var body1 = handler.Bodies[0];
        var body2 = handler.Bodies[1];
        Assert.Contains("a.json", body1);
        Assert.Contains("b.json", body2);
    }

    [Fact]
    public async Task StartUpload_StopsOnFailure()
    {
        var responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK),
            new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "bad" },
            new HttpResponseMessage(HttpStatusCode.OK)
        };
        var handler = new StubHandler(responses);
        var page = new JsonIngest();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        page.GetType().GetProperty("Http", flags)!.SetValue(page, new HttpClient(handler));
        page.GetType().GetProperty("Nav", flags)!.SetValue(page, new NavStub("http://base/"));

        var files = new List<IBrowserFile>
        {
            new StubBrowserFile("a.json", "{}"),
            new StubBrowserFile("b.json", "{}"),
            new StubBrowserFile("c.json", "{}")
        };
        SetPrivateField(page, "_selectedFiles", files);

        await InvokePrivateMethod(page, "StartUpload");

        Assert.Equal(2, handler.Requests.Count); // third file not uploaded
        var status = (string?)GetPrivateField(page, "status");
        Assert.Contains("b.json", status);
    }
}
