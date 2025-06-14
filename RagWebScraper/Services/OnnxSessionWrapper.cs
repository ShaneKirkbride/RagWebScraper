using Microsoft.ML.OnnxRuntime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RagWebScraper.Services;

public class OnnxSessionWrapper : IOnnxSession
{
    private readonly InferenceSession _session;

    public OnnxSessionWrapper(string modelPath)
    {
        _session = new InferenceSession(modelPath);
    }

    public IDisposableReadOnlyCollection<NamedOnnxValue> Run(IEnumerable<NamedOnnxValue> inputs)
    {
        var results = _session.Run(inputs.ToList());
        return new NamedOnnxValueCollection(results);
    }

    private sealed class NamedOnnxValueCollection : IDisposableReadOnlyCollection<NamedOnnxValue>
    {
        private readonly IDisposableReadOnlyCollection<DisposableNamedOnnxValue> _inner;

        public NamedOnnxValueCollection(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> inner)
        {
            _inner = inner;
        }

        public int Count => _inner.Count;

        public NamedOnnxValue this[int index] => _inner[index];

        public IEnumerator<NamedOnnxValue> GetEnumerator() => _inner.Cast<NamedOnnxValue>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose() => _inner.Dispose();
    }

    public void Dispose() => _session.Dispose();
}
