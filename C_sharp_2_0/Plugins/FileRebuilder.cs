using Client.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Client.Plugins
{

    /// <summary>
    /// - load a file, rebuild forgotten streets, save as full save
    /// </summary>
    public class FileRebuilder
    {
        private string TreePath { get; set; }

        private SolverConnection _solver { get; set; }


        public FileRebuilder(Dictionary<string, string> arguments)
        {
            // start solver executable
            _solver = new SolverConnection(arguments["-solver"]);
            TreePath = arguments["-tree"];
        }


        public void Run()
        {
            TreeUtil.LoadTree(_solver, TreePath);
            if (!File.Exists(TreePath))
            {
                Console.WriteLine("Specified tree file doesn't exist.");
                return;
            }
            if (TreeUtil.GetCalculatedBoardLengthInLoadedTree(_solver, TreePath) == 5)
            {
                Console.WriteLine($"Tree {TreePath} is already a full save!");
                return;
            }

            TreeUtil.RebuildForgottenStreets(_solver, TreePath);
            TreeUtil.DumpTree(_solver, TreePath, TreeUtil.TreeSize.full);
        }
    }
}
