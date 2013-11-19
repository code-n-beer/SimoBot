using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Net;

namespace SimoBot.Features
{
    class WikipediaFeature : IFeature
    {
        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["wiki"] = Execute;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
        }

        public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
        {
            string[] messageAsArray = Message.Trim().Split(' ');
            if (messageAsArray.Length >= 2)
            {
                if (messageAsArray[1].ToLower() == "en")
                {
                    Say(Channel, ReadRandomEntry("en"), Client);
                    return;
                }
                else if (messageAsArray[1].ToLower() == "fi")
                {
                    Say(Channel, ReadRandomEntry("fi"), Client);
                    return;
                }
                string wikiEntry = ReadWikiEntry(Message);

                Say(Channel, wikiEntry, Client);
            }
        }

        private static void Say(string channel, string msg, IrcDotNet.IrcClient client)
        {
            client.LocalUser.SendMessage(channel, msg);
        }

        private static string ReadWikiEntry(string Entry, string language = "fi")
        {
            string ret = "";
            string placeholder = Entry;
            Entry = UrlEncode(Entry, Encoding.Default);
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(@"http://" + language +
                    ".wikipedia.org/w/api.php?format=xml&action=query&titles=" + Entry + @"&prop=extracts&exsentences=2&explaintext");
                request.UserAgent = "SimoBot/3.0 (tsarpf@gmail.com)";

                var response = request.GetResponse();

                var reader = XmlTextReader.Create(response.GetResponseStream());

                reader.ReadToFollowing("extract");

                ret = reader.ReadElementContentAsString();
            }
            catch (Exception ex)
            {
                ret = ex.Message;
            }

            if (ret.ToLower().StartsWith("redirect"))
            {
                return ReadWikiEntry(ret.Remove(0, "REDIRECT ".Length));
            }
            else if (ret.Contains("'None' is an element node.") && language == "fi")
            {
                return ReadWikiEntry(placeholder, "en");
            }
            else if (ret.ToLower().StartsWith("ohjaus"))
            {
                return ReadWikiEntry(ret.Remove(0, "ohjaus ".Length));
            }

            return ret;
        }

        private static string ReadRandomEntry(string language = "fi")
        {
            string ret = "";

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(@"http://" + language +
                        ".wikipedia.org/w/api.php?format=xml&action=query&generator=random&prop=extracts&exsentences=2&explaintext&grnnamespace=0");
                request.UserAgent = "SimoBot/3.0 (tsarpf@gmail.com)";

                var response = request.GetResponse();

                var reader = XmlTextReader.Create(response.GetResponseStream());

                reader.ReadToFollowing("extract");

                ret = reader.ReadElementContentAsString();
            }
            catch (Exception e)
            {
                ret = e.Message;
            }

            return ret;
        }

        private static string UrlEncode(string s, Encoding e)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte i in e.GetBytes(s))
            {
                if ((i >= 'A' && i <= 'Z') ||
                                (i >= 'a' && i <= 'z') ||
                                (i >= '0' && i <= '9') ||
                                i == '-' || i == '_')
                {
                    sb.Append((char)i);
                }
                else if (i == ' ')
                {
                    sb.Append('+');
                }
                else
                {
                    sb.Append('%');
                    sb.Append(i.ToString("X2"));
                }
            }

            return sb.ToString();
        } 
    }
}



/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Web;
//using StringBuilder;
//using System.Web.Services;

namespace SimoBot
{
    static class Wikipedia
    {
        public static string ReadWikiEntry(string Entry, string language = "fi")
        {
            string ret = "";
            string placeholder = Entry;
            Entry = UrlEncode(Entry, Encoding.Default);
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(@"http://" + language +
                    ".wikipedia.org/w/api.php?format=xml&action=query&titles=" + Entry + @"&prop=extracts&exsentences=2&explaintext");
                request.UserAgent = "SimoBot/3.0 (tsarpf@gmail.com)";

                var response = request.GetResponse();

                var reader = XmlTextReader.Create(response.GetResponseStream());

                reader.ReadToFollowing("extract");

                ret = reader.ReadElementContentAsString();
            }
            catch (Exception ex)
            {
                ret = ex.Message;
            }

            if (ret.ToLower().StartsWith("redirect"))
            {
                return ReadWikiEntry(ret.Remove(0, "REDIRECT ".Length));
            }
            else if (ret.Contains("'None' is an element node.") && language == "fi")
            {
                return ReadWikiEntry(placeholder, "en");
            }
            else if (ret.ToLower().StartsWith("ohjaus"))
            {
                return ReadWikiEntry(ret.Remove(0, "ohjaus ".Length));
            }

            return ret;
        }

                public static string ReadRandomEntry(string language = "fi")
                {
                        string ret = "";

                        try
                        {
                                var request = (HttpWebRequest)WebRequest.Create(@"http://" + language +
                                        ".wikipedia.org/w/api.php?format=xml&action=query&generator=random&prop=extracts&exsentences=2&explaintext&grnnamespace=0");
                                request.UserAgent = "SimoBot/3.0 (tsarpf@gmail.com)";

                                var response = request.GetResponse();

                                var reader = XmlTextReader.Create(response.GetResponseStream());

                                reader.ReadToFollowing("extract");

                                ret = reader.ReadElementContentAsString();
                        }
                        catch (Exception e)
                        {
                                ret = e.Message;
                        }

                        return ret;
                }
                //
                //
                //http://en.wikipedia.org/w/api.php?action=query&generator=random&prop=extracts&exsentences=2&explaintext&grnnamespace=0
        public static string UrlEncode(string s, Encoding e)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte i in e.GetBytes(s))
            {
                if ((i >= 'A' && i <= 'Z') ||
                                (i >= 'a' && i <= 'z') ||
                                (i >= '0' && i <= '9') ||
                                i == '-' || i == '_')
                {
                    sb.Append((char)i);
                }
                else if (i == ' ')
                {
                    sb.Append('+');
                }
                else
                {
                    sb.Append('%');
                    sb.Append(i.ToString("X2"));
                }
            }

            return sb.ToString();
        } 
    }
}
*/
