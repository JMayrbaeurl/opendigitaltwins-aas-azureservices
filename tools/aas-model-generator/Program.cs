// See https://aka.ms/new-console-template for more information
using Microsoft.DigitalWorkplace.DigitalTwins.Models.Generator;

Console.WriteLine("Hello, World!");

//...
var currentDir = Directory.GetCurrentDirectory();
var jsonDir = "C:\\Dev\\git\\JMayrbaeurl\\opendigitaltwins-aas\\Ontology";
var options = new ModelGeneratorOptions
{
    OutputDirectory = Path.Combine(currentDir, "..", "..", "..", "Generated"),
    Namespace = "Generator.Tests.Generated",
    JsonModelsDirectory = jsonDir
};

var generator = new ModelGenerator(options);
await generator.GenerateClassesAsync();