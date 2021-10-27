using Client.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Plugins
{

    /// <summary>
    ///  Take a directory as an input, list all files in directory,  write a report(one line per file):
    ///  filename, size(full, no_rivers or no_turns), [ICM / no ICM], [rake / no rake], pot, exploitability
    /// </summary>
    class FileReporter
    {
        public string Directory { get; }
        private SolverConnection _solver { get; }


        public FileReporter(Dictionary<string, string> arguments)
        {
            _solver = new SolverConnection(arguments["-solver"]);
            Directory = arguments["-directory"];
        }


        public void Run()
        {
            StringBuilder sb = new StringBuilder();

            var files = CFRFileUtil.GetAllCFRFiles(Directory);
            int fileCount = files.Length;
            int counter = 1;
            foreach (var file in files)
            {
                Console.WriteLine($"Processing File {counter}/{fileCount} ({file})");

                var metaData = TreeUtil.ShowMetadata(_solver, file);
                TreeUtil.LoadTree(_solver, file);
                string treeSize = TreeUtil.GetTreeSize(_solver, metaData).ToString();
                string ICM = TreeUtil.HasICM(metaData) ? "ICM" : "no ICM";
                string rake = TreeUtil.HasRake(metaData) ? "rake" : "no rake";
                string pot = TreeUtil.GetPot(metaData);
                var calcResults = _solver.GetResponseFromSolver("calc_results");
                string exploitability = TreeUtil.GetExploitability(calcResults);

                sb.Append("#" + file);
                sb.Append("#" + treeSize);
                sb.Append("#" + ICM);
                sb.Append("#" + rake);
                sb.Append("#" + pot);
                sb.Append("#" + exploitability);
                sb.AppendLine();
                counter++;
            }
            Console.WriteLine(sb.ToString());
        }
    }
}

