# ObsidianRag

A RAG (Retrieval-Augmented Generation) system for searching and analyzing Obsidian notes using vector embeddings.

## Project Goal

ObsidianRag is a web service that automatically indexes Obsidian Markdown notes and provides semantic search based on vector embeddings. The system automatically scans your note directory, processes changes, and updates the vector database.

## Installation and Running

### Prerequisites

- Docker and Docker Compose
- Ollama installed and running with the `qwen3-embedding:8b` model

### Step 1: Install Ollama and Embedding Model

1. Install [Ollama](https://ollama.ai/)
2. Start Ollama
3. Pull the embedding model:

```bash
ollama pull qwen3-embedding:8b
```

### Step 2: Configure Your Obsidian Vault Path

Open `compose.yaml` and update the path to your Obsidian vault:

```yaml
volumes:
  - /path/to/your/Obsidian/vault:/app/data:ro
```

### Step 3: Start with Docker Compose

```bash
docker-compose up -d
```
**Note:** Database migrations are applied automatically when the application starts.

## TODO



## License

MIT
