using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Xml;

namespace SimoBot.Features
{
	class OracleFeature : IFeature
	{

		public void RegisterFeature(EngineMessageHandlers features)
		{
			features.commands["oracle"] = Execute;
		}

		public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
		{
		}

		public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
		{


			string question = UrlEncode(Message, Encoding.Default);
            string url = "http://www.lintukoto.net/viihde/oraakkeli/index.php?kysymys_24171206=" + question + "&rnd=24171206";

			string html = getUrlHtmlContent(url);

			string sstring = "<p class='vastaus'>";
			int index = html.IndexOf(sstring);

			string restOfString = html.Substring(index + sstring.Length);
			restOfString = restOfString.Substring(0, restOfString.IndexOf("</p>"));
			string answer = restOfString;

			Client.LocalUser.SendMessage(Channel, answer);
		}

		private string getUrlHtmlContent(string URL)
		{
			WebClient client = null;
			string htmlCode = "";
			try
			{
				client = new WebClient();
				client.Encoding = System.Text.Encoding.GetEncoding(1252);
				htmlCode = client.DownloadString(URL);
			}
			catch (WebException)
			{
				Console.WriteLine("Empty html code received from: " + URL);
				return "";
			}

			return htmlCode;
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
