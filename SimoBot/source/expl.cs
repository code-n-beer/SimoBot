using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimoBot
{
    public class expl
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();

        string fileName = "";

        public expl(string fN)
        {
            fileName = fN;
            StreamReader reader;
            try
            {
                reader = new StreamReader(fileName);
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

            dictionary = new Dictionary<string, string>();

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
            name = name.ToLower();
            //expl = expl.ToLower();

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
                    expl = dictionary[name] + " | " + expl;
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
            StreamWriter writer = new StreamWriter(fileName, false);
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
