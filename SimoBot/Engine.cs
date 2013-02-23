using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MySql.Data.MySqlClient;


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
                string entry = msg.message.Replace("!wiki ", "");
                string wikiEntry = Wikipedia.ReadWikiEntry(entry);
                //if (wikiEntry.Length > 400)
                //{
                //    wikiEntry = wikiEntry.Substring(0, 400);
                //}
                Say(wikiEntry);
            }
			else if(msg.messageAsArray.Length == 1)
			{
				//wanted random
				Say(Wikipedia.ReadRandomEntry());
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
                string explanation = msg.message.Replace("!add ", "");
                explanation = explanation.Remove(0, explName.Length + 1);

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
            else if (msg.message.ToLower().Contains("simo"))
            {
                if (!(msg.message.ToLower().Contains("simonix")))
                {
                    markovTriggers(msg.messageAsArray);
                }
            }

            //addToDB(msg);
            MCR.addNewLineToRedis(msg.message);
        }
        
        private void markovTriggers(string[] msg)
        {
            //int randomIdx = new Random().Next(msg.messageAsArray.Length);
            //Say(MCR.getNewMarkov(msg.messageAsArray[randomIdx]));
            for (int i = 0; i < msg.Length - 1; i++)
            {

            }
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
            //char[] charAr = new char[msgLength];
            char[] charAr = msg.message.ToCharArray();
            string reversedMsg = "";

            for (int i = msgLength; i > 0; i--)
            {
                reversedMsg += charAr[i - 1];
            }

            reversedMsg = reversedMsg.Replace("r!", "").Trim();

            Say(reversedMsg);
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
            msg = "PRIVMSG " + DATA.channel + " :" + msg;
	    if (msg.Length > 400) msg = msg.Substring(0, 400);
            DATA.ircWriter.WriteLine(msg);
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
