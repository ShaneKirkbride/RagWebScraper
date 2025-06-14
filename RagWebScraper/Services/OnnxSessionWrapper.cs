using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        var list = inputs.ToList();
        var results = _session.Run(list);
        return new DisposableNamedOnnxValueCollection(results);
    }

    private sealed class DisposableNamedOnnxValueCollection : IDisposableReadOnlyCollection<NamedOnnxValue>
    {
        private readonly IDisposableReadOnlyCollection<DisposableNamedOnnxValue> _inner;

        public DisposableNamedOnnxValueCollection(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> inner)
        {
            _inner = inner;
        }

        public int Count => _inner.Count;

        public NamedOnnxValue this[int index] => _inner[index];

        public IEnumerator<NamedOnnxValue> GetEnumerator()
        {
            foreach (var value in _inner)
                yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose() => _inner.Dispose();
    }

    public void Dispose() => _session.Dispose();
}
