using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

class Program
{
    static void Main(string[] args)
    {

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: sed-replacement <sourceJsonPath> <secretsJsonPath>");
        }

        // Get the source and secrets JSON file paths from command line arguments
        string sourceJsonPath = args[0];
        string secretsJsonPath = args[1];

        string outputJsonPath = "output.json";

        if (!File.Exists(sourceJsonPath))
        {
            Console.WriteLine($"Error: {sourceJsonPath} not found.");
            return;
        }

        if (!File.Exists(secretsJsonPath))
        {
            Console.WriteLine($"Error: {secretsJsonPath} not found.");
            return;
        }

        // Read and parse JSON files
        string sourceContent = File.ReadAllText(sourceJsonPath);
        string secretsContent = File.ReadAllText(secretsJsonPath);

        JsonNode? sourceJson = JsonNode.Parse(sourceContent);
        JsonNode? secretsJson = JsonNode.Parse(secretsContent);

        if (sourceJson == null)
        {
            Console.WriteLine($"Error: Failed to parse {sourceJsonPath} as valid JSON.");
            return;
        }

        if (secretsJson == null)
        {
            Console.WriteLine($"Error: Failed to parse {secretsJsonPath} as valid JSON.");
            return;
        }

        // Merge secrets into source (secrets values override source values)
        MergeJsonNodes(sourceJson, secretsJson);

        // Write the merged result to output.json
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string outputContent = sourceJson.ToJsonString(options);
        File.WriteAllText(outputJsonPath, outputContent);

        Console.WriteLine($"Successfully merged {sourceJsonPath} and {secretsJsonPath} into {outputJsonPath}");

    }

    /// <summary>
    /// Merges two JsonNode objects, where the source overrides the target. Be careful, its recursive
    /// </summary>
    /// <param name="target">The target JsonNode to be modified.</param>
    /// <param name="source">The source JsonNode whose values will override the target.</
    static void MergeJsonNodes(JsonNode target, JsonNode source)
    {
        if (source is JsonObject sourceObj && target is JsonObject targetObj)
        {
            foreach (var property in sourceObj)
            {
                if (targetObj.ContainsKey(property.Key))
                {
                    // If both have the same key, recursively merge if both are objects
                    if (property.Value is JsonObject && targetObj[property.Key] is JsonObject targetValue)
                    {
                        MergeJsonNodes(targetValue, property.Value);
                    }
                    else
                    {
                        // Override the target value with source value
                        targetObj[property.Key] = property.Value?.DeepClone();
                    }
                }
                else
                {
                    // Add new property from source
                    targetObj[property.Key] = property.Value?.DeepClone();
                }
            }
        }
    }
}