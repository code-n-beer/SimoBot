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
