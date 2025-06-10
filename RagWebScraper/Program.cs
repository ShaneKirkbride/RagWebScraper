using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using OpenAI;
using RagWebScraper.Models;
using RagWebScraper.Services;
using RagWebScraper.Shared;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Charts;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------
// Static & Request Limits
// ---------------------------------------------
builder.WebHost.UseStaticWebAssets();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_073_741_824; // 1 GB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1_073_741_824; // 1 GB
});

// ---------------------------------------------
// Blazorise
// ---------------------------------------------
builder.Services
    .AddBlazorise(options => { options.Immediate = true; })
    .AddBootstrap5Providers();
builder.Services.AddBootstrap5Components();

// ---------------------------------------------
// OpenAI Setup
// ---------------------------------------------
var openAiKey = builder.Configuration["OpenAI:ApiKey"];
if (string.IsNullOrWhiteSpace(openAiKey))
    throw new InvalidOperationException("OpenAI API key not configured.");
builder.Services.AddSingleton(new OpenAIClient(openAiKey));

// ---------------------------------------------
// Add Services & Dependency Injection
// ---------------------------------------------

// Infrastructure
builder.Services.AddSingleton<AppStateService>();
builder.Services.AddSingleton<PdfResultStore>();
builder.Services.AddSingleton<ITextExtractor, PdfTextExtractorService>();
builder.Services.AddSingleton<TextChunker>();
builder.Services.AddSingleton<IEmbeddingService>(new EmbeddingService(openAiKey));
builder.Services.AddSingleton<IChunkIngestorService, ChunkIngestorService>();
builder.Services.AddSingleton<IPdfProcessingQueue, PdfProcessingQueue>();
builder.Services.AddHostedService<PdfProcessingWorker>();

// Analysis / AI
builder.Services.AddSingleton<ISentimentAnalyzer, SentimentAnalyzerService>();
builder.Services.AddSingleton<IKeywordExtractor, KeywordExtractorService>();
builder.Services.AddSingleton<IKeywordContextSentimentService, KeywordContextSentimentService>();
builder.Services.AddSingleton<KeywordSentimentSummaryService>();
builder.Services.AddSingleton<IPageAnalyzerService, PageAnalyzerService>();

// Scoped / Page-bound
builder.Services.AddScoped<IAnalysisService, PdfAnalysisService>();
builder.Services.AddScoped<IRagAnalyzerService, RagAnalyzerService>();
builder.Services.AddScoped<IKnowledgeGraphService, KnowledgeGraphService>();
builder.Services.AddScoped<IEntityGraphExtractor, SpaceEntityGraphExtractor>();
builder.Services.AddScoped<ICrossDocumentLinker, SemanticCrossLinker>();

// ---------------------------------------------
// HttpClients
// ---------------------------------------------
builder.Services.AddHttpClient<IWebScraperService, WebScraperService>();
builder.Services.AddHttpClient<VectorStoreService>();
builder.Services.AddHttpClient<QdrantSetupService>();
builder.Services.AddHttpClient<IPdfScraperService, PdfScraperService>();

// ---------------------------------------------
// NER (ONNX Model)
builder.Services.Configure<NerSettings>(builder.Configuration.GetSection("NerSettings"));
builder.Services.AddSingleton<INerService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();

    var modelPath = Path.Combine(AppContext.BaseDirectory, config["NerSettings:ModelPath"]);
    var vocabPath = Path.Combine(AppContext.BaseDirectory, config["NerSettings:VocabPath"]);
    var mergesPath = Path.Combine(AppContext.BaseDirectory, config["NerSettings:MergesPath"]);
    var dictPath = Path.Combine(AppContext.BaseDirectory, config["NerSettings:DictionaryPath"]);

    return new ONNXNerService(modelPath, vocabPath, mergesPath, dictPath);
});

// ---------------------------------------------
// Blazor & Controllers
// ---------------------------------------------
builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.Configure<CircuitOptions>(options => { options.DetailedErrors = true; });

// ---------------------------------------------
// Build & Configure App
// ---------------------------------------------
var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// ---------------------------------------------
// Startup Tasks
// ---------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var qdrant = scope.ServiceProvider.GetRequiredService<QdrantSetupService>();
    await qdrant.EnsureCollectionExistsAsync();
}

app.Run();
