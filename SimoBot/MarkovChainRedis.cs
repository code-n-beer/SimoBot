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

        private Dictionary<string, Dictionary<string, int>> prepareDictionary(string s)
        {
            string[] words = s.Split(' ');

            Dictionary<string, Dictionary<string, int>> chains = new Dictionary<string,Dictionary<string,int>>();

            for (int i = 0; i < words.Length - 2; i++)
            {

                string key = words[i] + " " + words[i + 1];

                if (chains.ContainsKey(key))
                {
                    if (chains[key].ContainsKey(words[i + 2]))
                    {
                        chains[key][words[i + 2]] += 1;
                    }
                    else
                    {
                        chains[key][words[i + 2]] = 1;
                    }
                }
                else
                {
                    //In this case there obviously are no records of following words for the two words,... Because there
                    //are no records of the two words themselves. So only one thingy is needed. Language barrier.
                    chains[key] = new Dictionary<string, int>();
                    chains[key].Add(words[i + 2], 1);
                }
            }

            return chains;
        }

        public void addNewLineToRedis(string s)
        {

            if (s.Split(' ').Length < 3) return;

            Dictionary<string, Dictionary<string, int>> chains = prepareDictionary(s);

            foreach (KeyValuePair<string, Dictionary<string, int>> kvpUpper in chains)
            {
                string key = kvpUpper.Key;
                foreach (KeyValuePair<string, int> kvpLower in kvpUpper.Value)
                {
                    string value = kvpLower.Key + " " + kvpLower.Value;
                    //Console.WriteLine("KEY:{ " + key + " } VALUE: { " + value + " }");
                    rclient.Add(key, value);
                }
                //Console.WriteLine();
            }
        }

        private string getTwoLastWords(string s)
        {
            string[] words = s.Split(' ');
            string rString = words[words.Length - 2] + " " + words[words.Length - 1];
            return rString;
        }

        private Dictionary<string, int> getFromRedis(string key)
        {

            string value = rclient.Get<string>(key);
            if (value == null) return null;
            string[] valueSplit = value.Split(' ');

            Dictionary<string, int> returnableDic = new Dictionary<string, int>();
            returnableDic.Add(valueSplit[0], Convert.ToInt32(valueSplit[1]));

            return returnableDic;
        }
        
        private string getNextWord(string key)
        {
            Dictionary<string, int> foundWords = getFromRedis(key);

            if (foundWords == null) { return ""; }

            if (foundWords.Count == 0) return "";

            int max = foundWords.Values.Max();

            List<string> candidates = new List<string>(foundWords.Keys);

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
            int randomIdx = new Random().Next(candidates.Count);

            return candidates[randomIdx];
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
                return keys[new Random().Next(keys.Count)];
            }

            int randomIdx = new Random().Next(keysWithSeed.Count);

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

        public void selectDb(int db)
        {
            rclient.Db = db;
        }


    }
}
