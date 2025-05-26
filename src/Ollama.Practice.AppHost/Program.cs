#pragma warning disable ASPIREHOSTINGPYTHON001
#pragma warning disable ASPIREPROXYENDPOINTS001
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var pdfUrl = builder.AddParameter("PdfUrl");


var qdrant = builder.AddQdrant("qdrant-ollama")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("qdrant-ollama", isReadOnly:false)
    .WithEndpointProxySupport(proxyEnabled: false);

var ollama = builder.AddOllama("ollama-ollama")
    .WithDataVolume("ollama-ollama")
    .WithContainerRuntimeArgs("--gpus=all");

var model = ollama.AddHuggingFaceModel(
    "model", "bartowski/Llama-3.2-1B-Instruct-GGUF:IQ4_XS"
);

const string collectionName = "RAG_Memory";

var memoryBuilder = builder.AddPythonApp(
        "memory-builder", "../Ollama.Practice.MemoryBuilder", "main.py"
    )
    .WithEnvironment("PdfUrl", pdfUrl)
    .WithEnvironment("CollectionName", collectionName)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithReference(model)
    .WaitFor(model);

var client = builder.AddPythonApp(
        "client", "../Ollama.Practice.Client", "main.py"
    )
    .WithEnvironment("CollectionName", collectionName)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithReference(model)
    .WaitFor(model)
    .WaitForCompletion(memoryBuilder);

if (builder.ExecutionContext.IsRunMode && builder.Environment.IsDevelopment())
{
    memoryBuilder.WithEnvironment("DEBUG", "True");
}

builder.Build().Run();
