using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;

namespace SimoBot
{
    class LastFmStuff
    {
        Dictionary<string, string> nickDictionary;
        string APIKey;
        string filename;
        public LastFmStuff(string APIKey, string filename = "LastFmNick.txt")
        {
            this.filename = filename;
            this.APIKey = APIKey;
            try
            {
                nickDictionary = getLastFmNickDictionary(filename);
            }
            catch (FileNotFoundException)
            {
                nickDictionary = new Dictionary<string, string>();
            }
        }

        public string runSetLastFm(Message msg)
        {
            if (msg.messageAsArray.Length < 2) { return "!setlastfm 'your-Last.fm-Nick-Here' (remove the quotes)"; }

            string lastfmNick = msg.messageAsArray[1];

            if (nickDictionary.Keys.Contains(msg.nick))
            {
                UpdateLastFmNick(msg.nick, lastfmNick);
            }
            else
            {
                AddNewLastFMNick(msg.nick, lastfmNick);
            }

            return "You can now say !np to get what '" + lastfmNick + "' is playing";
        }

        public string runNP(Message msg)
        {

            if (msg.messageAsArray.Length >= 2)
            {
                string nick = msg.messageAsArray[1];
                return getNP(nick);
            }
            else
            {
                if (nickDictionary.Keys.Contains(msg.nick))
                {
                    return getNP(nickDictionary[msg.nick]);
                }
                else
                {
                    return "Your nick wasn't found from the saved Last.Fm nicklist. Try !setlastfm 'YourLastFmNick' (without quotes)"; 
                }
            }
        }

        private string getNP(string lastFmUser)
        {
            string URL = "http://ws.audioscrobbler.com/2.0/?method=user.getrecenttracks&nowplaying=%22true%22&user="
               + lastFmUser + "&limit=1&api_key=" + APIKey;

            WebClient client = null;
            string htmlCode = "";
            try
            {
                client = new WebClient();
                htmlCode = client.DownloadString(URL);
            }
            
            catch (WebException)
            {
                return "";
            }


            List<string> htmlCodeInLines = new List<string>();

            string line = "";

            foreach (char c in htmlCode)
            {
                if (c == '\n')
                {
                    htmlCodeInLines.Add(line);
                    line = "";
                }
                else
                {
                    line += c;
                }
            }

            Regex RgxWildCard = new Regex(".*");

            string artistStr = htmlCodeInLines[4].ToString();

            int countOfQuotes = 0;
            string artist = "";
            foreach (char c in artistStr)
            {
                if (c == '"') { countOfQuotes++; }
                if (countOfQuotes == 0 || countOfQuotes == 2)
                {
                    artist += c;
                }
            }

            artist = artist.Replace(@"<artist mbid="">", null).Replace("</artist>", null).Trim().Replace("&amp;", "&");

            string trackStr = htmlCodeInLines[5].ToString();
            string track = trackStr.Replace("<name>", null).Replace("</name>", null).Replace("&amp;", "&").Trim();

            string lastFmMsg;

            if (htmlCodeInLines[3].Contains(@"nowplaying=""true"""))
            {
                lastFmMsg = lastFmUser + " is now playing: <" + artist + "> - <" + track + ">";
            }
            else
            {
                lastFmMsg = lastFmUser + " played last: <" + artist + "> - <" + track + ">";
            }

            return lastFmMsg;
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

        public void UpdateLastFmNick(string IRCnick, string lastFMNick)
        {
            nickDictionary[IRCnick] = lastFMNick;

            StreamWriter writer = new StreamWriter(filename, false); // append = false which means if the file exists, it will be overwritten.

            foreach (KeyValuePair<string, string> nicks in nickDictionary)
            {
                writer.WriteLine(nicks.Key + ' ' + nicks.Value);
            }

            writer.Close();
        }

        public void AddNewLastFMNick(string nickIRC, string lastFMNick)
        {
            StreamWriter LastFMNickTxtWriter = new StreamWriter(filename, true);
            LastFMNickTxtWriter.WriteLine(nickIRC + ' ' + lastFMNick);
            LastFMNickTxtWriter.Close();

            nickDictionary[nickIRC] = lastFMNick;
        }
    }
}
