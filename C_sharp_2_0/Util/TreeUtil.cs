using Client.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Client.Util
{
    public static class TreeUtil
    {
        public static void LoadTree(SolverConnection solver, string fileName)
        {
            var loadTree = solver.GetResponseFromSolver("load_tree " + fileName);
            Console.WriteLine(loadTree[0]);
        }

        public static void DumpTree(SolverConnection solver, string fileName, TreeSize mode)
        {
            Console.WriteLine($"Dumping tree ({mode}) to: {fileName}");

            var saveResult = solver.GetResponseFromSolver($"dump_tree {fileName} {mode}");
            Console.WriteLine(saveResult[0]);
        }

        public static string ShowRootRange(SolverConnection solver, string player)
        {
            if (player != "OOP" && player != "IP") throw new ArgumentException($"Player not recognized: {player}");

            Console.WriteLine($"{player} range:");
            var r = solver.GetResponseFromSolver($"show_range {player} r");
            return r[0];
        }

        public static string[] ShowMetadata(SolverConnection solver, string fileName)
        {
            return solver.GetResponseFromSolver("show_metadata " + fileName);
        }

        public enum TreeSize
        {
            full,
            no_rivers,
            no_turns
        }


        public static TreeSize GetTreeSize(SolverConnection solver, string[] metadata)
        {
            const int BoardOffset = 2;

            if (metadata.ToList().Exists(line => line.Contains("flag") && !line.Contains("INCOMPLETE_TREE")))
            {
                return TreeSize.full;
            }

            var currentNodeId = "r:0";
            while (true)
            {
                var nodeInfo = solver.GetResponseFromSolver("show_node " + currentNodeId);
                if (nodeInfo.ToList().Any(x => x.Contains("UNSOLVED SCHEMATIC")))
                {
                    var boardLength = nodeInfo[BoardOffset].Split(" ").Length;
                    if (boardLength == 4) return TreeSize.no_rivers;
                    else if (boardLength == 3) return TreeSize.no_turns;
                    else
                    {
                        throw new ArgumentException($"Impossible save: board length is {boardLength}");
                    }
                }

                var showChildren = solver.GetResponseFromSolver("show_children " + currentNodeId);
                if (showChildren.ToList().Exists(line => line.StartsWith("r:0") && line.EndsWith("c")))//as default try choosing checking
                {
                    currentNodeId = showChildren.FirstOrDefault(line => line.StartsWith("r:0") && line.EndsWith("c"));
                }
                else
                {
                    currentNodeId = showChildren.FirstOrDefault(line => line.StartsWith("r:0") && !line.EndsWith("f"));
                }
            }
        }

        public static bool HasICM(string[] metadata) => metadata.ToList().Exists(line => line.Contains("ICM.Enabled#True"));

        public static bool HasRake(string[] metadata) => metadata.ToList().Exists(line => line.Contains("Rake.Enabled#True"));

        public static string GetPot(string[] metadata)
        {
            if (metadata.Length > 3)
            {
                return metadata[3];
            }
            else
            {
                return "";
            }
        }

        public static string GetExploitability(string[] results) => results.FirstOrDefault(x => x.StartsWith("Exploitable", ignoreCase: true, culture: null));

        public static TreeSize ParseTreeSize(string mode)
        {
            switch (mode)
            {
                case "0":
                case "to_no_rivers":
                    return TreeSize.no_rivers;
                case "1":
                case "to_no_turns":
                    return TreeSize.no_turns;
                default:
                    throw new Exception("Couldn't parse tree size: only possible options are to_no_rivers and to_no_turns");
            }
        }

        public static int TreeSizeToBoardLength(TreeSize size)
        {
            switch (size)
            {
                case TreeSize.full:
                    return 5;
                case TreeSize.no_rivers:
                    return 4;
                case TreeSize.no_turns:
                    return 3;
                default:
                    throw new ArgumentException("Unknown tree size");
            }
        }

        public static int GetCalculatedBoardLengthInLoadedTree(SolverConnection solver, string filename)
        {
            var metadata = ShowMetadata(solver, filename);
            const int BoardOffset = 2;
            int result;

            if (metadata.ToList().Exists(line => line.Contains("flag") && !line.Contains("INCOMPLETE_TREE")))
            {
                result = TreeSizeToBoardLength(TreeSize.full);
                return result;
            }

            var currentNodeId = "r:0";
            while (true)
            {
                var nodeInfo = solver.GetResponseFromSolver("show_node " + currentNodeId);

                if (nodeInfo.ToList().Any(x => x.Contains("UNSOLVED SCHEMATIC")))
                {
                    result = nodeInfo[BoardOffset].Split(" ").Length;
                    return result;
                }

                var showChildren = solver.GetResponseFromSolver("show_children " + currentNodeId);
                if (showChildren.ToList().Exists(line => line.StartsWith("r:0") && line.EndsWith("c")))//as default try choosing checking
                {
                    currentNodeId = showChildren.FirstOrDefault(line => line.StartsWith("r:0") && line.EndsWith("c"));
                }
                else
                {
                    currentNodeId = showChildren.FirstOrDefault(line => line.StartsWith("r:0") && !line.EndsWith("f"));
                }
            }
        }

        public static void RebuildForgottenStreets(SolverConnection solver, string treePath)
        {
            var rebuildResponse = solver.GetResponseFromSolver("rebuild_forgotten_streets");
            foreach (var line in rebuildResponse)
            {
                Console.WriteLine(line);
            }
        }
    }
}

