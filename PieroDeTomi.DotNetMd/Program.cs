using PieroDeTomi.DotNetMd;

//const int expectedArgsCount = 1;

//if (args.Length < expectedArgsCount)
//{
//    Console.ForegroundColor = ConsoleColor.Red;
//    Console.WriteLine("Invalid arguments");
//    return;
//}

//var assemblyFilePath = args[0];
var configFilePath = @"..\..\..\..\_assets\dotnetmd.json";

var logger = new ConsoleLogger();
var tool = new DotNetMdTool(configFilePath, logger);

tool.Run();

Console.WriteLine($"{Environment.NewLine}Generation completed. Press any key to exit ...");
Console.ReadKey();