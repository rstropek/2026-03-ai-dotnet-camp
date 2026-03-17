using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var client = new ResponsesClient(config["OPENAI_API_KEY"]);

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
};

// Message-History
var systemMessage = File.ReadAllText("systemprompt.md");

// Menu data for function calling
var menuJson = File.ReadAllText("menu.json");
var menu = JsonSerializer.Deserialize<Menu>(menuJson, jsonOptions)!;

// Generate JSON schema from DTO for the function tool
var schemaNode = jsonOptions.GetJsonSchemaAsNode(typeof(GetMenuArgs), new JsonSchemaExporterOptions
{
    TreatNullObliviousAsNonNullable = true,
    TransformSchemaNode = (context, schema) =>
    {
        // Add description from DescriptionAttribute
        var attrProvider = context.PropertyInfo?.AttributeProvider ?? context.TypeInfo.Type;
        var desc = attrProvider?
            .GetCustomAttributes(typeof(DescriptionAttribute), true)
            .OfType<DescriptionAttribute>()
            .FirstOrDefault();
        if (desc is not null && schema is JsonObject jObj)
        {
            jObj.Insert(0, "description", desc.Description);
        }

        // Add additionalProperties: false for object types (required by OpenAI strict mode)
        if (schema is JsonObject obj && obj.ContainsKey("properties"))
        {
            obj["additionalProperties"] = false;
        }

        return schema;
    }
});

var getMenuTool = ResponseTool.CreateFunctionTool(
    "GetMenu",
    BinaryData.FromString(schemaNode.ToJsonString()),
    strictModeEnabled: true,
    functionDescription: "Returns menu items and prices from the Döner restaurant. Use this whenever a customer asks about the menu, available items, or prices."
);

string GetMenu(MenuCategory category)
{
    if (category == MenuCategory.All)
    {
        return menuJson;
    }

    return category switch
    {
        MenuCategory.Foods => JsonSerializer.Serialize(menu.Foods, jsonOptions),
        MenuCategory.Drinks => JsonSerializer.Serialize(menu.Drinks, jsonOptions),
        MenuCategory.Extras => JsonSerializer.Serialize(menu.Extras, jsonOptions),
        MenuCategory.Sauces => JsonSerializer.Serialize(menu.Sauces, jsonOptions),
        _ => """{"error": "Unknown category"}"""
    };
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

        var functionCalls = new List<FunctionCallResponseItem>();

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
                    functionCalls.Add(funcCall);
                    break;
            }
        }

        // If tool(s) were called, execute them all and loop back with the results
        if (functionCalls.Count > 0)
        {
            inputItems = [];
            foreach (var funcCall in functionCalls)
            {
                if (funcCall.FunctionName == "GetMenu")
                {
                    Console.WriteLine($"\n🔧 Function call: {funcCall.FunctionName}({funcCall.FunctionArguments})");
                    var menuArgs = JsonSerializer.Deserialize<GetMenuArgs>(funcCall.FunctionArguments.ToString(), jsonOptions)!;
                    var result = GetMenu(menuArgs.Category);
                    Console.WriteLine($"📋 Result: {result}\n");
                    inputItems.Add(ResponseItem.CreateFunctionCallOutputItem(funcCall.CallId, result));
                }
            }
            continue;
        }

        break; // No tool call — done with this turn
    }
}

// DTOs for JSON schema generation and deserialization

[JsonConverter(typeof(JsonStringEnumConverter<MenuCategory>))]
enum MenuCategory
{
    [Description("Main food items")]
    Foods,
    [Description("Available drinks")]
    Drinks,
    [Description("Extra toppings and add-ons")]
    Extras,
    [Description("Available sauces")]
    Sauces,
    [Description("The complete menu with all categories")]
    All
}

class GetMenuArgs
{
    [Description("The menu category to retrieve: foods, drinks, extras, sauces, or all for the complete menu. Always limit to the category that you really need. Only use `all` if the user explicitly asks for the entire menu or if you really need to show all categories to answer the user's question.")]
    public required MenuCategory Category { get; init; }
}

class MenuItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}

class Menu
{
    public string? Currency { get; init; }
    public List<MenuItem>? Foods { get; init; }
    public List<MenuItem>? Drinks { get; init; }
    public List<MenuItem>? Extras { get; init; }
    public List<string>? Sauces { get; init; }
}