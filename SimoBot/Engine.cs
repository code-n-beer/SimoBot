using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SimoBot
{
    public delegate void MessageHandler(Message msg);


    class Engine
    {
        Parser parser;
        EngineData DATA;
        public BackgroundWorker bgwIrcReader = new BackgroundWorker();
        LastFmStuff lastFm;

        private Dictionary<string, MessageHandler> messageHandlers;
        private Dictionary<string, MessageHandler> privMsgHandlers;

        public Engine(string configPath = "config.txt")
        {
            parser = new Parser();
            DATA = new EngineData(configPath);
            addHandlers();
            lastFm = new LastFmStuff();

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
            //privMsgHandlers["!setlastfm"] = setLastFmHandler;
            // privMsgHandlers["!uguu"] = uguuHandler;
            //privMsgHandlers["!wiki"] = wikiHandler;
            //privMsgHandlers["!r"] = reverseHandler;
            //privMsgHandlers["URLTITLE"] = URLHandler;
        }

        private void npHandler(Message msg)
        {

        }

        private void PrivMsgHandler(Message msg)
        {
            if(privMsgHandlers.Keys.Contains(msg.firstWord.ToLower()))
            {
                privMsgHandlers[msg.firstWord.ToLower()](msg);
            }
        }

        private void PingHandler(Message msg)
        {
            DATA.ircWriter.WriteLine("PONG :" + msg.message);
        }


        public void Say(string msg)
        {
            msg = "PRIVMSG " + DATA.channel + " :" + msg;
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
            }
        }

        
        
    }
}
