using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimoBot
{
    class LastFmStuff
    {
        Dictionary<string, string> nickDictionary;
        public LastFmStuff(string filename = "LastFmNick.txt")
        {
            try
            {
                nickDictionary = getLastFmNickDictionary(filename);
            }
            catch (FileNotFoundException)
            {
                nickDictionary = new Dictionary<string, string>();
            }
        }

        private Dictionary<string, string> getLastFmNickDictionary(string filename = "LastFmNick.txt")
        {
            Dictionary<string, string> NickDir = new Dictionary<string, string>();
            string output = "asdf";
            StreamReader reader = new StreamReader(filename);

            string[] outputArray;
            string nickLast;
            string nickIRC;


            try
            {
                while (output != null)
                {
                    output = reader.ReadLine();

                    if (output == null) break;

                    outputArray = output.Split(' ');

                    nickIRC = outputArray[0].ToString();
                    nickLast = outputArray[1].ToString();

                    NickDir.Add(nickIRC, nickLast);
                }
            }
            catch (Exception)
            { }

            reader.Close();

            return NickDir;
        }

        public void UpdateLastFmNick(string IRCnick, string lastNick)
        {
            nickDictionary[IRCnick] = lastNick;

            StreamWriter writer = new StreamWriter("LastFmNick.txt", false); // append = false which means if the file exists, it will be overwritten.

            foreach (KeyValuePair<string, string> nicks in nickDictionary)
            {
                writer.WriteLine(nicks.Key + ' ' + nicks.Value);
            }

            writer.Close();
        }

        public void AddNewLastFMNick(string nickIRC, string nickLast)
        {
            StreamWriter LastFMNickTxtWriter = new StreamWriter("LastFmNick.txt", true);
            LastFMNickTxtWriter.WriteLine(nickIRC + ' ' + nickLast);
            LastFMNickTxtWriter.Close();
        }
    }
}
