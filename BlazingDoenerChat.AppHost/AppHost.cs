var builder = DistributedApplication.CreateBuilder(args);

var apiKey = builder.AddParameter("openai-key", secret: true);
var openai = builder.AddOpenAI("openai")
                    .WithApiKey(apiKey);
var chat = openai.AddModel("chat", "gpt-5.2");

builder.AddProject<Projects.BlazingDoenerChat>("blazingdoenerchat")
       .WithExternalHttpEndpoints()
       .WithReference(chat);

builder.Build().Run();
