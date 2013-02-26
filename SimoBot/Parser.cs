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

	static Regex numbers = new Regex("^[0-9]");
	static Regex charactersAZaz = new Regex("^[a-zA-Z]");
	static Regex dhms = new Regex("^[dhmsDHMS]");

	static public bool isLegitTweet(out string fail, string tweet)
	{
		if (tweet.Length > 140)
		{
			fail = "Tweet was over 140 characters";
			return false;
		}
		if (tweet.Contains("@"))
		{
			fail = "Tweet contained '@'. Not allowed.";
			return false;
		}
		fail = null;
		return true;
	}

	static public bool isValidTime(string time)
	{
		if (time.Length < 2)
			return false;

		//if(!numbers.IsMatch(time[0].ToString()))
		//	return false;

		if (!"0123456789".Contains(time[0]))
			return false;

		for (int i = 1; i < time.Length; i++)
		{
			char c = time[i];

			if (dhms.IsMatch(time[i].ToString())) // if(d or h or m or s)
			{

				if ( ! numbers.IsMatch(time[i - 1].ToString()))
				{
					//Is not preceded by a number, hence the datetime format is flawed
					return false;
				}

				if (time.IndexOf(time[i]) != time.LastIndexOf(time[i])) //ie: contains more than one of the same character -> format flawed
				{
					return false;
				}
			}
		}



		return true;
	}

		static public DateTime convertToDateTimeAndGetAlarmTime(string time)
		{
			int days = 0;
			int hours = 0;
			int minutes = 0;
			int seconds = 0;

			DateTime now = DateTime.Now;

			for (int i = time.Length - 1; i >= 0; i--) //Disgusting, terrible code. Please look away ^__^. It's actually funny how utterly confusing it is.
			{
				char c = time[i];
				if(dhms.IsMatch(c.ToString())) // If d/h/m/s
				{
					string number = "";

					//for (char curChar = time[i-1]; numbers.IsMatch(curChar.ToString()); i--)
					//{
					//	curChar = time[i - 1];
					//	number = curChar + number;
					//}

					int j = i - 1;
					while (numbers.IsMatch(time[j].ToString()))
					{
						number = time[j] + number;
						if (j == 0)
							break;
						j--;
					}
					c  = c.ToString().ToLower().ToCharArray()[0]; //An another monstrosity. If you haven't cringed a single time so far, never code anything yourself again, ever.

					int intNumber = Convert.ToInt32(number);
					switch (c)
					{
						case 'd':
							days = intNumber;
							break;
						case 'h':
							hours = intNumber;
							break;
						case 'm':
							minutes = intNumber;
							break;
						case 's':
							seconds = intNumber;
							break;
					}
				    //Adding one because the for loop end statement will soon deduct 1 from it again. This ist he most terrible thing ever :D
					//but it does make it possible to skip the numbers we already went through. 
				}
			}

			DateTime alarm = new DateTime();
			alarm = now;
			alarm = alarm.AddDays(days);
			alarm = alarm.AddHours(hours);
			alarm = alarm.AddMinutes(minutes);
			alarm = alarm.AddSeconds(seconds);

			return alarm;
		}
    }
}