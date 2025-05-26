import os
import io
import requests
from qdrant_client import QdrantClient
from PyPDF2 import PdfReader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_ollama import OllamaEmbeddings
from langchain_qdrant import QdrantVectorStore

pdf_url = os.getenv("PdfUrl")
qdrant_conn = os.getenv("ConnectionStrings__qdrant-ollama_http")
collection_name = os.getenv("CollectionName")
model_conn = os.getenv("ConnectionStrings__model")

qdrant_conn_parts = qdrant_conn.split(";")
qdrant_url = next(p.split("=", 1)[1] for p in qdrant_conn_parts if p.startswith("Endpoint="))
qdrant_key = next(p.split("=", 1)[1] for p in qdrant_conn_parts if p.startswith("Key=")) 

ollama_conn_parts = model_conn.split(";")
ollama_url = next(p.split("=", 1)[1] for p in ollama_conn_parts if p.startswith("Endpoint="))
ollama_model = next(p.split("=", 1)[1] for p in ollama_conn_parts if p.startswith("Model="))

print(f"Using PdfUrl: {pdf_url}")
print(f"Using CollectionName: {collection_name}")
print(f"Using qdrant_conn: {qdrant_conn}")
print(f"Using Qdrant URL: {qdrant_url}")
print(f"Using Qdrant Key: {qdrant_key}")
print(f"Using model_conn: {model_conn}")
print(f"Using Ollama Endpoint: {ollama_url}")
print(f"Using Ollama Model: {ollama_model}")

client = QdrantClient(url = qdrant_url, api_key = qdrant_key)
if client.collection_exists(collection_name):
    print("Collection already exists, ending execution.")
    exit(0)

print("Downloading pdf...")
resp = requests.get(pdf_url)
pdf_bytes = io.BytesIO(resp.content)

print("Processing pdf...")
reader = PdfReader(pdf_bytes)
text = "\n".join(page.extract_text() or "" for page in reader.pages)

print("Chunking pdf...")
splitter = RecursiveCharacterTextSplitter(chunk_size=1000, chunk_overlap=100)
chunks = splitter.split_text(text)

print("Generating Embeddings...")
embeddings = OllamaEmbeddings(
    model = ollama_model,
    base_url=ollama_url
)

print("Uploading chunks to Qdrant...")
db = QdrantVectorStore.from_texts(
    texts=chunks,
    embedding=embeddings,
    url=qdrant_url,
    api_key=qdrant_key,
    collection_name=collection_name
)
