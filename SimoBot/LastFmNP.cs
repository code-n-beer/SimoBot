using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;


namespace SimoBot
{
    class LastFmNP
    {
        BackgroundWorker bgwLastFmNP = new BackgroundWorker();

        public Parser ParseMachine;

        public LastFmNP(Parser pr)
        {
            bgwLastFmNP.DoWork += new DoWorkEventHandler(bgwLastFmNP_DoWork);

            ParseMachine = pr;
        }

        public Engine engine;
        string lastFmUser = "";


        public void RunLastFmNP(string lFU, Engine eng)
        {
            engine = eng;
            lastFmUser = lFU;

            if (!bgwLastFmNP.IsBusy)
                bgwLastFmNP.RunWorkerAsync();
        }


        private void bgwLastFmNP_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgwLastFmNP = sender as BackgroundWorker;

            DoLastFmNP();
        }

        private void DoLastFmNP()
        {
            string URL = "http://ws.audioscrobbler.com/2.0/?method=user.getrecenttracks&nowplaying=%22true%22&user="
                + lastFmUser + "&limit=1&api_key=censored";

            WebClient client = new WebClient();
            string htmlCode = client.DownloadString(URL);

            List<string> htmlCodeInLines = new List<string>();

            string line = "";

            foreach(char c in htmlCode)
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

            /*
            int i;
            for (i = 0; htmlCodeInLines[i].Contains("<artist"); i++)
            {

            }
             */

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

            artist = artist.Replace(@"<artist mbid="">", null).Replace("</artist>", null).Trim().Replace("&amp;","&");

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

            engine.WriteToIrc(lastFmMsg);
        }
    }
}
