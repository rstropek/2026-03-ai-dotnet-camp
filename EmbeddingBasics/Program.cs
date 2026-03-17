using Microsoft.Extensions.Configuration;
using OpenAI.Embeddings;
using OpenAI.Responses;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var client = new EmbeddingClient("text-embedding-3-large", config["OPENAI_API_KEY"]);

// Ask user for three sentences to embed
Console.WriteLine("Enter three sentences to embed:");
var sentences = new string[3];
for (int i = 0; i < sentences.Length; i++)
{
    Console.Write($"Sentence {i + 1}: ");
    sentences[i] = Console.ReadLine() ?? "";
}

// Get embeddings for the sentences
var response = await client.GenerateEmbeddingsAsync(sentences);
var embeddings = response.Value.Select(e => e.ToFloats().ToArray()).ToArray();

var similarity12 = DotProduct(embeddings[0], embeddings[1]);
var similarity13 = DotProduct(embeddings[0], embeddings[2]);

if (similarity12 > similarity13)
{
    Console.WriteLine($"Sentence 1 is more similar to Sentence 2 ({similarity12}) than to Sentence 3 ({similarity13}).");
}
else if (similarity13 > similarity12)
{
    Console.WriteLine($"Sentence 1 is more similar to Sentence 3 ({similarity13}) than to Sentence 2 ({similarity12}).");
}
else
{
    Console.WriteLine($"Sentence 1 is equally similar to Sentence 2 ˚and Sentence 3 ({similarity13}).");
}

static float DotProduct(float[] a, float[] b)
{
    ArgumentOutOfRangeException.ThrowIfNotEqual(a.Length, b.Length);

    float result = 0;
    for (int i = 0; i < a.Length; i++)
    {
        result += a[i] * b[i];
    }

    return result;
}
