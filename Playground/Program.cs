using System.Reflection;

var asm = typeof(OpenAI.Responses.ResponseTool).Assembly;

// Check all streaming types for function-related info
Console.WriteLine("=== All Streaming types with relevant properties ===");
foreach (var t in asm.GetExportedTypes()
    .Where(t => t.Namespace == "OpenAI.Responses" && t.Name.StartsWith("Streaming"))
    .OrderBy(t => t.Name))
{
    var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.Name.Contains("Function") || p.Name.Contains("CallId") || p.Name.Contains("Name"))
        .ToList();
    if (props.Count > 0)
    {
        Console.WriteLine($"\n  {t.Name}");
        foreach (var p in props)
            Console.WriteLine($"    {p.PropertyType.Name} {p.Name}");
    }
}

// Check StreamingResponseOutputItemAddedUpdate
Console.WriteLine("\n=== StreamingResponseOutputItemAddedUpdate ===");
var addedType = asm.GetExportedTypes().FirstOrDefault(t => t.Name == "StreamingResponseOutputItemAddedUpdate");
if (addedType != null)
{
    foreach (var p in addedType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        Console.WriteLine($"  {p.PropertyType.Name} {p.Name}");
}

// Check StreamingResponseOutputItemDoneUpdate
Console.WriteLine("\n=== StreamingResponseOutputItemDoneUpdate ===");
var doneType = asm.GetExportedTypes().FirstOrDefault(t => t.Name == "StreamingResponseOutputItemDoneUpdate");
if (doneType != null)
{
    foreach (var p in doneType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        Console.WriteLine($"  {p.PropertyType.Name} {p.Name}");
}
