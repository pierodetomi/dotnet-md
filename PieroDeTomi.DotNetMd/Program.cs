using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd;

//const int expectedArgsCount = 1;

//if (args.Length < expectedArgsCount)
//{
//    Console.ForegroundColor = ConsoleColor.Red;
//    Console.WriteLine("Invalid arguments");
//    return;
//}

//var assemblyFilePath = args[0];
var assemblyFilePath = @"..\..\..\..\PieroDeTomi.DotNetMd.SampleLib\bin\Debug\net8.0\PieroDeTomi.DotNetMd.SampleLib.dll";
var outputPath = @"..\..\..\..\docs";

if (!Directory.Exists(outputPath))
    Directory.CreateDirectory(outputPath);

var logger = new ConsoleLogger();
var parser = new AssemblyXmlDocParser(logger);

parser.LoadAssemblyAndXmlDoc(assemblyFilePath);
var types = parser.GetTypes();

var docGenerator = new DocGenerator(logger);

types.ForEach(type =>
{
    var doc = docGenerator.GenerateDoc(type);

    var sanitizedTypeName = type.Name.ToLower()
        .Replace(" ", string.Empty)
        .Replace(".", "-")
        .Replace("<", "__")
        .Replace(",", "__")
        .Replace(">", "__");

    File.WriteAllText(Path.Combine(outputPath, $"{sanitizedTypeName}.md"), doc);
});

//var json = JsonConvert.SerializeObject(types);
//logger.LogDebug(message: json);

Console.ReadLine();