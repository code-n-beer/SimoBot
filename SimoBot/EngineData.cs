using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SimoBot
{
    public class EngineData
    {
        //public static string MySQLConnectionString = "";
        public string MySQLConnectionString, LastFmAPIKey, server, port, nick, channel, localPicturePath, remotePicturePath, explPath, timerPath;
        public string RedisMainDB, RedisFastOneWordDB;

        public StreamReader ircReader;
        public StreamWriter ircWriter;
        public TcpClient tcpClient;
        public NetworkStream networkStream;
        public DateTime startTime;
        public List<string> nickList;
        public List<string> antonioLines;


        public EngineData(string configPath)
        {
            startTime = DateTime.Now;
            readConfig(configPath);
            populateAntonio();

            initSockets();

            //Initialize Last.fm thingy.

            connect();
        }

        private void populateAntonio(string filename = "antonio.txt")
        {
            StreamReader antonioReader = new StreamReader(filename);
            string line = "asdf";

            antonioLines = new List<string>();

            while (line != null)
            {
                line = antonioReader.ReadLine();
                if (line == null) break;
                antonioLines.Add(line);
            }

        }

        private void readConfig(string filename = "config.txt")
        {
            StreamReader confReader = new StreamReader(filename);
            string line = "asdf";
            //string[] confs = new string[8];

            List<string> lines = new List<string>();
            while(line != null)
            {
                line = confReader.ReadLine();
                //If a comment, skip dat shit.
                if (line == null) break;
                if (line.StartsWith("*")) continue;


                lines.Add(line);
            }

            MySQLConnectionString = lines[0];
            LastFmAPIKey = lines[1];
            server = lines[2];
            port = lines[3];
            nick = lines[4];
            channel = lines[5];
            localPicturePath = lines[6];
            remotePicturePath = lines[7];
            explPath = lines[8];
            RedisMainDB = lines[9];
            RedisFastOneWordDB = lines[10];
	    timerPath = lines[11];
        }

        private void initSockets()
        {
            tcpClient = new TcpClient(server, Convert.ToInt32(port));

            networkStream = tcpClient.GetStream();

            ircReader = new StreamReader(networkStream, Encoding.Default);
            ircWriter = new StreamWriter(networkStream, Encoding.Default);

            ircWriter.AutoFlush = true;
        }

        private void connect()
        {
            ircWriter.WriteLine("PASS kikkihiiri"); //Doesn't really matter what you put here.
            ircWriter.WriteLine("NICK " + nick);
            ircWriter.WriteLine("USER " + nick + " 8 * :" + nick);

            string line = ircReader.ReadLine();
            while(!line.Contains("/MOTD") && !line.Contains("End of MOTD"))
            {
                Console.WriteLine(line);

                if(line.Contains("PING"))
                {
                    ircWriter.WriteLine("PONG :" + line.Split(':')[1]);
                    Console.WriteLine("PONG :" + line.Split(':')[1]);
                }
                line = ircReader.ReadLine();
            }

            //Now we're connected and ready join a channel:

            ircWriter.WriteLine("JOIN " + channel);
            line = ircReader.ReadLine();
            while (!line.Contains("353"))
            {
                Console.WriteLine(line);
                line = ircReader.ReadLine();
            }
            Console.WriteLine(line);

            nickList = new Parser().BuildNickList(line); 
            //Now we've joined a channel.
        }
    }
}
