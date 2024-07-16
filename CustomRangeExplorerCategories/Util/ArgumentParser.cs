using System.Collections.Generic;

namespace RangeExplorerCategories.Util
{
    /// <summary>
    /// Parsed command line arguments
    /// </summary>
    public class ArgumentsParser
    {
        public string MainArgument { get; private set; }

        public Dictionary<string, string> Values { get; private set; }

        public ArgumentsParser(string[] args)
        {
            string command = null;

            Values = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    command = arg;
                    Values[command] = null;
                }
                else if (command == null)
                {
                    MainArgument = arg;
                }
                else
                {
                    Values[command] = arg;
                }
            }
        }
    }
}
