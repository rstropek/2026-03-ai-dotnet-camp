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
        StreamingEnabled = true,
    };
    await foreach(var chunk in client.CreateResponseStreamingAsync(options))
    {
        switch (chunk)
        {
            case StreamingResponseCreatedUpdate contentPart:
                previousResponseId = contentPart.Response.Id;
                break;
            case StreamingResponseOutputTextDeltaUpdate textDelta:
                Console.Write(textDelta.Delta);
                break;
            case StreamingResponseOutputTextDoneUpdate textDone:
                Console.WriteLine("\n");
                break;
        }
    }
}