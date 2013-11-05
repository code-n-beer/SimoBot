using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimoBot
{
    class ConfigLoader
    {
        public ConfigLoader(string filename = "config.txt")
        {
            StreamReader reader = new StreamReader(filename);
            string line = reader.ReadLine();

            Dictionary<string, Dictionary<string, string>> configs = new Dictionary<string, Dictionary<string, string>>();

            string channel = "";
            string server = "";
            while (line != null)
            {
                if (line.Contains("@"))
                {
                    channel = line.Split('@')[0];
                    server = line.Split('@')[1];
					
                    //aaarghh shit I'm too tired for this

                }

                line = reader.ReadLine();
            }
        }
    }
}
