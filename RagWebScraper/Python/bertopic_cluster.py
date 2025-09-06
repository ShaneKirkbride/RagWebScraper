import sys, json
from bertopic import BERTopic
from sklearn.feature_extraction.text import TfidfVectorizer
from umap import UMAP
from hdbscan import HDBSCAN


def main():
    if len(sys.argv) < 2:
        print("Usage: bertopic_cluster.py <num_clusters>", file=sys.stderr)
        sys.exit(1)
    num_clusters = int(sys.argv[1])
    docs = json.load(sys.stdin)
    if not isinstance(docs, list):
        print("Input must be a JSON array of documents", file=sys.stderr)
        sys.exit(1)
    # Use a small embedding model for faster test execution
    vectorizer = TfidfVectorizer()
    embeddings = vectorizer.fit_transform(docs).toarray()

    n_neighbors = min(15, max(2, len(docs) - 1))
    umap_model = UMAP(n_neighbors=n_neighbors)
    hdbscan_model = HDBSCAN(min_cluster_size=2, min_samples=1)

    model = BERTopic(
        nr_topics=num_clusters,
        embedding_model=None,
        umap_model=umap_model,
        hdbscan_model=hdbscan_model,
        calculate_probabilities=False,
        verbose=False,
    )
    topics, _ = model.fit_transform(docs, embeddings)
    descriptors = []
    for topic in sorted(set(topics)):
        if topic == -1:
            continue
        words_scores = model.get_topic(topic) or []
        words = [w for w, _ in words_scores]
        descriptors.append({"cluster_id": int(topic), "top_words": words})
    result = {
        "assignments": [int(t) for t in topics],
        "descriptors": descriptors,
    }
    json.dump(result, sys.stdout)


if __name__ == "__main__":
    main()
