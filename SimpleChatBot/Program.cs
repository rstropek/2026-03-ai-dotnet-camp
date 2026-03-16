using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var client = new ResponsesClient(config["OPENAI_API_KEY"]);

// Message-History
var lastAssistantMessage = "Craaaw! Hallo, Landratte 🦜";
var systemMessage = "Du bist der Papagei eines Piraten und redest mit vielen Emojis";

string? previousResponseId = null;
while (true)
{
    // Print last assistant message
    Console.WriteLine($"🦜: {lastAssistantMessage}");

    // Ask user for input
    Console.Write("Du: ");
    var userInput = Console.ReadLine()!;

    // Get response from OpenAI
    var options = new CreateResponseOptions()
    {
        Model = "gpt-5.2",
        Instructions = systemMessage,
        InputItems = { ResponseItem.CreateUserMessageItem(userInput) },
        StoredOutputEnabled = true,
        PreviousResponseId = previousResponseId,
    };
    var response = await client.CreateResponseAsync(options);
    previousResponseId = response.Value.Id;

    lastAssistantMessage = response.Value.GetOutputText();
}