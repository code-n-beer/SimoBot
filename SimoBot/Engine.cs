using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace SimoBot
{
    public delegate void MessageHandler(Message msg);


    class Engine
    {
        Expl expl;
        Parser parser;
        EngineData DATA;
        public BackgroundWorker bgwIrcReader = new BackgroundWorker();
        LastFmStuff lastFm;
        URLTitleAndPictureSave URLTAPS;
        MarkovChainTest.MarkovChainRedis MCR;
		TimerHandler timerHandler;
		SimoTwitter simoTwitter;
		FinnishNewsTicker newsTicker;
        //Wikipedia wiki;

        private Dictionary<string, MessageHandler> messageHandlers;
        private Dictionary<string, MessageHandler> privMsgHandlers;

        public Engine(string configPath = "config.txt")
        {
            parser = new Parser();
            DATA = new EngineData(configPath);
            expl = new Expl(DATA.explPath);
            MCR = new MarkovChainTest.MarkovChainRedis(Convert.ToInt32(DATA.RedisMainDB), Convert.ToInt32(DATA.RedisFastOneWordDB));
            addHandlers();
            lastFm = new LastFmStuff(DATA.LastFmAPIKey);
            URLTAPS = new URLTitleAndPictureSave(DATA.localPicturePath, DATA.remotePicturePath, DATA.MySQLConnectionString);
			timerHandler = new TimerHandler(DATA.timerPath, this);
			simoTwitter = new SimoTwitter(DATA.twitterCredentials);
			newsTicker = new FinnishNewsTicker(DATA.allowedArticleWebsites, MCR);

			addKickRegexes();

            bgwIrcReader.DoWork += new DoWorkEventHandler(bgwIrcReader_DoWork);
            bgwIrcReader.RunWorkerAsync();


        }

        private void addHandlers()
        {
            messageHandlers = new Dictionary<string, MessageHandler>();
            messageHandlers["PING"] = PingHandler;
            messageHandlers["PRIVMSG"] = PrivMsgHandler;

            privMsgHandlers = new Dictionary<string, MessageHandler>();

            //The following do not exist yet...
            privMsgHandlers["!np"] = npHandler;
            privMsgHandlers["!setlastfm"] = setLastFmHandler;
            privMsgHandlers["!uguu"] = uguuHandler;
            //privMsgHandlers["!wiki"] = wikiHandler;
            privMsgHandlers["!r"] = reverseHandler;
            //privMsgHandlers["URLTITLE"] = URLHandler;
            privMsgHandlers["!expl"] = explHandler;
            privMsgHandlers["!add"] = addHandler;
            privMsgHandlers["!remove"] = removeHandler;
            privMsgHandlers["!wiki"] = wikiHandler;
            //privMsgHandlers["!m"] = markovHandler;
            privMsgHandlers["simobot"] = comebackHandler;
            privMsgHandlers["simobot:"] = comebackHandler;
            privMsgHandlers["simobot,"] = comebackHandler;
            privMsgHandlers["!switchdb"] = switchHandler;
            privMsgHandlers["!antonio"] = antonioHandler;
			privMsgHandlers["!timer"] = timerCommandHandler;
			privMsgHandlers["!timerremove"] = removeTimerHandler;
			privMsgHandlers["!tweet"] = tweetHandler;
			privMsgHandlers["!random"] = randomHandler;
			privMsgHandlers["!news"] = newsHandler;
			privMsgHandlers["!simonews"] = simoNewsHandler;
			privMsgHandlers["!resetnews"] = resetNewsHandler; 
        }

		private void resetNewsHandler(Message msg)
		{
			newsTicker.resetTickers();
			Say("Reset'd");
		}

		private void simoNewsHandler(Message msg)
		{
			string newsLine = newsTicker.getNewTick();

			if (newsLine.StartsWith("Couldn't"))
			{
				Say("Couldn't find new articles :D");
				return;
			}

			newsLine = newsLine.Replace(" :D ", "");

            string[] newsLineArray = newsLine.Split(' ');

			string markovString = newsLineArray[0] + " " + newsLineArray[1];

            Console.WriteLine(markovString);
			Say(MCR.getNewMarkov(markovString));
		}

		private void newsHandler(Message msg)
		{
			try
			{
				string tick = newsTicker.getNewTick();

                if(tick.StartsWith("Couldn't"))
				{
                    Say("Couldn't find new articles :D");
                    return;
				}

				Say("News: " + tick);
			}
			catch (Exception e)
			{
				Console.WriteLine("Main handler error: " + e.Message);
				newsHandler(msg);
			}
		}

		private void randomHandler(Message msg)
		{
			Console.WriteLine(msg.message);
			if (msg.message.Trim().Split(' ').Length < 3 || msg.message.Trim().Split(' ').Length > 3)
			{
				Say("!random <start> <end>");
				return;
			}
	
			string[] numbers = msg.message.Substring(8).Split(' ');

			try
			{
				int start = Convert.ToInt32(numbers[0]);
				int end = Convert.ToInt32(numbers[1]);

				int random = new Random().Next(start, end + 1);

				Say(random.ToString());
			}
			catch (Exception e)
			{
				Say(e.Message);
			}


		}

		private void tweetHandler(Message msg)
		{
			string fail;
			string newTweet = msg.message.Substring(6).Trim(); //length of "!tweet" ... sigh.
			if (Parser.isLegitTweet(out fail, newTweet))
			{
				try
				{
					justInCaseFile(msg);
					Say(simoTwitter.tweet(newTweet));
				}
				catch (Exception e)
				{
					Console.WriteLine("Tweeting '" + newTweet + "' failed because: " + e.Message);
				}
			}
			else
			{
				Say(fail);
			}
		}

		private void justInCaseFile(Message msg)
		{
			StreamWriter writer = new StreamWriter("tweetlog.txt", true);
			writer.WriteLine(DateTime.Now.ToString() + " --- '" + msg.nick + "' tried '" + msg.message + "'");
			writer.Flush();
			writer.Close();
		}

		private void timerCommandHandler(Message msg)
		{
			try
			{
				if (msg.messageAsArray.Length > 2)
				{
					string message = msg.message.Replace(msg.messageAsArray[0], "").Replace(msg.messageAsArray[1], "").Trim();
					Say(timerHandler.addTimer(msg.nick, message, msg.messageAsArray[1]));
				}
				else
				{
					Console.WriteLine("messageAsArray too short");
				}
			}
			catch(Exception e)
			{
				Console.WriteLine("Timer crashed: " + e.Message);
			}
		}

		private void removeTimerHandler(Message msg)
		{
			int idx;
			try
			{
				if (msg.messageAsArray.Length >= 2)
				{
					string command = msg.messageAsArray[1].ToLower();
					if (command != "all" && command != "first" && command != "last")
					{
						idx = Convert.ToInt32(msg.messageAsArray[1]);
						Say(timerHandler.removeTimer(msg.nick, idx));
					}
					else
					{
						Say(timerHandler.removeTimer(msg.nick,command));
					}
				}
				else
				{
					Console.WriteLine("messageAsArray too short");
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Say("!timerremove wants a number. You gave something else");
			}
		}

        private void antonioHandler(Message msg)
        {
            int idx = new Random().Next(0, DATA.antonioLines.Count - 1);
            Say(DATA.antonioLines[idx]);
        }

        private void switchHandler(Message msg)
        {
            try
            {
                int db = Convert.ToInt32(msg.messageAsArray[1]);
                MCR.selectDb(db);
                Say("Changed to " + db);
            }

            catch (Exception e)
            {
                Say("Failed to change");
            }
        }

        private void comebackHandler(Message msg)
        {
            if(msg.messageAsArray.Length == 1)
                Say(msg.nick + ": " + MCR.getNewMarkov(""));
            else if (msg.messageAsArray.Length == 2) // Aka nick + 1 word
            {
                Say(msg.nick + ": " + MCR.getNewMarkov(msg.messageAsArray[1]));
            }
            else if (msg.messageAsArray.Length > 2) // Aka nick + more than 1 word
            {
                Say(msg.nick + ": " + MCR.getNewMarkov(msg.messageAsArray[1] + " " + msg.messageAsArray[2]));
            }
        }

        private void markovHandler(Message msg)
        {
            try
            {
                if(msg.messageAsArray.Length == 1)
                    Say(MCR.getNewMarkov(msg.messageAsArray[1]));
                if (msg.messageAsArray.Length > 2)
                    Say(MCR.getNewMarkov(msg.messageAsArray[1] + " " + msg.messageAsArray[2]));
                else
                    Say(MCR.getNewMarkov(""));
            }
            catch (Exception e)
            {
                Say(e.Message);
            }
        }

        private void wikiHandler(Message msg)
        {
            if (msg.messageAsArray.Length >= 2)
            {
				if (msg.messageAsArray[1].ToLower() == "en")
				{
					Say(Wikipedia.ReadRandomEntry("en"));
					return;
				}
				else if(msg.messageAsArray[1].ToLower() == "fi")
				{
					Say(Wikipedia.ReadRandomEntry("fi"));
					return;
				}
                string entry = msg.message.Replace("!wiki ", "");
                string wikiEntry = Wikipedia.ReadWikiEntry(entry);
                //if (wikiEntry.Length > 400)
                //{
                //    wikiEntry = wikiEntry.Substring(0, 400);
                //}
                Say(wikiEntry);
            }
        }

        private void explHandler(Message msg)
        {
            if (msg.messageAsArray.Length == 2)
            {
                Say(expl.explain(msg.messageAsArray[1]));
            }
            else if (msg.messageAsArray.Length == 1)
            {
                Say(expl.explain());
            }
            else
            {
                Say("!expl command takes one (to explain a certain word) or zero arguments(to get a random expl). No less, no more.");
            }
        }

        private void addHandler(Message msg)
        {
            if (msg.messageAsArray.Length < 3)
            {
                Say("!add expl_name actual expl");
                return;
            }

            try
            {

                string explName = msg.messageAsArray[1];
                string explanation = msg.message.Substring(explName.Length + "!add ".Length);

                Say(expl.addExpl(explName, explanation));
            }
            catch (Exception e)
            {
                Say("Something went wrong, did not add");
                Console.WriteLine("Fail: " + e.Message);
            }


            //expl.addExpl(msg.messageAsArray[1],
        }

        private void removeHandler(Message msg)
        {
            if (msg.messageAsArray.Length < 2)
            {
                Say("!remove expl_name");
            }

            expl.remove(msg.messageAsArray[1]);
        }

        /*
        //not yet implemented anywhere
        string[] markovTriggers =
        {
        "simo",
        "pääsiäinen",
        "kakka"
        };
        */
        private void PrivMsgHandler(Message msg)
        {
            if (privMsgHandlers.Keys.Contains(msg.firstWord.ToLower()))
            {
                privMsgHandlers[msg.firstWord.ToLower()](msg);
            }
            else if (containsUrl(msg.message))
            {
                try
                {
                    //Say(URLTAPS.getURLTitle(msg));
                    string asd = URLTAPS.getURLTitle(msg);
                    if (asd != "fail")
                        Say(asd);
                }
                catch (Exception e)
                {
                    //Say(e.Message);
                }
            }

            markovTriggers(msg);

            MCR.addNewLineToRedis(msg.message);
        }

		private Regex RgxUrl = new Regex("(((https|http|ftp):\\/\\/)|www\\.)(([0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+)|localhost|([a-zA-Z0-9\\-]+\\.)*[a-zA-Z0-9\\-]+\\.(com|net|org|info|biz|gov|name|edu|[a-zA-Z][a-zA-Z]))(:[0-9]+)?((\\/|\\?)[^ \"]*[^ ,;\\.:\">)])?");

		List<Regex> regexit = new List<Regex>();

		private void addKickRegexes()
		{
			regexit.Add(new Regex(".*top.*le.*"));
			//regexit.Add(new Regex(".*flora.*lel.*"));
			regexit.Add(new Regex(@".*fl[aeiouyäöåAEIOUYÄÖÅ]*ra\s*l[aeiouyäöåAEIOUYÄÖÅ]*l.*"));
			//regexit.Add(new Regex(@".*:\).*"));
		}
        
        private void markovTriggers(Message msg)
        {
			for (int i = 0; i < regexit.Count; i++)
			{
				string reversedString = reverseString(msg.message);
				if (regexit[i].IsMatch(msg.message))
				{
					kick(msg.nick, "Kicked due to: " + regexit[i].ToString());
				}
			}
            //int randomIdx = new Random().Next(msg.messageAsArray.Length);
            //Say(MCR.getNewMarkov(msg.messageAsArray[randomIdx]));
        }

		public void kick(string nick, string msg)
		{
			if (msg.Length > 400) msg = msg.Substring(0, 400);
			string outputMsg = "KICK " + DATA.channel + " " + nick + " :" + msg;
			DATA.ircWriter.WriteLine(outputMsg);
		}


        public void addToDB(Message msg)
        {
            string CommandString = "INSERT INTO msgs VALUES ('" +
                   msg.time + "','" + msg.nick.Replace(@"Diaz\", "Diazzlash").Replace(@"\", "slash") +
                   "','" + msg.channel + "','" + msg.action +
                   "','" + msg.message.Replace(";", "").Replace("'", "") + "');";


            string ConnectionString = DATA.MySQLConnectionString;
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            MySqlCommand cmd = conn.CreateCommand();
            MySqlDataReader Reader;

            cmd.CommandText = CommandString;
            try
            {
                conn.Open();
                Reader = cmd.ExecuteReader();
                //Reader.Read();

                conn.Close();
            }
            catch (MySqlException mysliexepshun)
            {
                string durp = mysliexepshun.Message;
                Console.WriteLine("MySQL failed: " + durp);
            }
        }

        private void reverseHandler(Message msg)
        {
            int msgLength = msg.message.Length;

            string reversedMsg = "";

			reversedMsg = reverseString(msg.message);

            Say(reversedMsg);
        }

		private string reverseString(string theString)
		{
			string reversedString = "";

			for (int i = theString.Length; i > 0; i--)
			{
				reversedString += theString[i - 1];
			}

			reversedString = reversedString.Replace("r!", "").Trim();

			return reversedString;
		}

        private void uguuHandler(Message msg)
        {
            //TODO: refresh nicklist incase of joins/quits

            string[] IllegalNicks = { "SIMOBOT", "Q" };

            Random r = new Random();
            int idx = r.Next(DATA.nickList.Count);

            string nick = DATA.nickList[idx];

            while (IllegalNicks.Contains(nick))
            {
                idx = r.Next(DATA.nickList.Count);
                nick = DATA.nickList[idx];
            }

            string uguu = "ug";

            int NumU = r.Next(2, 16);
            for (int i = 0; i < NumU; i++)
                uguu += 'u';


            Say(nick + ": " + uguu + '~');
        }

        private void setLastFmHandler(Message msg)
        {
            Say(lastFm.runSetLastFm(msg));
        }

        private void npHandler(Message msg)
        {
            Say(lastFm.runNP(msg));
        }

        private bool containsUrl(string input)
        {
            input = input.ToLower();
            if (input.Contains("www.") || input.Contains("http://") || input.Contains("https://")) //Add more if necessary..?
            {
                if (input.Contains("[") || input.Contains("]"))
                    return false;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void PingHandler(Message msg)
        {
            DATA.ircWriter.WriteLine("PONG :" + msg.message);
        }

        public void Say(string msg)
        {
            /*
			for (int i = 0; i < regexit.Count; i++)
			{
				if (regexit[i].IsMatch(msg))
				{
					Say("Oh u...");
					return;
				}
			}
            */

            msg = "PRIVMSG " + DATA.channel + " :" + msg;
	        if (msg.Length > 400) msg = msg.Substring(0, 400);

			string text = msg;

			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				if (i < text.Length - 1 && //check that we're not at the last character
					c == '.' &&
					text[i + 1] != '.' &&
					text[i + 1] != ' ')
				{
					text = text.Substring(0, i) + " " + text.Substring(i + 1);
				}

				int charCount = 0;
				if (c == '&')
				{
					charCount++;
					for (int j = i; j < text.Length; j++)
					{
						if (text[j] == ' ')
							break;

						if (text[j] == ';')
						{
							text = text.Remove(i, charCount);
							break;
						}

					}
				}

				charCount = 0;
				if (c == '#')
				{
					charCount++;
					for (int j = i; j < text.Length; j++)
					{
						if (text[j] == ' ')
							break;

						if (text[j] == ';')
						{
							text = text.Remove(i, charCount);
							break;
						}

					}
				}
			}

            DATA.ircWriter.WriteLine(text);
        }

        private void bgwIrcReader_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgwIrcReader = sender as BackgroundWorker;

            //while(something) ?
            run();
        }

        private void run()
        {
            string input = DATA.ircReader.ReadLine();
            while (input != null)
            {
                Console.WriteLine(input);
                Message msg = parser.ParseMsg(input);

                if (messageHandlers.Keys.Contains(msg.action))
                {
                    messageHandlers[msg.action](msg);
                }

                input = DATA.ircReader.ReadLine();


                //Perhaps a good idea to sleep once in a while so we don't just use all processing available
                //when waiting for something to happen.
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
