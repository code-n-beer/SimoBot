using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SimoBot
{

    public delegate void MessageEventHandler(object sender, MessageEventArgs e);

    // This is the EventArgs class for the MessageReceived event.
    // It includes the message to be passed on.
    public class MessageEventArgs : EventArgs
    {
        public string Message;

        public MessageEventArgs(string msg)
        {
            Message = msg;
        }
    }


    public class Engine
    {
        public event MessageEventHandler MessageReceived;

        string server; //These variables are declared here so that they are visible everywhere in the Engine
        int port;      
        string channel;
        string nick;

        bool ChannelNickListExists = false;
        List<string> ChannelNickList;

        public TcpClient tcpClient;
        public NetworkStream networkStream;
        public StreamReader ircReader;
        public StreamWriter ircWriter;

        string lastUrlPasterNick = "";
        DateTime startTime = DateTime.Now;

        string output = "derp";

        public BackgroundWorker bgwIrcReader = new BackgroundWorker();

        //public Statser StatsMachine;
        public Parser ParseMachine;
        public ExternalMySQL ExternalStatsMachine;

        public Dictionary<string, string> lastFmNickDir;

        public DictionaryLoader dictionaryLoader;

        public Engine(Parser pr, ExternalMySQL ems, DictionaryLoader DirLoader)
        {
            bgwIrcReader.DoWork += new DoWorkEventHandler(bgwIrcReader_DoWork);

            //StatsMachine = sm;
            ParseMachine = pr;
            ExternalStatsMachine = ems;
            dictionaryLoader = DirLoader;
        }

    /*    protected virtual void Console.WriteLine(string Message)
        {
            // MessageReceived needs to be checked for null to avoid NullReferenceException if no one wants to handle the event.
            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageEventArgs(Message));
            }
        }
     * */

        public void RunIrc(string Server, int Port, string Channel, string Nick) // This function is run from frmMain
        {
            bgwIrcReader.RunWorkerAsync();

            server = Server;   // And the variables are given their value here, when the engine is run.
            port = Port;
            channel = Channel;
            nick = Nick;
        }

        public string GetUguu(List<string> Nicks, bool OnlyUuu)   // A function mostly written by Matias Juntunen.
        {
            string[] IllegalNicks = { "SIMOBOT", "Q" };


            Random r = new Random();
            int idx = r.Next(Nicks.Count);

            string nick = Nicks[idx];

            while (IllegalNicks.Contains(nick))
            {
                idx = r.Next(Nicks.Count);
                nick = Nicks[idx];
            }

            string uguu = "ug";

            int NumU = r.Next(2, 16);
            for (int i = 0; i < NumU; i++)
                uguu += 'u';

            if (OnlyUuu)
            {
                return nick + ": " + uguu + '~';
            }
            else
            {
                return uguu + '~';
            }
        }

        private void bgwIrcReader_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgwIrcReader = sender as BackgroundWorker;

            tcpClient = new TcpClient(server, port);
            networkStream = tcpClient.GetStream();
            ircReader = new StreamReader(networkStream, Encoding.Default);
            ircWriter = new StreamWriter(networkStream, Encoding.Default);
            ircWriter.NewLine = "\n";
            ircWriter.AutoFlush = true;

            try
            {
                Connect();

                JoinChannel();

                CheckForMessages();
            }

            catch (Exception ebin)
            {
                Console.WriteLine("*Unexpected error - " + ebin.Message + " - at: exception IrcReader");
                /*
                tcpClient.Close();
                networkStream.Close();
                ircReader.Close();
                 */
            }


        }

        private void Connect()
        {

            ircWriter.WriteLine("PASS kikkihiiri");
            ircWriter.WriteLine("NICK " + nick);
            ircWriter.WriteLine("USER " + nick + " 8 * :" + nick);            
         
            try
            {
                lastFmNickDir = dictionaryLoader.GetLastFmNickDictionary();
                Console.WriteLine("Row number: 168"); 
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("LAST.FM NICK PRESETS FILE DOES NOT EXIST !!!");
            }
            catch (Exception dick)
            {
                string penis = dick.Message;
                Console.WriteLine("SOMETHING FUCKING OBSCURE HAPPENED !?!?!");
                Console.WriteLine("Exception message: " + penis);
            }
            Console.WriteLine("Row number: 180"); 

            try
            {
                do
                {
                    string[] outputArray = null;
                    try
                    {
                        output = ircReader.ReadLine(); //Reads a line from the network stream.
                        outputArray = output.Split(null);      // Chops the line to words.
                    }
                    catch (NullReferenceException)
                    {
                        Console.Write("NullReferenceException, output = " + output + " and" + '\n' +
                            "ircReader.ToString() +  says: " + ircReader.ToString() + '\n');
                    }
                    
                    catch (Exception aids)
                    {
                        output = null;
                        Console.WriteLine("Failed to read network stream: " + aids.Message);
                    }
                    

                    if (output == null)
                    {
                        Console.WriteLine("Output was null, this shouldn't happen before MOTD");
                    }
                    else
                    {
                        Console.WriteLine(output);
                    }

                    if (output.Contains("PING"))
                    {
                        ircWriter.Write("PONG :" + (outputArray[outputArray.Length - 1].Replace(":", null)) + "\r\n");
                        Console.WriteLine("Sent to server: 'PONG " + (outputArray[outputArray.Length - 1].Replace(":", null)) + "'");
                    }
                }
                while (!output.Contains("/MOTD")); // At irc.quakenet.org the client is ready to join channels when the server sends you the Message Of The Day.
                Console.WriteLine("Row number: 197");
            }
            catch (Exception connectionFailure)
            {
                Console.WriteLine("Connection failed: " + connectionFailure.Message);
            }
        }
        
        private void JoinChannel()
        {
            ircWriter.WriteLine("JOIN " + channel);

            do
            {
                output = ircReader.ReadLine();
            }
            while (!output.Contains("353"));

            if (output.Contains("353"))   //353 is the irc code for channel's nicklist BTW.
            {
                ChannelNickList = ParseMachine.BuildNickList(output);
                ChannelNickListExists = true;
            }
            /*
            string msg;
            if (ChannelNickListExists == true)
            {
                bool OnlyUuu = false;
                msg = "PRIVMSG " + channel + " :" + GetUguu(ChannelNickList, OnlyUuu);
            }
            else { msg = "PRIVMSG " + channel + " :Konnichiwa sekai~!"; }

            ircWriter.WriteLine(msg);  
             *             */
            /*
            Encoding utf = Encoding.UTF8;
            byte[] derp = Encoding.Unicode.GetBytes(msg); 
            byte[] mesitshun = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, derp);
            char[] MesitshunChars = new char[utf.GetCharCount(mesitshun, 0, mesitshun.Length)];
            utf.GetChars(mesitshun, 0, mesitshun.Length, MesitshunChars, 0);
            string finalmesitshun = new string(MesitshunChars); // finalmesitshun is now converted from Unicode to UTF8.
            */
        }

        private void CheckForMessages()
        {
            expl Expl = new expl("expl.txt");
            do
            {

                output = ircReader.ReadLine();

                string[] outputArray = output.Split(null);

                Console.WriteLine(output);

                if (output.Contains("PING"))
                {
                    ircWriter.WriteLine("PONG :" + (outputArray[outputArray.Length - 1].Replace(":", null)));
                }

                try
                {
                    if (output.Contains("PRIVMSG"))
                    {
                        HandlePRIVMSGs(outputArray);
                    }
                }
                catch (Exception ExHandlePRIVMSG)
                {
                    Console.WriteLine("*Unexpected error - " + ExHandlePRIVMSG.Message + " - at: exception ExHandlePRIVMSG");
                }

                try
                {
                    if (output.ToLower().Contains("!np"))
                    {
                        try
                        {
                            if (outputArray[3].ToString().Contains("!np"))
                            {
                                string[] parsedMsg = ParseMachine.ParseMsg(output, outputArray);
                                string msgAuthor = parsedMsg[1].ToString();

                                if (arrayIndexExists(outputArray, 4)) // Then user supplied the nick from whom he wants the np.
                                {
                                    string[] wordsInMessage = parsedMsg[4].ToString().Split(' ');
                                    string lastFmUser = wordsInMessage[1].ToString();

                                    LastFmNP LastFmNPMachine = new LastFmNP(ParseMachine);
                                    LastFmNPMachine.RunLastFmNP(lastFmUser, this);
                                }
                                else    // User did not supply a nick. So it seems he wants the np from an user preset to his nick
                                {
                                    LastFmNP LastFmNPMachine = new LastFmNP(ParseMachine);
                                    try
                                    {
                                        LastFmNPMachine.RunLastFmNP(lastFmNickDir[msgAuthor], this);
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        WriteToIrc("Your nick isn't in the preset list");
                                    }
                                }
                            }
                        }
                        catch (NullReferenceException) { }
                    }
                }
                catch (Exception ExLastFmNP)
                {
                    Console.WriteLine("*Unexpected error - '" + ExLastFmNP.Message + "' - at: exception ExLastFmNP");
                }

                try
                {


                    if (output.ToLower().Contains("!setlastfm"))
                    {

                        try
                        {

                            if (outputArray[3].ToString().ToLower().Contains("!setlastfm"))
                            {
                                string[] parsedMsg = ParseMachine.ParseMsg(output, outputArray);
                                string[] msgArray = parsedMsg[4].ToString().Split(' ');

                                string nickIRC = parsedMsg[1].ToString();
                                string nickLast = msgArray[1].ToString();

                                if (dicIndexExists(lastFmNickDir, nickIRC))
                                {
                                    dictionaryLoader.UpdateLastFmNick(lastFmNickDir, nickIRC, nickLast);
                                }
                                else
                                {
                                    dictionaryLoader.AddNewLastFMNick(nickIRC, nickLast);
                                    lastFmNickDir = dictionaryLoader.GetLastFmNickDictionary();
                                    WriteToIrc("The LastFM nick '" + nickLast + "' is now bound to your nick '" + nickIRC + "'");
                                }
                            }
                        }
                        catch (Exception aids) { Console.WriteLine(aids.Message + " this shouldn't have happened!?"); }

                    }
                    
                }
                catch (Exception LastFmStuff)
                {
                    Console.WriteLine("*Unexpected error - " + LastFmStuff.Message + " - at exception LastFmStuff");
                }

                try
                {
                    if (outputContainsUrl(output) && output.Contains("#"))  // This part does the magic that gets the title of the link pasted to irc and chats it to channel
                    {
                        string[] parsedmsg = ParseMachine.ParseMsg(output, outputArray);

                        DateTime stopTime = DateTime.Now;
                        TimeSpan elapsedTime = stopTime - startTime;

                        if (parsedmsg[1].ToString() == lastUrlPasterNick && elapsedTime.TotalSeconds < 4)
                        {

                        }
                        else
                        {
                            URLTitle URLTitleMachine = new URLTitle(ParseMachine);
                            URLTitleMachine.RunURLTitle(output, this);
                            lastUrlPasterNick = parsedmsg[1].ToString();
                            startTime = DateTime.Now;
                        }
                    }
                }
                catch (Exception ExURLTitle)
                {
                    Console.WriteLine("*Unexpected error - '" + ExURLTitle.Message + "' - at: exception ExURLTitle");
                }


                try // Uguu stuff put in a function. It still looks a bit messy...
                {
                    if (output.ToLower().Contains("!uguu"))
                    {
                        if (outputArray[3].ToLower().Contains("!uguu"))
                        {
                            string msg = MakeUguuMsg(output, outputArray);
                            ircWriter.WriteLine(msg);
                        }
                    }
                }
                catch (Exception ExUguu)
                {
                    Console.WriteLine("*Unexpected error - '" + ExUguu.Message + "' - at: exception ExUguu");
                }

                if (output.ToLower().Contains("!add") || output.ToLower().Contains("!expl") || output.ToLower().Contains("!remove"))
                {
                    if (outputArray.Length >= 4)
                    {
                        string[] ParsedMessageArray = ParseMachine.ParseMsg(output, outputArray);
                        string[] msgSplitByNull = ParsedMessageArray[4].Split(' ');
                        List<string> msgWordsList = new List<string>();


                        for (int i = 0; i < msgSplitByNull.Length; i++)
                        {
                            if (msgSplitByNull[i] != "")
                            {
                                msgWordsList.Add(msgSplitByNull[i]);
                            }
                        }



                        if (outputArray[3].ToLower().Contains("!expl"))
                        {
                            if (msgWordsList.Count >= 2)
                            {
                                 WriteToIrc(Expl.explain(msgWordsList[1]));
                            }
                            else
                            {
                                 WriteToIrc(Expl.explain());
                            }
                        }
                        if (outputArray[3].ToLower().Contains("!add"))
                        {
                            if (msgWordsList.Count > 2)
                            {
                                //Expl.addExpl(msgWordsList[1]
                                string expl = msgWordsList[2];
                                if (msgWordsList.Count > 3)
                                {
                                    for (int i = 3; i < msgWordsList.Count; i++)
                                    {
                                        expl += " " + msgWordsList[i];
                                    }
                                }
                                WriteToIrc(Expl.addExpl(msgWordsList[1], expl));
                            }
                            else
                            {
                                WriteToIrc("Name for expl is required. !add <expl_name> <expl>");
                            }
                        }

                        if (outputArray[3].ToLower().Contains("!remove"))
                        {
                            if (msgWordsList.Count > 1)
                            {
                                Expl.remove(msgWordsList[1]);
                            }
                        }
                    }
                }
            }
            while (output != null);

            if (output == null)
            {
                tcpClient.Close();
                ircReader.Close();
                ircWriter.Close();

                RunIrc(server, port, channel, nick);


            }
        }

        private string MakeUguuMsg(string output, string[] outputArray)
        {
            bool OnlyUuu;

            string[] ParsedMessageArray = ParseMachine.ParseMsg(output, outputArray);
            string[] msgSplitByNull = ParsedMessageArray[4].Split(' ');
            List<string> msgWordsList = new List<string>();


            for (int i = 0; i < msgSplitByNull.Length; i++)
            {
                if (msgSplitByNull[i] != "")
                {
                    msgWordsList.Add(msgSplitByNull[i]);
                }
            }


            if (msgWordsList.Count >= 2) //Aka there is more in the msg than the !uguu. Presuming the next word is nick, we discard everything that follows.
                OnlyUuu = true;
            else
                OnlyUuu = false;

            string msg;
            if (OnlyUuu)
                msg = "PRIVMSG " + channel + " :" + msgWordsList[1] + ": " + GetUguu(ChannelNickList, OnlyUuu);
            else
                msg = "PRIVMSG " + channel + " :" + GetUguu(ChannelNickList, OnlyUuu);

            return msg;
        }

        public void WriteToIrc(string msg)
        {
            msg = "PRIVMSG " + channel + " :" + msg;
            ircWriter.WriteLine(msg);
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

        private void HandlePRIVMSGs(string[] outputArray)
        {
            try
            {
                string[] ParsedMessageArray = ParseMachine.ParseMsg(output, outputArray);
                //AWLTMSMachine.addToDB(ParsedMessageArray);
                //StatsMachine.MakeStats(output, outputArray);
                ExternalMySQLHandling(ParsedMessageArray);
                handleReverse(ParsedMessageArray);
                

            }
            catch (Exception ebidebin)
            {
                Console.WriteLine(ebidebin.Message);
            }
        }
        /*
        private void handleTimer(string[] ParsedMsgArray)
        {
            string[] wordsInMsg = ParsedMsgArray[4].Split(' ');
            if (wordsInMsg[0].ToLower() == "!timer")
            {
                string 
            }
        }
        */
        private void handleReverse(string[] ParsedMsgArray)
        {
            string[] wordsInMsg = ParsedMsgArray[4].Split(' ');
            if (wordsInMsg[0] == ":!r" || wordsInMsg[0] == ":!R")
            {
                string reversedMsg = ParseMachine.ReverseMsg(ParsedMsgArray[4].ToString().Remove(ParsedMsgArray[4].IndexOf(':'), 1));
                reversedMsg = reversedMsg.Replace("r!", "");
                reversedMsg = reversedMsg.Replace("R!", "");
                //reversedMsg = reversedMsg;
                //reversedMsg =  reversedMsg.Replace(":", "");
                reversedMsg = reversedMsg.Trim();

                WriteToIrc(reversedMsg);
            }
        }

        private void ExternalMySQLHandling(string[] ParsedMessageArray)
        {
            string ParsedMessage = ParsedMessageArray[0].ToString() +
                             " " + ParsedMessageArray[1].ToString() +
                             " " + ParsedMessageArray[2].ToString() +
                             " " + ParsedMessageArray[3].ToString() +
                             " " + ParsedMessageArray[4].ToString();

            ExternalStatsMachine.QueueToExternalMySQL(ParsedMessageArray, ParsedMessage);

        }

        private bool arrayIndexExists(string[] what, int index)
        {
            try
            {
                string derp = what[index].ToString();
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            catch (Exception asfd)
            {
                string asdf = asfd.Message; // This shouldn't have happened
            }

            return true;
        }

        private bool dicIndexExists(Dictionary<string, string> dictionary, string index)
        {
            try
            {
                string derp = dictionary[index].ToString();
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
            return true;
        }

    }
}
