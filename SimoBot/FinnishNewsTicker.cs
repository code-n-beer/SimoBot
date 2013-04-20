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
		WebClient client;
		HtmlDocument htmldoc;
		string regexString = @"[A-ZÄÖÅ]{1}[^.?!]{3,100}\sja\s[^A-ZÄÖÅ]{1}[^.?!]{3,100}[.?!]";
		//string regexString = @"[A-ZÖÄÅ][^.]{0,175}\sja\s[^.]{0,175}[.]";
		//                     [A-ZÖÄÅ][^.]{0,100}\sja\s[^.A-ZÖÄÅ]{0,100}[.]
		//

		public FinnishNewsTicker(string allowedWebsites)
		{
			this.allowedWebsites = prepareAllowedWebsites(allowedWebsites);
			this.newsWebsite = "http://www.ampparit.com/haku?q=ja&t=news";
			usedTickers = new List<string>();
			usedTickers.Add("olenrikki");
			client = new WebClient();
			htmldoc = new HtmlDocument();
			htmldoc.OptionFixNestedTags = true;
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

				string encoding = "ISO-8859-1";

				if (currentSite == "Ilta-Sanomat" || currentSite == "Mobiili.fi" || currentSite == "Turun Sanomat")
				{
					encoding = "UTF-8";
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

			return "Couldn't find new articles";
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
					return IltalehtiGet();
				case "Ilta-Sanomat":
					return IltaSanomatGet();
				case "Mobiili.fi":
					return MobiilifiGet();
				case "Turun Sanomat":
					return TurunSanomatGet();
				default:
					return "Tried to get text from unsupported website??";
			}

			return "Something went wrong (tried to get text from " + currentSite;
		}

		private string TurunSanomatGet()
		{
			HtmlNode textNode = htmldoc.DocumentNode.SelectSingleNode("//div[@class='text']");
			string articleText = textNode.InnerText;

			Match match = Regex.Match(articleText, regexString);

			if (!match.Success)
			{
				return "olenrikki";
			}

			string text = match.Captures[0].ToString();

			return text;
		}


		private string MobiilifiGet()
		{
			HtmlNode textNode = htmldoc.DocumentNode.SelectSingleNode("//div[@class='sharecontainer']");
			string articleText = textNode.InnerText;

			Match match = Regex.Match(articleText, regexString);

			if (!match.Success)
			{
				return "olenrikki";
			}

			string text = match.Captures[0].ToString();

			return text;
		}

		private string IltalehtiGet()
		{
			HtmlNode textNode = htmldoc.DocumentNode.SelectSingleNode("//div[@class='keski']");
			string articleText = textNode.InnerText;

			Match match = Regex.Match(articleText, regexString);

			if (!match.Success)
			{
				return "olenrikki";
			}

			string text = match.Captures[0].ToString();

			return text;
		}


		private string IltaSanomatGet()
		{
			HtmlNode textNode = htmldoc.DocumentNode.SelectSingleNode("//div[@id='article-text']");
			string articleText = textNode.InnerText;

			Match match = Regex.Match(articleText, regexString);

			if (!match.Success)
			{
				return "olenrikki";
			}

			string text = match.Captures[0].ToString();

			return text;
		}

		private string automagicFunnytizer(string text, string curSite)
		{

			if (text.Contains('\n'))
			{
				//int pos = text.IndexOf("\n");
				text = text.Replace("\n", " ");
				//text = text.Trim().Substring(0, pos);
			}


			int jaPos = text.IndexOf(" ja ");

			string finalString = text.Substring(0, jaPos) + " :D" + text.Substring(jaPos) + " :D";
			finalString = finalString.Replace(".", "");

			finalString = finalString.Replace("  ", " ");

			finalString += " t. " + curSite;

			return finalString;
		}

		private bool isFromAllowedSource(string source, out string site)
		{
			for (int i = 0; i < allowedWebsites.Count; i++)
			{
				if (source.ToLower().Contains("jääkiekko")) //Add other forbidden tags here
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
