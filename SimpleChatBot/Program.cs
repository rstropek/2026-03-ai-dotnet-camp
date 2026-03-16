using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var client = new ResponsesClient(config["OPENAI_API_KEY"]);

// Message-History
var lastAssistantMessage = "Craaaw! Hallo, Landratte 🦜";
List<ResponseItem> messages =
[
    ResponseItem.CreateSystemMessageItem("Du bist der Papagei eines Piraten und redest mit vielen Emojis"),
    ResponseItem.CreateAssistantMessageItem(lastAssistantMessage),
];

while (true)
{
    // Print last assistant message
    Console.WriteLine($"🦜: {lastAssistantMessage}");

    // Ask user for input
    Console.Write("Du: ");
    var userInput = Console.ReadLine()!;

    // Add user message to message history
    messages.Add(ResponseItem.CreateUserMessageItem(userInput));

    // Get response from OpenAI
    var response = await client.CreateResponseAsync("gpt-5.2", messages);

    messages.Add(ResponseItem.CreateAssistantMessageItem(response.Value.GetOutputText()));
    lastAssistantMessage = response.Value.GetOutputText();
}