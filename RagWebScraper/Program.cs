using OpenAI;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Charts;
using RagWebScraper.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Server;
using RagWebScraper.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RagWebScraper.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

// Load config and OpenAI key
var openAiKey = builder.Configuration["OpenAI:ApiKey"];
if (string.IsNullOrWhiteSpace(openAiKey))
    throw new InvalidOperationException("OpenAI API key not configured.");

var openAiClient = new OpenAIClient(openAiKey);
builder.Services.AddSingleton(openAiClient);

// Blazorise setup
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true; // ensures UI updates immediately on changes
    })
    .AddBootstrap5Providers();

builder.Services.AddBootstrap5Components();

// HttpClients for services that need them
builder.Services.AddHttpClient<IWebScraperService, WebScraperService>();
builder.Services.AddHttpClient<VectorStoreService>();
builder.Services.AddHttpClient<QdrantSetupService>();

// Register backend services
builder.Services.AddSingleton<KeywordSentimentSummaryService>();
builder.Services.AddSingleton<KeywordContextSentimentService>();
builder.Services.AddSingleton<SentimentAnalyzerService>();
builder.Services.AddSingleton<KeywordExtractorService>();
builder.Services.AddSingleton<TextChunker>();
builder.Services.AddSingleton<IEmbeddingService>(
    new EmbeddingService(openAiKey));
builder.Services.AddSingleton<PdfTextExtractorService>();

// Bind settings
builder.Services.Configure<NerSettings>(
    builder.Configuration.GetSection("NerSettings"));

// Manually create ONNXNerService with DI-resolved config
builder.Services.AddSingleton<INerService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();

    var modelPath = Path.Combine(AppContext.BaseDirectory, config["NerSettings:ModelPath"]);
    var vocabPath = Path.Combine(AppContext.BaseDirectory, config["NerSettings:VocabPath"]);
    var mergesPath = Path.Combine(AppContext.BaseDirectory, config["NerSettings:MergesPath"]);
    var dictPath = Path.Combine(AppContext.BaseDirectory, config["NerSettings:DictionaryPath"]);

    return new ONNXNerService(modelPath, vocabPath, mergesPath, dictPath);
});

builder.Services.AddScoped<IEntityGraphExtractor, SpacyEntityGraphExtractor>();
builder.Services.AddScoped<ICrossDocumentLinker, SemanticCrossLinker>();
builder.Services.AddScoped<IRagAnalyzerService, RagAnalyzerService>();

// Controllers and Blazor setup
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.Configure<CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});
builder.Services.AddSingleton<IPageAnalyzerService, PageAnalyzerService>();
builder.Services.AddSingleton<IChunkIngestorService, ChunkIngestorService>();
builder.Services.AddSingleton<AppStateService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
using (var scope = app.Services.CreateScope())
{
    var qdrant = scope.ServiceProvider.GetRequiredService<QdrantSetupService>();
    await qdrant.EnsureCollectionExistsAsync();
}

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();