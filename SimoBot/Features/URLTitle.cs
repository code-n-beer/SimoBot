using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;

namespace SimoBot.Features
{
    class URLTitle : IFeature
    {
        private Regex RgxUrl = new Regex("(((https|http|ftp):\\/\\/)|www\\.)(([0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+)|localhost|([a-zA-Z0-9\\-]+\\.)*[a-zA-Z0-9\\-]+\\.(com|net|org|info|biz|gov|name|edu|[a-zA-Z][a-zA-Z]))(:[0-9]+)?((\\/|\\?)[^ \"]*[^ ,;\\.:\">)])?");
        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.regexes[RgxUrl] = Execute;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
        }

        public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
        {
            Client.LocalUser.SendMessage(Channel, getURLTitle(Message));
        }

        private bool UrlWasToPicture(string url)
        {
            url = url.ToLower();
            if (url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") ||
                url.EndsWith(".gif") || url.EndsWith(".bmp"))
            {
                return true;
            }

            return false;
        }


        private string getURLTitle(string msg)
        {
            string input = msg;
            string URL = parseURL(input);
            string title = "";


            if (URL.Contains("https://"))
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
                            Replace("&ndash;", "").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", @"'").
                            Replace("&#039;", "'").Replace("&raquo;", "»").Replace("&#8217;", "´");
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
                }
                else
                {
                    return title;
                }

                return title;
            }
            catch (Exception)
            {
				return "";
            }
        }

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
    }
}
