using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;


namespace SimoBot.Features
{
	class NowPlayingFeature
	{
		public void RegisterFeature(EngineMessageHandlers features)
		{
			features.commands["np"] = Execute;
			features.commands["setlastfm"] = ExecuteSet;
		}


		Dictionary<string, Dictionary<string, string>> configs;
		string APIKey;
		string filename;
		Dictionary<string, string> nickDictionary;

		public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
		{
			this.configs = configs;
			APIKey = ConfigLoader.FindValueFromNestedDictionary(configs, "lastfmapikey");
			filename = ConfigLoader.FindValueFromNestedDictionary(configs, "lastfmfile");
			try
			{
				nickDictionary = getLastFmNickDictionary(filename);
			}
			catch (FileNotFoundException)
			{
				nickDictionary = new Dictionary<string, string>();
			}
		}

		public void ExecuteSet(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
		{
			string stuff = runSetLastFm(Message, Sender.NickName);
			Client.LocalUser.SendMessage(Channel, stuff);
		}

		public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
		{
			string stuff = runNP(Message, Sender.NickName);
			Client.LocalUser.SendMessage(Channel, stuff);
		}

		private string runSetLastFm(string msg, string nick)
		{
			string[] messageAsArray = msg.Split(' ');

			if (messageAsArray.Length < 1)
			{
				return "format: !setlastfm examplenick";
			}

			string lastfmNick = messageAsArray[0];

			if (nickDictionary.Keys.Contains(nick))
			{
				UpdateLastFmNick(nick, lastfmNick);
			}
			else
			{
				AddNewLastFMNick(nick, lastfmNick);
			}

			return "You can now say !np to get what '" + lastfmNick + "' is playing";
		}

		public string runNP(string msg, string senderNick)
		{
			string[] messageAsArray = msg.Split(' ');
			try
			{

				if (messageAsArray.Length >= 2)
				{
					string nick = messageAsArray[0];
					return getNP(nick);
				}
				else
				{
					if (nickDictionary.Keys.Contains(senderNick))
					{
						return getNP(nickDictionary[senderNick]);
					}
					else
					{
						return "Your nick wasn't found from the saved Last.Fm nicklist. Try !setlastfm 'YourLastFmNick' (without quotes)";
					}
				}
			}
			catch (Exception e)
			{
				return "Olen rikki: " + e.Message;
			}
		}

		private string getNP(string lastFmUser)
		{
			string URL = "http://ws.audioscrobbler.com/2.0/?method=user.getrecenttracks&nowplaying=%22true%22&user="
			   + lastFmUser + "&limit=1&api_key=" + APIKey;


			string htmlCode = getUrlHtmlContent(URL);
			if (htmlCode == "")
				return "";


			List<string> htmlCodeInLines = new List<string>(htmlCode.Split('\n'));

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

			artist = artist.Replace(@"<artist mbid="">", null).Replace("</artist>", null).Trim().Replace("&amp;", "&");

			string trackStr = htmlCodeInLines[5].ToString();
			string track = trackStr.Replace("<name>", null).Replace("</name>", null).Replace("&amp;", "&").Trim();

			string lastFmMsg;

			if (htmlCodeInLines[3].Contains(@"nowplaying=""true"""))
			{
				lastFmMsg = lastFmUser + " playing: <" + artist + "> - <" + track + ">";
			}
			else
			{
				lastFmMsg = lastFmUser + " played: <" + artist + "> - <" + track + ">";
			}

			//Get three most used tags for the song

			string tagURL = "http://ws.audioscrobbler.com/2.0/?method=track.gettoptags&artist="
				+ artist + "&track=" + track + "&api_key=" + APIKey;

			htmlCode = getUrlHtmlContent(tagURL);

			if (htmlCode == "")
				return lastFmMsg;

			List<string> htmlCodeLines = new List<string>(htmlCode.Split('\n'));

			if (htmlCodeLines.Count >= 14)
			{
				lastFmMsg += " (" + htmlCodeLines[4].Replace("<name>", "").Replace("</name>", "").Trim();
				lastFmMsg += ", " + htmlCodeLines[8].Replace("<name>", "").Replace("</name>", "").Trim();
				lastFmMsg += ", " + htmlCodeLines[12].Replace("<name>", "").Replace("</name>", "").Trim() + ")";
			}
			else if (htmlCodeLines.Count >= 10)
			{
				lastFmMsg += " (" + htmlCodeLines[4].Replace("<name>", "").Replace("</name>", "").Trim();
				lastFmMsg += ", " + htmlCodeLines[8].Replace("<name>", "").Replace("</name>", "").Trim() + ")";
			}
			else if (htmlCodeLines.Count >= 5 && htmlCodeLines[4].Contains("<name>"))
			{
				lastFmMsg += " (" + htmlCodeLines[4].Replace("<name>", "").Replace("</name>", "").Trim() + ")";
			}
			else
			{
				//Track had no tags, get artist tags instead
				string artistTagURL = "http://ws.audioscrobbler.com/2.0/?method=artist.gettoptags&artist="
								+ artist + "&api_key=" + APIKey;

				htmlCode = getUrlHtmlContent(artistTagURL);

				List<string> htmlCodeLinesArtistTopTags = new List<string>(htmlCode.Split('\n'));

				if (htmlCodeLinesArtistTopTags.Count >= 14)
				{
					lastFmMsg += " (" + htmlCodeLinesArtistTopTags[4].Replace("<name>", "").Replace("</name>", "").Trim();
					lastFmMsg += ", " + htmlCodeLinesArtistTopTags[8].Replace("<name>", "").Replace("</name>", "").Trim();
					lastFmMsg += ", " + htmlCodeLinesArtistTopTags[12].Replace("<name>", "").Replace("</name>", "").Trim() + ")";
				}
				else if (htmlCodeLinesArtistTopTags.Count >= 10)
				{
					lastFmMsg += " (" + htmlCodeLinesArtistTopTags[4].Replace("<name>", "").Replace("</name>", "").Trim();
					lastFmMsg += ", " + htmlCodeLinesArtistTopTags[8].Replace("<name>", "").Replace("</name>", "").Trim() + ")";
				}
				else if (htmlCodeLinesArtistTopTags.Count >= 5 && htmlCodeLinesArtistTopTags[4].Contains("<name>"))
				{
					lastFmMsg += " (" + htmlCodeLinesArtistTopTags[4].Replace("<name>", "").Replace("</name>", "").Trim() + ")";
				}
			}

			return lastFmMsg;

		}

		private string getUrlHtmlContent(string URL)
		{
			WebClient client = null;
			string htmlCode = "";
			try
			{
				client = new WebClient();
				htmlCode = client.DownloadString(URL);
			}

			catch (WebException)
			{
				Console.WriteLine("Empty html code received from: " + URL);
				return "";
			}

			return htmlCode;
		}

		private Dictionary<string, string> getLastFmNickDictionary(string filename = "LastFmNick.txt")
		{
			Dictionary<string, string> NickDir = new Dictionary<string, string>();
			string output = "asdf";
			StreamReader reader = new StreamReader(filename);

			string[] outputArray;
			string nickLast;
			string nickIRC;


			try
			{
				while (output != null)
				{
					output = reader.ReadLine();

					if (output == null) break;

					outputArray = output.Split(' ');

					nickIRC = outputArray[0].ToString();
					nickLast = outputArray[1].ToString();

					NickDir.Add(nickIRC, nickLast);
				}
			}
			catch (Exception)
			{ }

			reader.Close();

			return NickDir;
		}

		public void UpdateLastFmNick(string IRCnick, string lastFMNick)
		{
			nickDictionary[IRCnick] = lastFMNick;

			StreamWriter writer = new StreamWriter(filename, false); // append = false which means if the file exists, it will be overwritten.

			foreach (KeyValuePair<string, string> nicks in nickDictionary)
			{
				writer.WriteLine(nicks.Key + ' ' + nicks.Value);
			}

			writer.Close();
		}

		public void AddNewLastFMNick(string nickIRC, string lastFMNick)
		{
			StreamWriter LastFMNickTxtWriter = new StreamWriter(filename, true);
			LastFMNickTxtWriter.WriteLine(nickIRC + ' ' + lastFMNick);
			LastFMNickTxtWriter.Close();

			nickDictionary[nickIRC] = lastFMNick;
		}
	}
}