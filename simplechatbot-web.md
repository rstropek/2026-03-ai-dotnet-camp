Before you generate any code, research about the latest information on Blazor using the microsoft documentation MCP server (Learn Microsoft). Latest info about aspire can be found on context7 (MCP server).

* Task: Turn #SimpleChatBot from a console app into a web app
  * Continue to use Streaming (see existing console app)
* Technology: Blazor Server with .NET 10
  * `BlazingDoenerChat` as project/folder name
  * Add to #samples.slnx
* Design simple, as it is a demo, no overly smart design, but clean and functional
  * Input field + send button at the bottom, chat history above
* Use aspire 13 as orchestration layer during development
  * Add an aspire AppHost to the solution
  * Add aspire ServiceDefaults to the solution
  * Integrate `BlazingDoenerChat` with aspire

If you create new project, always use `dotnet` CLI. NEVER directly create .csproj files. Same is true for adding projects to the solution.

Once you are done generating code, make sure everything compiles successfully WITHOUT any warnings or errors.

For the OpenAI integration, use the built-in support for OpenAI of aspire. You can grab the OPENAI_API_KEY from the #SimpleChatBot project by running `dotnet user-secrets list` in the project folder.
