using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimoBot.Features
{
    class CompoundWordFeature : IFeature
    {

        List<string> words;
        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["comp"] = Execute;
        }

        Dictionary<string, string> configs;
        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
            words = loadWords(configs);
        }

        public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
        {

            int num = 0;
			if (Message.Trim().Length > 0)
			{
				try
				{
					num = Convert.ToInt32(Message);
				}
                catch(Exception e)
				{
					Console.WriteLine(e.Message);
					Client.LocalUser.SendMessage(Channel, "!comp or !comp <integer>");
					return;
				}

				Client.LocalUser.SendMessage(Channel, getCompoundWord(num));
				return;
			}
            Client.LocalUser.SendMessage(Channel, getCompoundWord());
        }

        string[] forbiddenStrings = {
            "nen",
            "tus",
            "tys",
            "ton",
            "tön",
            "mus",
            "set",
            "ine"};
        private string getCompoundWord(int length = 3, int depth = 0)
        {

			if (depth >= 2000)
			{
				return "Couldn't find a match in 1000 tries";
			}
            int idx = new Random().Next(words.Count);

            string word = words[idx];

            List<string> fittingWords = new List<string>();

            for (int i = word.Length - length; i >= 0; i--)
            {
                string subString = word.Substring(i, length);

                if (forbiddenStrings.Contains(subString))
                    continue;

                for (int j = 0; j < words.Count; j++)
                {
                    if (words[j].StartsWith(subString))
                    {
                        fittingWords.Add(words[j]);
                    }
                }

                if (fittingWords.Count > 0)
                {
                    break;
                }
                length++;
            }

            if(fittingWords.Count == 0)
            {
                return getCompoundWord(length, ++depth);                
            }

            idx = new Random().Next(fittingWords.Count);
            string sndWord = fittingWords[idx];

			string result = word + sndWord.Substring(length, sndWord.Length - length);

			if (result == word)
				return getCompoundWord(length, ++depth);

            return result;
        }

        private List<string> loadWords(Dictionary<string, Dictionary<string, string>> configs)
        {
            string fileName = ConfigLoader.FindValueFromNestedDictionary(configs, "compoundwords");
            StreamReader reader = new StreamReader(fileName);

            List<string> words = new List<string>();

            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                words.Add(line); 
            }

            return words;
        }
    }
}
