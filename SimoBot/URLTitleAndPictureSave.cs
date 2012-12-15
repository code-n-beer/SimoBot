using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace SimoBot
{
    class URLTitleAndPictureSave
    {
        private string localPicPath, remotePicPath;
        private string URL;
        Message message;
        private string connectionString;
        string title = "";
        System.Diagnostics.Process wgetProcess;

        public URLTitleAndPictureSave(string localPicPath, string remotePicPath, string connectionString)
        {
            this.connectionString = connectionString;
            this.localPicPath = localPicPath;
            this.remotePicPath = remotePicPath;
            wgetProcess = new System.Diagnostics.Process();
            //wgetProcess.Start();
        }
        private bool fileNameAlreadyExists(string filename)
        {
            string[] filePaths = Directory.GetFiles(localPicPath);
            List<string> list = new List<string>(filePaths);
            if (list.Contains(localPicPath + filename))
            {
                return true;
            }
            else
            {
                return false;
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

        private Regex RgxUrl = new Regex("(((https|http|ftp):\\/\\/)|www\\.)(([0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+)|localhost|([a-zA-Z0-9\\-]+\\.)*[a-zA-Z0-9\\-]+\\.(com|net|org|info|biz|gov|name|edu|[a-zA-Z][a-zA-Z]))(:[0-9]+)?((\\/|\\?)[^ \"]*[^ ,;\\.:\">)])?");

        private string parseURL(string input)
        {
            string URL = "";

            string[] outputArray = input.Split(null);

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

            //if (URL.ToLower().Contains("https://"))
            //{
            //    URL = URL.Replace("https://", "http://");
            //}

            return URL;
        }

        public string getURLTitle(Message msg)
        {
            this.message = msg;
            string input = msg.message;
            URL = parseURL(input);

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
                        picName = letters[i] + picName;
                    }
                }
                string picSaveName = picName;
                int j = 1;
                while (fileNameAlreadyExists(picSaveName))
                {
                    return "";
                }

                wgetProcess = System.Diagnostics.Process.Start("wget", "--no-check-certificate -O " + localPicPath + picSaveName + " " + URL);
                Console.WriteLine("Nope, didn't crash there.");
                try
                {
                    addToLinkMysqlList(picSaveName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("penis :(");
                }
                return "";
            }
            else
            {
                if (URL.ToLower().Contains("https://"))
                    URL = URL.Replace("https://", "http://");

                if (URL.Contains("["))
                    return "";

                try
                {
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
                                break;//i = htmlCodeSplitBiggerThan.Length;
                            }
                        }
                        title = title.Replace("</title", null).Trim().Replace("&ouml;", "ö").Replace("&auml;", "ä").
                            Replace("&#45;", "-").Replace("&amp;", "&").Replace("&#39;", "'").Replace("&#8211;", "-").
                            Replace("&#x202a;", "").Replace("&#x202c;", "").Replace("&rlm;", "").Replace("&#x202b;", "").
                            Replace("&ndash;", "").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", @"'");
                    }

                    else
                    {
                        title = "";
                    }

                    if (title.Contains('\n'))        // You cannot say multiline stuff to irc so you have to split them to multiple messages
                    {
                        string[] titleArray = title.Split('\n');
                        string returnstring = "";
                        for (int i = 0; i < titleArray.Length && i < 4; i++)
                        {
                            returnstring += titleArray[i].ToString().Trim() + " - ";
                        }
                        return returnstring;

                    }
                    else if (title == "")
                    {
                        //Don't spam if there's no title. For now, bwahahahha.
                    }
                    else
                    {
                        return title;
                    }

                    addToLinkMysqlList();
                    return title;
                }
                catch (Exception e)
                {
                    return "fail";//e.Message;
                }
            }
        }

        private string getCommandString()
        {
            string MsgReadyForInterwebz = "INSERT INTO links VALUES ('" +
                    message.time + "','" + message.nick.Replace(@"\", "slash") +
                        "','" + URL + "','" + title + "');";

            return MsgReadyForInterwebz;
        }

        private string getCommandString(string picName)
        {
            string MsgReadyForInterwebz = "INSERT INTO links VALUES ('" +
        message.time + "','" + message.nick.Replace(@"\", "slash") +
            "','" + remotePicPath + picName + "','" + title + "');";

            return MsgReadyForInterwebz;
        }

        private void addToLinkMysqlList()
        {
            string[] msg = message.messageAsArray;

            string ConnectionString = connectionString;
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            MySqlCommand cmd = conn.CreateCommand();
            MySqlDataReader Reader;
            string CommandString = getCommandString();
            cmd.CommandText = CommandString;
            try
            {
                conn.Open();
                Reader = cmd.ExecuteReader();

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
            //string[] msg = message.messageAsArray;

            string ConnectionString = connectionString;
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            MySqlCommand cmd = conn.CreateCommand();
            MySqlDataReader Reader;
            string CommandString = getCommandString(picName);
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
    }
}
