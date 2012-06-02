using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;

namespace MarkovChainTest
{
    class MarkovChainRedis
    {
        RedisClient rclient;
        public MarkovChainRedis(string host = "localhost", int port = 6380)
        {
            rclient = new RedisClient(host, port);
        }
        /// <summary>
        /// Only the first word from the seed is used
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        public string getNewMarkov(string seed, int wordCount = 100)
        {
            string finalString = "";
            string key = getFirstWordPair(seed);

            finalString += key;

            for (string word = getNextWord(key); word != ""; word = getNextWord(key))
            {
                finalString += " " + word;

                if(finalString.Length > 400) {break;}

                key = getTwoLastWords(finalString);
            }

            if (finalString.Split(' ').Length <= 3 && seed == "" && wordCount == 100)
            {
                finalString += " ja " + getNewMarkov("", 99);
            }

            if (finalString.Length > 400) finalString = finalString.Substring(0, 400);

            return finalString;
        }

        private Dictionary<string, List<string>> prepareDictionary(string s)
        {
            var chains = new Dictionary<string, List<string>>();

            string[] words = s.Split(' ');

            if (words.Length < 3)
                return null;

            for (int i = 0; i < words.Length - 2; i++)
            {

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
            s = strip(s);

            if (s.Split(' ').Length < 3) return;

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
        }

        private List<string> getFromRedis(string key)
        {
            List<string> values = rclient.GetAllItemsFromSet(key).ToList();
            //rclient.GetAllItemsFromSet
            if (values.Count == 0) return null;

            return values;
        }
        
        private string getNextWord(string key)
        {
            List<string> foundWords = getFromRedis(key);

            if (foundWords == null) { return ""; }

            if (foundWords.Count == 0) return "";


            /*
            List<string> candidates = new List<string>();
            foreach(KeyValuePair<string,int> kvp in foundWords)
            {
                if (kvp.Value == max)
                {
                    candidates.Add(kvp.Key);
                }
            }
            */
            int randomIdx = new Random(DateTime.Now.Millisecond).Next(foundWords.Count);

            return foundWords[randomIdx];
        }
        
        private string getFirstWordPair(string seed)
        {
            seed = getFirstWord(seed);

            List<string> keys = getAllKeys();
            List<string> keysWithSeed = new List<string>();

            foreach (string s in keys)
            {
                if (s.StartsWith(seed))
                    keysWithSeed.Add(s);
                //else if (s.EndsWith(seed))
                 //   keysWithSeed.Add(s);
            }

            //If the seeded word isn't found, return random key(=word pair)
            if (keysWithSeed.Count == 0)
            {
                return rclient.GetRandomKey();
            }

            int randomIdx = new Random(DateTime.Now.Millisecond).Next(keysWithSeed.Count);

            return keysWithSeed[randomIdx];
        }

        private string getFirstWord(string s)
        {
            string[] words = s.ToLower().Split(' ');

            if (words.Length < 1) return s.ToLower();

            return words[0];

        }

        private string getSecondWord(string s)
        {
            string[] words = s.ToLower().Split(' ');
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
    }
}
