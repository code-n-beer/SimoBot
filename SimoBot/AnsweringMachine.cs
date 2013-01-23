using System.IO;
using System.Collections.Generic;

namespace SimoBot
{
    public class AnsweringMachine
    {

        private List<string> lines;


        public AnsweringMachine(string confPath)
        {
            buildAnsweringMachine(confPath);


        }

        private void buildAnsweringMachine(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            string line = "asdf";
            lines = new List<string>();

            while (line != null)
            {
                line = reader.ReadLine();

                if (line == null) break;

                lines.Add(line);
            }
        }
    }
}