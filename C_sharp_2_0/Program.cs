using Client.Plugins;
using Client.Util;
using System;
using System.IO;

namespace Client
{
    class Program
    {
        //Examples of working args:
        //args = new[] { "print_ranges", "-solver", @"d:\PioSOLVER\PioSOLVER2-edge.exe", "-tree", @"d:\PioSOLVER\saves\ForTesting\FullWithRake.cfr" };
        //args = new[] { "files_info", "-solver", @"d:\PioSOLVER\PioSOLVER2-edge.exe", "-directory", @"d:\PioSOLVER\saves\ForTesting\"};
        //args = new[] { "shrink_saves", "-solver", @"d:\PioSOLVER\PioSOLVER2-edge.exe", "-savesdirectory", @"d:\PioSOLVER\saves\ForTesting\", "-outputdirectory", @"d:\PioSOLVER\saves\ForTesting\NewFolder", "-size", "to_no_turns" };
        //args = new[] { "rebuild_file", "-solver", @"d:\PioSOLVER\PioSOLVER2-edge.exe", "-tree", @"d:\PioSOLVER\saves\ForTesting\small.cfr" };
        static void Main(string[] args)
        {
            if (args.Length < 3) return;
            var arguments = new ArgumentsParser(args);
            switch (arguments.MainArgument.ToLower())
            {
                case "print_ranges":
                    var rangePrinter = new RangesPrinter(arguments.Values);
                    rangePrinter.Run();
                    break;
                case "files_info":
                    var fileReporter = new FileReporter(arguments.Values);
                    fileReporter.Run();
                    break;
                case "shrink_saves":
                    var fileShrinker = new FileShrinker(arguments.Values);
                    fileShrinker.Run();
                    break;
                case "rebuild_file":
                    var fileRebuilder = new FileRebuilder(arguments.Values);
                    fileRebuilder.Run();
                    break;
                default:
                    Console.WriteLine("Unknown Command: " + arguments.MainArgument);
                    break;
            }
        }
    }
}
