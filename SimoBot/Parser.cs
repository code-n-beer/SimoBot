using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimoBot
{
    public class Parser
    {
        public string[] ParseMsg(string wholeline, string[] wordsInLine)    // DateTime = 0 | Nick = 1 | Channel = 2 | Action = 3 | Msg = 4 |
        {

            /*
             * Parses a string such as
             * 
             * ":Asdf!~Name@a7890.elisa-laajakaista.fi PRIVMSG #asdf :asdf 1234 jkl;"
             * 
             * to an array with indexes containing the following:
             * 
             * [0] = Time in the format:  "year.month.day hour.minute.second"
             * [1] = Nick f.ex:           "Asdf"
             * [2] = Channel. f.ex:       "#asdf"
             * [3] = Action. f.ex:        "PRIVMSG"
             * [4] = Message. f.ex:       "asdf 1234 jkl;"
             * 
             */

            string[] parsedMsg = new string[5]; // Initialize parsedMsg


            // Get nick
            char[] FirstWordCharacterArray = wordsInLine[0].ToCharArray();
            char f = ' ';
            int i;
            for (i = 0; i < FirstWordCharacterArray.Length && f != '!'; i++)
            {
                f = FirstWordCharacterArray[i];
            }
            string nickBuffer = wholeline.Remove(i - 1);
            parsedMsg[1] = nickBuffer.Replace(" ", null).Replace(":", null);
            //Get nick ends

            //Time get
            string year = DateTime.Now.Year.ToString();
            string day = DateTime.Now.Day.ToString();
            string month = DateTime.Now.Month.ToString();
            string hour = DateTime.Now.Hour.ToString();
            string minute = DateTime.Now.Minute.ToString();
            string second = DateTime.Now.Second.ToString();

            parsedMsg[0] = year + '.' + month + '.' + day + ' '
                + hour + '.' + minute + '.' + second;


            parsedMsg[2] = wordsInLine[2]; // Channel
            parsedMsg[3] = wordsInLine[1]; // Action

            //Msg
            int a;
            for (a = 3; a < wordsInLine.Length; a++)
            {
                if (a < 3) { continue; }
                else
                {
                    //if (wordsInLine[a].Contains(':'))
                    //    wordsInLine[a] = wordsInLine[a].Remove(wordsInLine[a].IndexOf(':'), 1);  // What the hell is happening here?
                    parsedMsg[4] += wordsInLine[a] + " ";
                    parsedMsg[4] = parsedMsg[4].Replace("'", "´").Replace("„Ã", "Ä").Replace("–Ã", "Ö").Replace("¤Ã", "ä").Replace("¶Ã", "ö"); ;
                }
            }
            return parsedMsg;
            //Msg END

        }
        
        public List<string> BuildNickList(string output)
        {
            output = output.Replace(":", "").Replace("+", "").Replace("@", "");
            string[] OutputArray = output.Split(null);
            
            List<string> ChannelNickList = new List<string>();

            for (int i = 5; i < OutputArray.Length; i++)
            {
                    ChannelNickList.Add(OutputArray[i]);
            }
            return ChannelNickList;
        }

        //Easy way to check whether given URL is valid. Accepts www.xyz.com style URLs,
        //does not allow xyz.com style URLs. Don't know about svn:// and other non-http's
        public Regex RgxUrl = new Regex("(((https|http|ftp):\\/\\/)|www\\.)(([0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+)|localhost|([a-zA-Z0-9\\-]+\\.)*[a-zA-Z0-9\\-]+\\.(com|net|org|info|biz|gov|name|edu|[a-zA-Z][a-zA-Z]))(:[0-9]+)?((\\/|\\?)[^ \"]*[^ ,;\\.:\">)])?");

        public string GetUrl(string output) 
        {
            string URL = "";

            string[] outputArray = output.Split(null);

            int i = 0;

            try
            {
                while (!RgxUrl.IsMatch(outputArray[i]))
                {
                    i++;
                }

                URL = outputArray[i].ToString();

                URL = URL.TrimStart(':');
                
                if (!URL.ToLower().Contains("http://") && !URL.ToLower().Contains("https://"))
                {
                    URL = "http://" + URL; //.Replace(":", "");
                }
            }
            catch (NullReferenceException)
            {
                URL = "Failed to get URL.";
            }

            return URL;
        }

        public string ReverseMsg(string msg)
        {
            
            int msgLength = msg.Length;
            //char[] charAr = new char[msgLength];
            char[] charAr = msg.ToCharArray();
            string reversedMsg = "";

            for (int i = msgLength; i > 0; i--)
            {
                reversedMsg += charAr[i-1];
            }

            return reversedMsg;
        }
    }
}
