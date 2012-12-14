using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimoBot
{
    class Expl
    {
        Dictionary<string, string> dictionary;

        string filename = "";

        public Expl(string fN = "expl.txt")
        {
            dictionary = new Dictionary<string, string>();
            filename = fN;
            loadDictionary(filename);
        }

        private void loadDictionary(string filename = "expl.txt")
        {
            StreamReader reader;
            try
            {
                reader = new StreamReader(filename);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Expl file not found");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - aids happened");
                return;
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
        }

        public string addExpl(string name, string expl)
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
                refreshTextFile();
                return explain(name);
            }
            else
            {
                if (dictionary.ContainsKey(name))
                {
                    if ((dictionary[name] + " | " + expl).Length > 400)
                    {
                        return "Proposed new expl doesn't fit into one message character limit";
                    }
                    expl = dictionary[name] + " | " + expl.Trim();
                    dictionary[name] = expl;
                    refreshTextFile();
                    return explain(name);
                }
                else
                {
                    dictionary.Add(name, expl);
                    refreshTextFile();
                    return explain(name);
                }
            }
        }

        private void refreshTextFile()
        {
            StreamWriter writer = new StreamWriter(filename, false);
            foreach (KeyValuePair<string, string> aids in dictionary)
            {
                writer.WriteLine(aids.Key + "|" + aids.Value);
            }
            writer.Flush();
            writer.Close();
        }

        public string explain(string word)
        {
            word = word.ToLower();

            if (dictionary.ContainsKey(word))
            {
                return word + " : " + dictionary[word];
            }
            return "No such expl";
        }

        public string explain()
        {
            string key = dictionary.Keys.ElementAt(random(dictionary.Keys.Count));
            return key + " : " + dictionary[key];
        }

        private int random(int max)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            return random.Next(0, max);
        }

        public void remove(string what)
        {
            what = what.ToLower();
            if(dictionary.ContainsKey(what))
                dictionary.Remove(what);

            refreshTextFile();
        }
    }
}
