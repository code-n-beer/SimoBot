using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace SimoBot
{
	class FinnishNewsTicker
	{
		List<string> allowedWebsites;
		string newsWebsite;
		List<string> usedTickers;
		List<string> usedSites;
		WebClient client;
		HtmlDocument htmldoc;
		string regexString = @"[A-ZÄÖÅ]{1}[^.?!]{3,100}\sja\s[^A-ZÄÖÅ]{1}[^.?!]{3,100}[.?!]";
		//string regexString = @"[A-ZÖÄÅ][^.]{0,175}\sja\s[^.]{0,175}[.]";
		//                     [A-ZÖÄÅ][^.]{0,100}\sja\s[^.A-ZÖÄÅ]{0,100}[.]

		MarkovChainTest.MarkovChainRedis MCR;

		public FinnishNewsTicker(string allowedWebsites, MarkovChainTest.MarkovChainRedis MCR)
		{
			this.allowedWebsites = prepareAllowedWebsites(allowedWebsites);
			this.newsWebsite = "http://www.ampparit.com/haku?q=ja&t=news";
			usedTickers = new List<string>();
			usedTickers.Add("olenrikki");
			client = new WebClient();
			htmldoc = new HtmlDocument();
			htmldoc.OptionFixNestedTags = true;
			usedSites = new List<string>();
			this.MCR = MCR;
		}

		public void resetTickers()
		{
			usedSites = new List<string>();
			usedTickers = new List<string>();

            //Not sure why this was done in the constructor, but I don't want to spend
			//any time in finding out if not doing it would break something. 
			usedTickers.Add("olenrikki"); 
		}

		private List<string> prepareAllowedWebsites(string sitesInAString)
		{
			return new List<string>(sitesInAString.Split('|'));
		}

		private bool loadHTML(string url, string encoding = "ISO-8859-1")
		{
			client = new WebClient();

			htmldoc = new HtmlDocument();

			client.Encoding = Encoding.GetEncoding(encoding);

			htmldoc.Load(client.OpenRead(url), Encoding.GetEncoding(encoding));

			if (htmldoc.ParseErrors != null && htmldoc.ParseErrors.Count() > 0)
			{
			}

			if (htmldoc.DocumentNode == null)
			{
				return false;
			}

			return true;
		}

		private bool isAlreadyUsed(string URL)
		{
			for (int i = 0; i < usedSites.Count; i++)
			{
				if (usedSites[i] == URL)
					return true;
			}

			usedSites.Add(URL);
			return false;
		}

		public string getNewTick()
		{

			if (!loadHTML(newsWebsite))
				return "Couldn't load HTML from news site. Website down?";

			string articleURL = "";
			foreach (HtmlNode link in htmldoc.DocumentNode.SelectNodes("//div[@class='news-item-info']"))
			{
				//Continue until found article from allowed website
				string currentSite = "";
				if (!isFromAllowedSource(link.InnerText, out currentSite))
				{
					continue;
				}

				//At ampparit the second sibling is the actual link to the article
				HtmlNode thelink = link.NextSibling.NextSibling;

				articleURL = thelink.GetAttributeValue("href", "fail");

				if (articleURL == "fail")
				{
					continue;
				}

				if (isAlreadyUsed(articleURL))
				{
					continue;
				}

				string encoding = "UTF-8";
				if (currentSite == "Iltalehti")
				{
					encoding = "ISO-8859-1";
				}

				if (!loadHTML(articleURL, encoding))
				{
					Console.WriteLine("Couldn't load HTML from " + currentSite);
					continue;
				}


				string sentence = getArticleText(currentSite);

				if (alreadyUsedOrBrokenTick(sentence))
					continue;

				usedTickers.Add(sentence);

				sentence = automagicFunnytizer(sentence, currentSite);

				///Console.WriteLine(sentence);

				return sentence;
			}

			return "Couldn't find new articles :D";
		}

		private bool alreadyUsedOrBrokenTick(string tick)
		{
			if (tick.Contains("olenrikki") || tick.ToLower().Contains("tried to get"))
			{
				return true;
			}

			for (int i = 0; i < usedTickers.Count; i++)
			{
				if (usedTickers[i] == tick)
					return true;
			}
			return false;
		}

		private string getArticleText(string currentSite)
		{
			switch (currentSite)
			{
				case "Iltalehti":
					return getSentenceFromSite("//div[@id='container_keski']", "Iltalehti");
				case "Ilta-Sanomat":
					return getSentenceFromSite("//div[@id='article-text']");
				case "Mobiili.fi":
					return getSentenceFromSite("//div[@class='sharecontainer']");
				case "Turun Sanomat":
					return getSentenceFromSite("//div[@class='text']");
				case "Yle":
					return getSentenceFromSite("//div[@class='text']");
				case "Stara":
					return getSentenceFromSite("//div[@class='text']");
				default:
					return "Tried to get text from unsupported website? " + currentSite;
			}

			return "Something went wrong (tried to get text from " + currentSite;
		}

		private static void RemoveComments(HtmlNode node)
		{
			foreach (var n in node.ChildNodes.ToArray())
				RemoveComments(n);
			if (node.NodeType == HtmlNodeType.Comment)
				node.Remove();
		}

		private string getSentenceFromSite(string mainTextLocationXpath = "//div[@class='text']", string site = "")
		{
			HtmlNode textNode = htmldoc.DocumentNode.SelectSingleNode(mainTextLocationXpath);

			RemoveComments(textNode);

			string articleText = textNode.InnerText;

			articleText = articleText.Trim().Replace("\n", " ").Replace("  ", " ");


			if (site == "Iltalehti")
			{
				Match stopAtFunction = Regex.Match(articleText, "^(?:(?!\\r).)*");
				//Match stopAtFunction = Regex.Match(articleText, "^(?:(?!function).)*");
				articleText = stopAtFunction.Captures[0].ToString();
			}

			articleText = fixText(articleText);

			addToMarkov(articleText);


			Match match = Regex.Match(articleText, regexString);

			if (!match.Success)
			{
				return "olenrikki";
			}

			string text = match.Captures[0].ToString();

			return text;
		}

		private string fixText(string articleText)
		{
			string text = articleText;

			for (int i = 0; i < text.Length; i++)
			{
                char c = text[i];

				if (i < text.Length - 1 && // check that we're not at the last character
					c == '.' && 
					text[i + 1] != '.' && 
					text[i + 1] != ' ')
				{
					text = text.Substring(0, i) + " " + text.Substring(i + 1);
				}

                int charCount = 0;
				if (c == '&')
				{
					for (int j = i; j < text.Length; j++)
					{
						charCount++;
						if (text[j] == ' ')
							break;

						if (text[j] == ';')
						{
							text = text.Remove(i, charCount);
							break;
						} 
					}
				}

				if (c == '#')
				{
					for (int j = i; j < text.Length; j++)
					{
						charCount++;
						if (text[j] == ' ')
							break;

						if (text[j] == ';')
						{
							text = text.Remove(i, charCount);
							break;
						}
					}
				}


			}



			return text;
		}

		private void addToMarkov(string articleText)
		{
			MCR.addNewLineToRedis(articleText);
		}

		private string automagicFunnytizer(string text, string curSite)
		{

			if (text.Contains('\n'))
			{
				text = text.Replace("\n", " ");
			}

			int jaPos = text.LastIndexOf(" ja ");

			string finalString = text.Substring(0, jaPos) + " :D" + text.Substring(jaPos) + " :D";
			finalString = finalString.Replace(".", "");

			finalString = finalString.Replace("  ", " ");

			finalString += " t. " + curSite;

			return finalString;
		}

		private bool isFromAllowedSource(string source, out string site)
		{
			Console.WriteLine(source);
			for (int i = 0; i < allowedWebsites.Count; i++)
			{
				if (source.ToLower().Contains("jääkiekko") || source.ToLower().Contains("kiekko") ||
					source.ToLower().Contains("f1") || source.ToLower().Contains("naiset") ||
					source.ToLower().Contains("sm-liiga") || source.ToLower().Contains("liikenne")) //Add other forbidden tags here
				{

					site = "";
					return false;
				}
				if (source.Contains(allowedWebsites[i]))
				{
					site = allowedWebsites[i];
					return true;
				}
			}
			site = "";
			return false;
		}

	}
}
