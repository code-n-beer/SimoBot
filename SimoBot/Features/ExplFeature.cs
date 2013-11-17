using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimoBot
{
    class ExplFeature : IFeature
    {

		Dictionary<string, Dictionary<string, string>> explDictionaries;
        EngineMessageHandlers features;

		string configExplNameKey = "explfile";

		Dictionary<string, Dictionary<string, string>> configs;

        public void RegisterFeature(EngineMessageHandlers features)
        {
            this.features = features;
            features.commands["expl"] = Execute;
            features.commands["add"] =  ExecuteAdd;
			features.commands["remove"] = ExecuteRemove;
        }




        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
			this.configs = configs;
			explDictionaries = new Dictionary<string, Dictionary<string, string>>();
			foreach (var channel in configs)
			{
				explDictionaries[channel.Key] = loadDictionary(channel.Value[configExplNameKey]);
			}
        }

        public void Execute(IrcDotNet.IrcClient Client, string channel, IrcDotNet.IrcUser Sender, string message)
        {
			string expl = "";
			if (message.Trim() == "")
			{
				expl = explain(explDictionaries[channel]);
			}
			else
			{
				expl = explain(message, explDictionaries[channel]);
			}

			Client.LocalUser.SendMessage(channel, expl);
            
        }

		public void ExecuteRemove(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
		{
			string[] message = Message.Trim().Split(' ');
			if(message.Length == 1) {
		            remove(Message, explDictionaries[Channel], configs[Channel][configExplNameKey]);
			}
			else remove(message[0], message[1], explDictionaries[Channel],
			    configs[Channel][configExplNameKey]);
		}

        public void ExecuteAdd(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
        {
            string[] messageAsArray = Message.Trim().Split(' ');
			string name = messageAsArray[0];
			string expl = "";
			for (int i = 1;i < messageAsArray.Length; i++)
			{
				expl += messageAsArray[i] + " ";
			}
            expl = expl.Trim();

			Client.LocalUser.SendMessage(Channel, addExpl(name, expl, explDictionaries[Channel], configs[Channel][configExplNameKey]));
        }
		private string addExpl(string name, string expl, Dictionary<string, string> dictionary, string filename)
        {
            expl = expl.Trim();
            name = name.ToLower();

            if (name.Contains("|"))
            {
                return "'|' is not allowed in the expl name";
            }

            if (expl.Contains("|"))
            {
                if (dictionary.ContainsKey(name))
                {
                    return "Cannot add multiple expls to existing expl. Try removing first.";
                }

                dictionary.Add(name, expl);
                refreshTextFile(filename, dictionary);
                return explain(name, dictionary);
            }
            else
            {
                if (dictionary.ContainsKey(name))
                {
                    if ((dictionary[name] + " | " + expl).Length > 400)
                    {
                        if(expl.Length > 400)
                        {
                            return "Proposed new expl doesn't fit into one message character limit";
                        }
                        int suffix = getNumericSuffix(name);
                        if(suffix == -1) return addExpl(name + '2', expl, dictionary, filename);
                        return addExpl(name.TrimEnd("0123456789".ToCharArray()) + (suffix+1),
                        expl, dictionary, filename);
                    }
                    expl = dictionary[name] + " | " + expl.Trim();
                    dictionary[name] = expl;
                    refreshTextFile(filename, dictionary);
                    return explain(name, dictionary);
                }
                else
                {
                    dictionary.Add(name, expl);
                    refreshTextFile(filename, dictionary);
                    return explain(name, dictionary);
                }
            }
        }
        
        private int getNumericSuffix(string name)
        {
            int i = name.Length;
            while(i > 0 && Char.IsDigit(name[i-1]))
            {
            	i--;	
            }
            if(i == name.Length)
            {
		return -1;
            }
            return Int32.Parse(name.Substring(i, name.Length - i));
        }
        
        private string getLastSuffix(string name, Dictionary<string, string> dictionary)
        {
            string prefix = name.TrimEnd("0123456789".ToCharArray());
            int finalSuffix = getNumericSuffix(name);
            for(int i = finalSuffix;;i++) 
            {
            	if(dictionary.ContainsKey(prefix + i))
            	{
                    finalSuffix = i;
            	}
            	else if(i >= 2) break; //special case in mind
            }
            if(finalSuffix == -1) return name;
            return prefix + finalSuffix;
        }


        private void refreshTextFile(string filename, Dictionary<string, string> dictionary)
        {
            StreamWriter writer = new StreamWriter(filename, false);
            foreach (KeyValuePair<string, string> aids in dictionary)
            {
                writer.WriteLine(aids.Key + "|" + aids.Value);
            }
            writer.Flush();
            writer.Close();
        }

        public string explain(string word, Dictionary<string, string> dictionary)
        {
            word = word.ToLower();

            if (dictionary.ContainsKey(word))
            {
            	string ret = word + " : " + dictionary[word];
            	string final = getLastSuffix(word, dictionary);
            	if(!word.Equals(final))
            	{
                    ret += " | Continues in: " + final;
            	}
            	return ret;
            }
            return "No such expl";
        }

        public string explain(Dictionary<string, string> dictionary)
        {
            string key = dictionary.Keys.ElementAt(random(dictionary.Keys.Count));
            return key + " : " + dictionary[key];
        }
        
        private void remove(string what, string word, Dictionary<string, string> dictionary, string filename)
        {
            what = what.ToLower();
            word = word.Trim();
            if(dictionary.ContainsKey(what) && !String.IsNullOrEmpty(word) && dictionary[what].Contains(word))
            {
            	if(!dictionary[what].StartsWith(word))
            	{
                    dictionary[what] = dictionary[what].Replace(" | " + word, "");
            	}
            	else if(!dictionary[what].Equals(word))
            	{
                    dictionary[what] = dictionary[what].Replace(word + " | ", "");
            	}
            	else dictionary.Remove(what);
            }
            refreshTextFile(filename, dictionary);
        }
        
        private void remove(string what, Dictionary<string, string> dictionary, string filename)
        {
            what = what.ToLower();
            if(dictionary.ContainsKey(what))
                dictionary.Remove(what);

            refreshTextFile(filename, dictionary);
        }

		private Dictionary<string, string> loadDictionary(string filename)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			StreamReader reader;
			try
			{
				reader = new StreamReader(filename);
			}
			catch (FileNotFoundException)
			{
				Console.WriteLine("Expl file not found");
				return dictionary;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message + " - aids happened");
				return dictionary;
			}

			string line = reader.ReadLine();
			while (line != null)
			{
				string[] splitLine = line.Split('|');
				string explName = splitLine[0].Trim();  // word before '|' is expl name

				string expl = splitLine[1];
				if (splitLine.Length > 2)
				{
					for (int i = 2; i < splitLine.Length; i++)
					{
						expl += "|" + splitLine[i];
					}
				}

				dictionary.Add(explName, expl);

				line = reader.ReadLine();
			}
			reader.Close();

			return dictionary;
		}

        private int random(int max)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            return random.Next(0, max);
        }
    }
}
