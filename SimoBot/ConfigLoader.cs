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
        /*
         * 
         * 
         *               THIS WHOLE CLASS IS IN A HUGE NEED OF A COMPLETE REWRITE.... THE WAY IT IS NOW IS JUST HORRIBLE.          
         * 
         * 
         * 
         */ 
        public static Dictionary<string, Dictionary<string, string>> LoadConfig(string filename = "config.txt")
        {
            StreamReader reader = new StreamReader(filename);
            string line = reader.ReadLine();

            Dictionary<string, Dictionary<string, string>> configs = new Dictionary<string, Dictionary<string, string>>();


            string key = "";
            string value = "";

            string channel = "";

            while (line != null && line != "end")
            {
                if (line == "")
                {
                    line = reader.ReadLine();
                    continue;
                }

                if (line.Contains("@"))
                {
                    channel = line.Split('@')[0];
                    configs[channel] = new Dictionary<string, string>();
                    line = reader.ReadLine();
                    continue;
                }


                int pos = line.IndexOf('=');
                key = line.Substring(0, pos);

                value = line.Substring(pos + 1);

                configs[channel][key] = value;

                line = reader.ReadLine();

            }
			reader.Close();

            return configs;
        }

        public static List<Server> OrganizeConfsPerServer(Dictionary<string, Dictionary<string, string>> confs)
		{
			List<Server> servers = new List<Server>();
			Channel channel;
			foreach (KeyValuePair<string, Dictionary<string, string>> kvp in confs)
			{
				channel = new Channel();
				channel.config = kvp.Value;
				channel.channel = kvp.Value["channel"];
				channel.server = kvp.Value["server"];
				AddChannel(ref servers, channel);
			}

			return servers;

		}

		private static void AddChannel(ref List<Server> servers, Channel channel)
		{
            foreach(Server server in servers)
            {
               if(channel.server == server.server)
				{
				    server.channels.Add(channel);
					return;
				}
			}
            
			var channels = new List<Channel>();
            channels.Add(channel);

			Server newServer = new Server(channel.server, channels);
			servers.Add(newServer);

		}

        public static string FindValueFromNestedDictionary(Dictionary<string, Dictionary<string, string>> dictionary, string key)
		{
            foreach(var channelkvp in dictionary)
			{
                foreach(var kvp in channelkvp.Value)
				{
                    if(kvp.Key == key)
                        return kvp.Value;
				}
			}

			return null;
		}

		public static string GetCommitMessage(Dictionary<string, Dictionary<string, string>> dictionary)
		{

            StreamReader reader = new StreamReader(FindValueFromNestedDictionary(dictionary, "commitmessage"));
			string message = reader.ReadLine().Trim();
			string author = reader.ReadLine();
			if (author == null)
			{
				string[] split = message.Split(' ');
				message = "";
				for (int i = 0; i < split.Length - 1; i++)
				{
					message += split[i] + " ";
				}
				message = message.Trim();
				author = split[split.Length - 1].Trim();
			}
			author = author.Trim();
			reader.Close();

            return "Deployed commit: \"" + message + "\" by " + author;
		}
    }
}
