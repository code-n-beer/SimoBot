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
        public static Dictionary<string, Dictionary<string, string>> LoadConfig(string filename = "config.txt")
        {
            StreamReader reader = new StreamReader(filename);
            string line = reader.ReadLine();

            Dictionary<string, Dictionary<string, string>> configs = new Dictionary<string, Dictionary<string, string>>();

            string channel = "";
            string server = "";

            string key = "";
            string value = "";
            while (line != null && line != "end")
            {
                if (line == "")
                    continue;

                if (line.Contains("@"))
                {
                    channel = line.Split('@')[0];
                    server = line.Split('@')[1];
                    configs[line] = new Dictionary<string, string>();
                    continue;
                }


                int pos = line.IndexOf('=');
                key = line.Substring(0, pos + 1);

                value = line.Substring(pos);

                configs[line][key] = value;

                line = reader.ReadLine();

            }

            return configs;
        }
    }
}
