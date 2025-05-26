#pragma warning disable ASPIREHOSTINGPYTHON001
#pragma warning disable ASPIREPROXYENDPOINTS001
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var pdfUrlParam = builder.AddParameter("PdfUrl");
var modelNameParam = builder.AddParameter("ModelName");

var qdrant = builder.AddQdrant("qdrant-ollama")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("qdrant-ollama", isReadOnly:false)
    .WithEndpointProxySupport(proxyEnabled: false);

var ollama = builder.AddOllama("ollama-ollama")
    .WithDataVolume("ollama-ollama")
    .WithContainerRuntimeArgs("--gpus=all");

var model = ollama.AddHuggingFaceModel(
    "model", modelNameParam.Resource.Value
);

const string collectionName = "RAG_Memory";

var memoryBuilder = builder.AddPythonApp(
        "memory-builder", "../Ollama.Practice.MemoryBuilder", "main.py"
    )
    .WithEnvironment("PdfUrl", pdfUrlParam)
    .WithEnvironment("CollectionName", collectionName)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithReference(model)
    .WaitFor(model);

var crew = builder.AddPythonApp(
        "crew", "../Ollama.Practice.Crew", "main.py"
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
    crew.WithEnvironment("DEBUG", "True");
}

builder.Build().Run();
