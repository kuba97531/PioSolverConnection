using Client.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Client.Plugins
{
    /// <summary>
    /// Write a program that takes a directory as an input, an empty directory as an output and conversion mode.
    /// load a file and save it in target dir as a smaller save.
    /// Modes: to_no_rivers, to_no_turns.
    /// </summary>
    public class FileShrinker
    {
        private string OutputDirectory { get; set; }
        private string InputDirectory { get; set; }

        private TreeUtil.TreeSize SaveMode { get; set; }

        private SolverConnection _solver { get; set; }


        public FileShrinker(Dictionary<string, string> arguments)
        {
            _solver = new SolverConnection(arguments["-solver"]);
            InputDirectory = arguments["-savesdirectory"];
            OutputDirectory = arguments["-outputdirectory"];
            SaveMode = TreeUtil.ParseTreeSize(arguments["-size"]);
        }


        public void Run()
        {
            var files = CFRFileUtil.GetAllCFRFiles(InputDirectory);
            if (files.Length == 0)
            {
                Console.WriteLine("There are no files in input directory");
                return;
            }

            if (!Directory.Exists(OutputDirectory)) Directory.CreateDirectory(OutputDirectory);

            if ((Directory.GetFiles(OutputDirectory)).Length != 0)
            {
                Console.WriteLine("Target directory has to be empty");
                return;
            }

            foreach (var file in files)
            {
                TreeUtil.LoadTree(_solver, file);

                int desiredBoardLength = TreeUtil.TreeSizeToBoardLength(SaveMode);
                int loadedTreeBoardLength = TreeUtil.GetCalculatedBoardLengthInLoadedTree(_solver, file);

                if (desiredBoardLength == loadedTreeBoardLength)
                {
                    Console.WriteLine($"Tree: {file} already has desired size");
                    continue;
                }
                if (desiredBoardLength > loadedTreeBoardLength)
                {
                    Console.WriteLine($"Tree: {file} already has smaller size than precised");
                    continue;
                }

                string newFilePath = $"{OutputDirectory}\\{SaveMode}{Path.GetFileName(file)}";
                TreeUtil.DumpTree(_solver, newFilePath, SaveMode);
            }

        }

    }
}
