using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimoBot
{
    public class DictionaryLoader
    {
        public void AddNewLastFMNick(string nickIRC, string nickLast)
        {
            StreamWriter LastFMNickTxtWriter = new StreamWriter("LastFmNick.txt", true);
            LastFMNickTxtWriter.WriteLine(nickIRC + ' ' + nickLast);
            LastFMNickTxtWriter.Close();
        }

        public void UpdateLastFmNick(Dictionary<string, string> dictionary, string IRCnick, string lastNick)
        {
            dictionary[IRCnick] = lastNick;

            StreamWriter writer = new StreamWriter("LastFmNick.txt", false); // append = false; if the file exists, it will be overwritten.

            foreach (KeyValuePair<string, string> nicks in dictionary)
            {
                writer.WriteLine(nicks.Key + ' ' + nicks.Value);
            }

            writer.Close();
        }

        public StreamReader LastFMDirReader;

        public Dictionary<string, string> GetLastFmNickDictionary()
        {
            Dictionary<string, string> NickDir = new Dictionary<string, string>();
            string output = "asdf";

            try
            {
                LastFMDirReader = new StreamReader("LastFmNick.txt");
            }
            catch (NullReferenceException)
            {
                output = null;
            }
            catch (FileNotFoundException)
            {
                output = null;
            }
            catch (Exception SomethingElse)
            {
                string exepshun = SomethingElse.Message;
                output = null;
            }

            string[] outputArray;
            string nickLast;
            string nickIRC;


            try
            {
                while(output!=null)
                {
                    output = LastFMDirReader.ReadLine();

                    if (output == null) break;

                    outputArray = output.Split(' ');

                    nickIRC = outputArray[0].ToString();
                    nickLast = outputArray[1].ToString();

                    NickDir.Add(nickIRC, nickLast);
                }
            }
            catch (Exception)
            { }

            LastFMDirReader.Close();

            return NickDir;
        }
    }
}
