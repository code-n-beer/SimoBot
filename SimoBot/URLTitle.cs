using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Net;
using MySql.Data.MySqlClient;
using System.IO;

namespace SimoBot
{
    class URLTitle
    {
        BackgroundWorker bgwUrlTitle = new BackgroundWorker();

        string title = "";


        public Parser ParseMachine;

        public URLTitle(Parser pr)
        {
            ParseMachine = pr;

            bgwUrlTitle.DoWork += new DoWorkEventHandler(bgwUrlTitle_DoWork);
        }

        string output ="";

        public Engine engine;

        public void RunURLTitle(string op, Engine eng)
        {
            engine = eng;
            output = op;
            if (!bgwUrlTitle.IsBusy)
                bgwUrlTitle.RunWorkerAsync(); 
        }

        private void bgwUrlTitle_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgwUrlTitle = sender as BackgroundWorker;

            DoUrlTitle();
        }


        private bool fileNameAlreadyExists(string path, string filename)
        {
            string[] filePaths = Directory.GetFiles(path);
             List<string> list = new List<string>(filePaths);
             if (list.Contains(path+filename))
             {
                 return true;
             }
             else
             {
                 return false;
             }
        }


        private void DoUrlTitle()
        {
            string URL = ParseMachine.GetUrl(output);

            if (UrlWasToPicture(URL))
            {
                char[] letters = URL.ToCharArray();
                string picName = "";
                for (int i = letters.Length - 1; i >= 0; i--)
                {
                    if (letters[i] == '/')
                    {
                        break;
                    }
                    else
                    {
                        picName += letters[i];
                    }
                }
                picName = ParseMachine.ReverseMsg(picName);
                string picSaveName = picName;
                int j = 1;
                //string home = Environment.SpecialFolder.Personal.ToString();
                while (fileNameAlreadyExists("censored",picSaveName))
                {
                    picSaveName = "(" + j + ")" + picName;
                    j++;
                }
                // ~/sites/tsarpf.kapsi.fi/www/kuvei/
                //if (URL.ToLower().Contains("https://"))
                //{ 
                //    URL = URL.Replace("https://", "http://");
                //}

                System.Diagnostics.Process.Start("wget", "--no-check-certificate -O censored" + picSaveName + " " + URL);
                addToLinkMysqlList(picSaveName);
                return;
            }
            else
            {

                //if (URL.ToLower().Contains("https://"))
                //{
                //    URL = URL.Replace("https://", "http://");
                //}

                WebClient client = new WebClient();
                string htmlCode = client.DownloadString(URL);


                title = "";

                if (htmlCode.Contains("<title>"))
                {
                    string[] htmlCodeSplitBiggerThan = htmlCode.Split('>');

                    for (int i = 0; i < htmlCodeSplitBiggerThan.Length; i++)
                    {
                        if (htmlCodeSplitBiggerThan[i].Contains("</title"))
                        {
                            title = htmlCodeSplitBiggerThan[i].ToString();
                            i = htmlCodeSplitBiggerThan.Length; // use break; to get out of the for loop... anyone?
                        }
                    }
                    title = title.Replace("</title", null).Trim().Replace("&ouml;", "ö").Replace("&auml;", "ä").Replace("&#45;", "-").Replace("&amp;", "&").Replace("&#39;", "'").Replace("&#8211;", "-").Replace("&#x202a;", "").Replace("&#x202c;", "").Replace("&rlm;", "").Replace("&#x202b;", "").Replace("&ndash;", "").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", @"'");
                }

                else
                {
                    title = "";
                }

                if (title.Contains('\n'))        // You cannot say multiline stuff to irc so you have to split them to multiple messages
                {
                    string[] titleArray = title.Split('\n');

                    for (int i = 0; i < titleArray.Length && i < 4; i++)
                    {
                        engine.WriteToIrc(titleArray[i].ToString().Trim());
                    }


                }
                else if (title == "")
                {
                    //Don't spam if there's no title. For now, bwahahahha.
                }
                else
                {
                    engine.WriteToIrc(title);
                }

                addToLinkMysqlList();
            }
        }

        private string getCommandString(string[] ParsedMessageArray)
        {
            string MsgReadyForInterwebz = "INSERT INTO links VALUES ('" +
                    ParsedMessageArray[0].ToString() + "','" + ParsedMessageArray[1].ToString().Replace(@"\", "slash") +
                        "','" + ParseMachine.GetUrl(output) + "','" + title + "');";

            return MsgReadyForInterwebz;
        }

        private string getCommandString(string[] ParsedMessageArray, string picName)
        {
            string MsgReadyForInterwebz = "INSERT INTO links VALUES ('" +
        ParsedMessageArray[0].ToString() + "','" + ParsedMessageArray[1].ToString().Replace(@"\", "slash") +
            "','" + "censored" + picName  + "','" + title + "');";

            return MsgReadyForInterwebz;
        }

        private void addToLinkMysqlList()
        {
            string[] msg = ParseMachine.ParseMsg(output, output.Split(null));

            string ConnectionString = "SERVER=censored;" + "DATABASE=censored;" + "UID=censored;" + "PWD=censored;";
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            MySqlCommand cmd = conn.CreateCommand();
            MySqlDataReader Reader;
            string CommandString = getCommandString(msg);
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

        private void addToLinkMysqlList(string picName)
        {
            string[] msg = ParseMachine.ParseMsg(output, output.Split(null));

            string ConnectionString = "SERVER=censored;" + "DATABASE=censored;" + "UID=censored;" + "PWD=censored;";
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            MySqlCommand cmd = conn.CreateCommand();
            MySqlDataReader Reader;
            string CommandString = getCommandString(msg, picName);
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

        private bool UrlWasToPicture(string url)
        {
            url = url.ToLower();
            if (url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") ||
                url.EndsWith(".gif") || url.EndsWith(".bmp"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
