using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis;

namespace SimoBot.Features
{
	class MarkovFeature : IFeature
	{
        RedisClient rclient;
        int mainDb;
        int fastOneWordKeyDb;

        public string getNewMarkov(string seed, int recursionLevel = 0)
        {
            //seed = biggenFirstLetter(seed);

            string finalString = "";
            string key;
            string[] splitkey = seed.Trim().Split(' ');


            //If we already have a key
            if (splitkey.Length == 2)
            {
                key = splitkey[0] + " " + splitkey[1];
            }
            //If we only have one word
                        else if (splitkey.Length == 1 && seed.Trim() != "")
                        {
                                key = getFirstWordPairFromOneWord(seed);
                        }
                        else
                        {
                                key = rclient.GetRandomKey();
                        }

            finalString += key;

            for (string word = getNextWord(key); word != ""; word = getNextWord(key))
            {
                finalString += " " + word;

                if(finalString.Length > 400) {break;}

                key = getTwoLastWords(finalString);
            }

            if (splitkey.Length == 2 && finalString == seed && recursionLevel < 5)
            {
                recursionLevel++;
                return getNewMarkov(seed, recursionLevel);
            }
            if (finalString == seed) return getNewMarkov("");
            /*
                        if (finalString.Length > 380)
                        {

                        }
            */

                        if (finalString == "")
                        {
                recursionLevel++;
                                getNewMarkov(seed, recursionLevel);
                        }

            return finalString;
        }

        private string biggenFirstLetter(string str)
        {
            if (char.IsUpper(str[0]))
            {
                return str;
            }
            else
            {
                str = str[0].ToString().ToUpper() + str;
                str = str.Remove(1, 1);
                return str;
            }
            //if(str[0]
        }

        private Dictionary<string, List<string>> prepareDictionary(string s)
        {
            var chains = new Dictionary<string, List<string>>();

            string[] words = s.Split(' ');

            if (words.Length < 3)
                return null;

            for (int i = 0; i < words.Length - 2; i++)
            {
                                words[i] = words[i].Trim();
                                words[i + 1] = words[i + 1].Trim();
                                words[i + 2] = words[i + 2].Trim();

                                if (words[i] == "" || words[i + 1] == "" || words[i + 2] == "")
                                        continue;

                string key = words[i] + " " + words[i + 1];

                if (chains.ContainsKey(key))
                {
                    chains[key].Add(words[i + 2]);
                }
                else
                {
                    //In this case there obviously are no records of following words for the two words,... Because there
                    //are no records of the two words themselves. So only one thingy is needed. Language barrier.
                    chains[key] = new List<string>();
                    chains[key].Add(words[i + 2]);
                }
            }

            return chains;
        }

        public void addNewLineToRedis(string s)
        {
            //s = strip(s);

                        s = s.Trim();

            if (s.Split(' ').Length < 3) return;
            if(s.Contains("simo")) return;

            Dictionary<string, List<string>> chains = prepareDictionary(s);
            if (chains == null) return;

            foreach (KeyValuePair<string, List<string>> kvp in chains)
            {
                rclient.AddRangeToSet(kvp.Key, kvp.Value);
            }
        }

        public void selectDb(int db)
        {
            rclient.Db = db;
            mainDb = db;
        }

        private List<string> getFromRedis(string key)
        {
            List<string> values = rclient.GetAllItemsFromSet(key).ToList();

            if (values.Count == 0) return null;

            return values;
        }
        
        private string getNextWord(string key)
        {
            List<string> foundWords = getFromRedis(key);

            if (foundWords == null) { return ""; }

            if (foundWords.Count == 0) return "";

                int randomIdx = new Random(DateTime.Now.Millisecond).Next(foundWords.Count);

                return foundWords[randomIdx];
        }

        private string mostCommonWord(List<string> words)
        {
            Dictionary<string, int> wordCounts = new Dictionary<string, int>();
            foreach (string s in words)
            {
                if (wordCounts.ContainsKey(s))
                {
                    wordCounts[s]++;
                }
                else
                {
                    wordCounts[s] = 1;
                }
            }

            List<string> candidates = new List<string>();

            int max = wordCounts.Values.Max();

            foreach (KeyValuePair<string, int> kvp in wordCounts)
            {
                if (kvp.Value == max)
                {
                    candidates.Add(kvp.Key);
                }
            }

            int randomIdx = new Random().Next(candidates.Count);
            return candidates[randomIdx];
        }
        
        private string getFirstWordPairFromOneWord(string seed)
        {
                        try
                        {
                                rclient.Db = fastOneWordKeyDb;

                                List<string> keys = getAllKeys();
                                List<string> legitKeys = new List<string>();

                                bool keyFound = false;
                                string usableSeed = "";

                                for (int i = 0; i < keys.Count; i++)
                                {
                                        if (keys[i].Contains(seed.Trim()))
                                        {
                                                legitKeys.Add(keys[i]);
                                                //usableSeed = keys[i];
                                                keyFound = true;
                                                //break;
                                        }
                                }
                                if (keyFound)
                                {
                                        int random = new Random().Next(0, legitKeys.Count);

                                        usableSeed = legitKeys[random];

                                        string key = rclient.GetRandomItemFromSet(usableSeed);
                                        rclient.Db = mainDb;
                                        if (key == "") return rclient.GetRandomKey();
                                        Console.WriteLine("Non-random key: " + key);
                                        return key;
                                }
                                else
                                {
                                        rclient.Db = mainDb;
                                        string key = rclient.GetRandomKey();
                                        Console.WriteLine("Random key: " + key);
                                        return key;
                                }
                        }
                        catch (Exception e)
                        {
                                Console.WriteLine("getFirstWordPairFromOneWord failed: " + e.Message);
                                return "";
                        }
        }

        private string getFirstWord(string s)
        {
            string[] words = s.Split(' ');

            if (words.Length < 1) return s;

            return words[0];

        }

        private string getSecondWord(string s)
        {
            string[] words = s.Split(' ');
            return words[1];
        }

        private List<string> getAllKeys()
        {
            return rclient.GetAllKeys();
        }

        private string getTwoLastWords(string s)
        {
            string[] words = s.Split(' ');
            string rString = words[words.Length - 2] + " " + words[words.Length - 1];
            return rString;
        }

        private string strip(string str)
        {
            char[] arr = str.ToCharArray();

            arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c)
                                                 || c == '-' || c == '_'
                                                 || c == ':' || c == '('
                                                 || c == ')' || c == '<'
                                                 || c == '>' || c == ':'
                                                 || c == ' ' //|| c == 'ä'
                //|| c == 'ö' || c == 'å'
                                                 )));

            return new string(arr);
        }

		public void RegisterFeature(EngineMessageHandlers features)
		{
			features.commands["SimoBot"] = Execute;
		}

		public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
		{
            string host = ConfigLoader.FindValueFromNestedDictionary(configs, "redishost");
            int port = Convert.ToInt32(ConfigLoader.FindValueFromNestedDictionary(configs, "redisport"));

            int MainDB = Convert.ToInt32(ConfigLoader.FindValueFromNestedDictionary(configs, "markovmain"));
            int FastOneWordDB = Convert.ToInt32(ConfigLoader.FindValueFromNestedDictionary(configs, "markovfast"));


            rclient = new RedisClient(host, port);
            rclient.Db = MainDB;
            mainDb = MainDB;
            fastOneWordKeyDb = FastOneWordDB;
		}

		public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
		{
			string[] messageAsArray = Message.Split(' ');
			try
			{
				if (messageAsArray.Length == 1)
					Client.LocalUser.SendMessage(Channel, getNewMarkov(messageAsArray[1]));
				if (messageAsArray.Length > 2)
					Client.LocalUser.SendMessage(Channel, getNewMarkov(messageAsArray[1] + " " + messageAsArray[2]));
				else
					Client.LocalUser.SendMessage(Channel, getNewMarkov(""));
			}
			catch (Exception e)
			{
					Client.LocalUser.SendMessage(Channel, e.Message);
			}
		}
	}
}
