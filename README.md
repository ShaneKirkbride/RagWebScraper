# RAG Sentiment Analyzer (.NET + Blazor)

This project implements a sentiment-aware **Retrieval-Augmented Generation (RAG)** system in C# using **Blazor Server**, **ML.NET**, **Qdrant**, and **OpenAI**. It enables ingestion and semantic analysis of both web URLs and uploaded PDF documents.

## Features

* **Ingest from URLs or PDFs**
* **Local Sentiment Analysis** using ONNX model and ML.NET
* **Keyword Frequency & Context Sentiment Extraction**
* **Vector Embedding with OpenAI**
* **Vector storage and search in Qdrant**
* **Natural language query against stored content**
* **Automatic GPT-4 summary of keyword sentiment trends**
* **Interactive Blazor charts (Blazorise + Chart.js)**

---

## Getting Started

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Qdrant vector database](https://qdrant.tech/): run via Docker
* OpenAI API Key

### Configuration

Set your OpenAI API key and other configs in `appsettings.Development.json`:

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here"
  }
}
```

### Run Qdrant

```bash
docker run -p 6333:6333 qdrant/qdrant
```

### Run the App

```bash
dotnet run
```

Visit `https://localhost:7118`.

### Run Tests

```bash
dotnet test
```

---

## Architecture

* `SentimentAnalyzerService` – Tokenizes + runs ONNX model locally to score sentiment
* `KeywordExtractorService` – Extracts frequency of user-provided keywords
* `KeywordContextSentimentService` – Extracts keyword-local context windows and runs sentiment
* `TextChunker` – Splits text into windowed chunks for embedding
* `EmbeddingService` – Uses OpenAI Embeddings API (text-embedding-ada-002)
* `VectorStoreService` – Communicates with Qdrant to upsert + search chunks
* `KeywordSentimentSummaryService` – Aggregates keyword sentiment and calls GPT-4 for summary

---

## Documentation

All major services and controllers include XML documentation comments. IntelliSense in IDEs will display usage help automatically.

---

## Usage

### Ingest URLs

1. Navigate to `/analyze`
2. Paste URLs and provide comma-separated keywords
3. Analyze to view sentiment charts and keyword-level scores

### Upload PDFs

1. Navigate to `/upload-pdf`
2. Enter keywords
3. Select one or more PDFs
4. Analyze and review results & charts

### Query RAG

1. Navigate to `/query`
2. Ask natural language questions
3. See top-matching chunks from previously ingested content

### Cluster Documents

1. Navigate to `/cluster`
2. Upload `.txt` or `.pdf` files to use as documents
3. Optionally include previously analyzed URLs or PDFs
4. Choose the desired number of clusters and press **Cluster**

---

## Example Output

```
Keyword Sentiment Summary:
- "growth": moderately positive
- "risk": slightly negative

Summary: The documents are cautiously optimistic about growth and mildly concerned with risk exposure.
```

---

## Credits

* [Blazorise](https://blazorise.com/) for elegant charting and forms
* [PdfPig](https://github.com/UglyToad/PdfPig) for PDF text extraction
* [OpenAI .NET SDK (v2.1.0)](https://github.com/betalgo/openai) for API calls
* [Qdrant](https://qdrant.tech/) for fast vector search

---

## Roadmap Ideas

* In-browser annotation + highlighting
* Source citation in query results
* Scheduled crawl + analysis
* Full LLM completions based on retrieved chunks

---

## License

MIT

---

Built with love for AI prototyping and practical sentiment workflows. 🚀
