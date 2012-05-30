using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimoBot
{
    public class Parser
    {
        public Message ParseMsg(string wholeline)    // DateTime = 0 | Nick = 1 | Channel = 2 | Action = 3 | Msg = 4 |
        {
            string[] wordsInLine = wholeline.Split(' ');

            //Wololo, I'll just leave this here.
            if(wordsInLine.Length < 3 && wholeline.Contains("PING"))
            {
                return new Message("PING", wordsInLine[1].Replace(":",""));
            }

            if (wordsInLine.Length < 4)
            {
                return new Message("penis", "asdf");
            }


            /*
             * Parses a string such as
             * 
             * ":Asdf!~Name@a7890.asdf-broadband.eu PRIVMSG #asdf :asdf 1234 jkl;"
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
                parsedMsg[4] += wordsInLine[a] + " ";
                parsedMsg[4] = parsedMsg[4].Replace("'", "´").Replace("„Ã", "Ä").Replace("–Ã", "Ö").Replace("¤Ã", "ä").Replace("¶Ã", "ö");
                if (parsedMsg[4].StartsWith(":"))
                {
                    parsedMsg[4] = parsedMsg[4].Remove(0, 1);
                }
            }

            return new Message(parsedMsg);
            //Msg END

        }

        private bool outputContainsUrl(string output)
        {
            if (output.Contains("www.") || output.Contains("WWW.") || output.Contains("http://") || output.Contains("https://")) //Add more if necessary..? OR JUST FUCKING FIX THIS WHOLE PIECE OF SHIT
            {
                return true;
            }
            else
            {
                return false;
            }
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
        //does not allow xyz.com style URLs. doesn't accept other than https, http, ftp
        

    }
}