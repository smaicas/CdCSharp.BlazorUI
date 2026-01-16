using CdCSharp.DocGen.Cli;

if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
    PrintHelp();
    return 0;
}

string command = args[0].ToLowerInvariant();
string[] commandArgs = args.Skip(1).ToArray();

return command switch
{
    "generate" => await GenerateCommand.RunAsync(commandArgs),
    _ => PrintUnknownCommand(command)
};

static void PrintHelp()
{
    Console.WriteLine("CdCSharp Documentation Generator");
    Console.WriteLine();
    Console.WriteLine("Usage: docgen <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  generate    Generate documentation for a .NET project");
    Console.WriteLine();
    Console.WriteLine("Run 'docgen generate --help' for command options.");
}

static int PrintUnknownCommand(string command)
{
    Console.WriteLine($"Unknown command: {command}");
    Console.WriteLine();
    PrintHelp();
    return 1;
}