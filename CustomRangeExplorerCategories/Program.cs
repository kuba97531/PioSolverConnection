using System;
using RangeExplorerCategories.Util;

namespace RangeExplorerCategories
{
    public class IO
    {
        private static string _endString = null;

        public static void SetEndString(string s)
        {
            _endString = s;
        }

        public static void Print(params string[] s)
        {
            foreach (var line in s)
            {
                Console.WriteLine(line);
            }
            if (_endString != null)
            {
                Console.WriteLine(_endString);
            }
            Console.Out.Flush();
        }
    }

    class Program
    {
        static void Main()
        {
            string line;
            IO.Print("Custom Range Explorer Categories for PioSOLVER 3.0", "");

            while ((line = Console.ReadLine()) != null)
            {
                if (line == "exit") break;
                if (line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("set_end_string"))
                {
                    IO.SetEndString(line.Split()[1]);
                    IO.Print("set_end_string ok!");
                    continue;
                }
                if (line.StartsWith("is_ready"))
                {
                    IO.Print("is_ready ok!");
                    continue;
                }
                if (line.StartsWith("show_category_names"))
                {
                    var categoryNames = RangeExplorer.ShowCategoryNames();
                    IO.Print(categoryNames[0], categoryNames[1]);
                    continue;
                }
                if (line.StartsWith("show_categories"))
                {
                    if (line.Split().Length < 2)
                    {
                        IO.Print("ERROR: show_categories missing argument");
                        continue;
                    }
                    else
                    {
                        var board = line.Split()[1];
                        var categories = RangeExplorer.ShowCategories(board);
                        IO.Print(categories[0], categories[1]);
                    }
                    continue;
                }
                IO.Print("ERROR: unknown command " + line.Split()[0]);
            }
        }



    }
}
