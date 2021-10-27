using Client.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Plugins
{
    /// <summary>
    /// Load a file and output root strategy
    /// </summary>
    public class RangesPrinter
    {
        private string TreePath { get; set; }

        private SolverConnection _solver { get; set; }


        public RangesPrinter(Dictionary<string, string> arguments)
        {
            _solver = new SolverConnection(arguments["-solver"]);
            TreePath = arguments["-tree"];
        }


        public void Run()
        {
            TreeUtil.LoadTree(_solver, TreePath);

            var rangeOOP = TreeUtil.ShowRootRange(_solver, "OOP");
            Console.WriteLine(rangeOOP);
            var rangeIP = TreeUtil.ShowRootRange(_solver, "IP");
            Console.WriteLine(rangeIP);

        }
    }
}
