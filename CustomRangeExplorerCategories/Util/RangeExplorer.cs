using System.Collections.Generic;

namespace RangeExplorerCategories.Util
{
    class RangeExplorer
    {
        public static string[] CategoryNames = new string[] {
            "no_ace ace",
            "no_hit 1_hit 2_hits 3_hits",
        };
        public static string ComputeFirstLine(string board)
        {
            List<string> output = new List<string>();
            foreach (var hand in SolverHandOrder.HandOrder)
            {
                if (hand.Contains("A"))
                {
                    output.Add("1");
                }
                else
                {
                    output.Add("0");
                }
            }
            return string.Join(" ", output);
        }
        public static string ComputeSecondLine(string board)
        {
            var boardRanks = new int[char.MaxValue + 1];

            foreach (var ch in board.ToCharArray())
            {
                boardRanks[ch]++;
            }

            int countHits(string hand)
            {
                return boardRanks[hand[0]] + boardRanks[hand[2]];
            }

            List<string> output = new List<string>();
            foreach (var hand in SolverHandOrder.HandOrder)
            {
                var hits = countHits(hand);
                if (hits <= 3)
                {
                    output.Add(hits.ToString());
                }
                else
                {
                    output.Add("3");
                }
            }
            return string.Join(" ", output);
        }

        public static string[] ShowCategoryNames()
        {
            return CategoryNames;
        }
        public static string[] ShowCategories(string board)
        {
            return new string[] {
                ComputeFirstLine(board),
                ComputeSecondLine(board)
            };
        }
    }
}
