using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var client = new ResponsesClient(config["OPENAI_API_KEY"]);

// Message-History
var systemMessage = File.ReadAllText("systemprompt.md");

// Menu data for function calling
var menuJson = File.ReadAllText("menu.json");
var menuDocument = JsonDocument.Parse(menuJson);

// Define the GetMenu function tool
var getMenuTool = ResponseTool.CreateFunctionTool(
    "GetMenu",
    BinaryData.FromObjectAsJson(new
    {
        type = "object",
        properties = new
        {
            category = new
            {
                type = "string",
                description = "The menu category to retrieve: foods, drinks, extras, sauces, or all for the complete menu.",
                @enum = new[] { "foods", "drinks", "extras", "sauces", "all" }
            }
        },
        required = new[] { "category" }
    }),
    strictModeEnabled: true,
    functionDescription: "Returns menu items and prices from the Döner restaurant. Use this whenever a customer asks about the menu, available items, or prices."
);

string GetMenu(string category)
{
    if (category == "all")
    {
        return menuJson;
    }

    if (menuDocument.RootElement.TryGetProperty(category, out var categoryElement))
    {
        return categoryElement.GetRawText();
    }

    return """{"error": "Unknown category"}""";
}

Console.WriteLine("Willkommen bei DönerBrot!");

string? previousResponseId = null;
while (true)
{
    // Ask user for input
    Console.Write("Du: ");
    var userInput = Console.ReadLine()!;

    List<ResponseItem> inputItems = [ResponseItem.CreateUserMessageItem(userInput)];

    // Loop to handle tool calls — continues until the model produces a text response
    while (true)
    {
        var options = new CreateResponseOptions()
        {
            Model = "gpt-5.2",
            Instructions = systemMessage,
            StoredOutputEnabled = true,
            PreviousResponseId = previousResponseId,
            StreamingEnabled = true,
            Tools = { getMenuTool },
        };

        foreach (var item in inputItems)
        {
            options.InputItems.Add(item);
        }

        string? functionCallId = null;
        string? functionName = null;
        BinaryData? functionArgs = null;

        await foreach (var chunk in client.CreateResponseStreamingAsync(options))
        {
            switch (chunk)
            {
                case StreamingResponseCreatedUpdate created:
                    previousResponseId = created.Response.Id;
                    break;
                case StreamingResponseOutputTextDeltaUpdate textDelta:
                    Console.Write(textDelta.Delta);
                    break;
                case StreamingResponseOutputTextDoneUpdate:
                    Console.WriteLine("\n");
                    break;
                case StreamingResponseOutputItemDoneUpdate itemDone
                    when itemDone.Item is FunctionCallResponseItem funcCall:
                    functionCallId = funcCall.CallId;
                    functionName = funcCall.FunctionName;
                    functionArgs = funcCall.FunctionArguments;
                    break;
            }
        }

        // If a tool was called, execute it and loop back with the result
        if (functionCallId is not null && functionName == "GetMenu")
        {
            var parsedArgs = JsonDocument.Parse(functionArgs!);
            var category = parsedArgs.RootElement.GetProperty("category").GetString()!;
            var result = GetMenu(category);

            inputItems = [ResponseItem.CreateFunctionCallOutputItem(functionCallId, result)];
            continue;
        }

        break; // No tool call — done with this turn
    }
}