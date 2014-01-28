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
            Client.LocalUser.SendMessage(Channel, getCompoundWord());
        }

        private string getCompoundWord()
        {
            int idx = new Random().Next(words.Count);

            string word = words[idx];

            List<string> fittingWords = new List<string>();

            int compoundLength = 0;

            int length = 3;
            for (int i = word.Length - length; i >= 0; i--)
            {
                string subString = word.Substring(i, length);

                //Lets try finding a substring that works, when we find one that works, we should stop finding words for longer 
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
                return getCompoundWord();                
            }

            idx = new Random().Next(fittingWords.Count);
            string sndWord = fittingWords[idx];

			string result = word + sndWord.Substring(length, sndWord.Length - length);


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
