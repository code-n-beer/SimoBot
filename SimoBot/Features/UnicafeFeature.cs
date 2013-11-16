using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot.Features
{
	using System;
	using SimoBot;
	using System.Xml;
	using System.Net;
	using System.Collections.Generic;
	using System.Linq;
	using IrcDotNet;

	namespace SimoBot.Features
	{
		class Meal
		{
			private string name;
			private string price;

			public Meal(string name, string price)
			{
				this.name = name;
				this.price = price;
			}

			public override string ToString()
			{
				return name + " - " + price;
			}
		}

		class Lunch
		{
			private string date;
			private List<Meal> meals;

			public Lunch(string date, List<Meal> meals)
			{
				this.date = date;
				this.meals = meals;
			}

			public string GetDate()
			{
				return this.date;
			}

			public List<Meal> GetMeals()
			{
				return this.meals;
			}
		}

		abstract class Restaurant
		{
			private string name;
			private int id;

			public Restaurant(string name, int id)
			{
				this.name = name;
				this.id = id;
			}

			public override string ToString()
			{
				List<Meal> meals = GetTodayLunch();
				if (meals == null)
				{
					return name + " has no lunch today";
				}
				string message = name + ": ";
				message += String.Join("; ", from x in meals select x.ToString());

				return message;
			}

			private List<Meal> GetTodayLunch()
			{
				List<Lunch> lunches = ParseData();
				string today = DateTime.Today.ToString("dd.MM.yyyy");
				foreach (Lunch lunch in lunches)
				{
					if (lunch.GetDate() == today)
						return lunch.GetMeals();
				}
				return null;
			}

			private List<Lunch> ParseData()
			{
				string inputUrl = string.Format("http://www.hyyravintolat.fi/rss/fin/{0}/", this.id);
				var client = new WebClient();
				var content = client.DownloadString(inputUrl);
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(content);
				return ParseLunches(doc);
			}

			private List<Lunch> ParseLunches(XmlDocument doc)
			{
				List<Lunch> lunches = new List<Lunch>();
				var days = doc.DocumentElement.GetElementsByTagName("item");
				foreach (XmlElement node in days)
				{
					var title = node.GetElementsByTagName("title")[0].InnerText;
					var dateString = title.Trim().Split(' ')[1];
					var description = node.GetElementsByTagName("description")[0];
					var meals = ParseMeals(description.InnerText);
					lunches.Add(new Lunch(dateString, meals));
				}
				return lunches;
			}

			private List<Meal> ParseMeals(string data)
			{
				List<Meal> meals = new List<Meal>();
				XmlDocument xml = new XmlDocument();
				xml.LoadXml("<lol>" + data.Replace("&euro;", "€") + "</lol>");
				var mealWrappers = xml.DocumentElement.GetElementsByTagName("p");
				foreach (XmlElement mealWrapper in mealWrappers)
				{
					var spans = mealWrapper.GetElementsByTagName("span");
					string name = "";
					string price = "";
					foreach (XmlElement span in spans)
					{
						if (span.GetAttribute("class") == "meal")
						{
							name = span.InnerText;
						}
						else if (span.GetAttribute("class") == "priceinfo")
						{
							price = span.InnerText;
						}
					}
					meals.Add(new Meal(name, price));
				}
				return meals;
			}
		}

		class Exactum : Restaurant
		{
			public Exactum()
				: base("Exactum", 11)
			{
			}

		}

		class Chemicum : Restaurant
		{
			public Chemicum()
				: base("Chemicum", 10)
			{
			}

		}

		public class UnicafeFeature : IFeature
		{
			public void Execute(IrcClient Client, string Channel, IrcUser Sender, string Message)
			{
				Restaurant exactum = new Exactum();
				Restaurant chemicum = new Chemicum();
				string message = "";
				Message = Message.Trim();
				if (Message == "exa" || Message == "exactum")
				{
					message = exactum.ToString();
				}
				else if (Message == "chem" || Message == "chemicum")
				{
					message = chemicum.ToString();
				}
				else if (Message == "")
				{
					message = exactum.ToString() + " ;; " + chemicum.ToString();
				}
				else
				{
					return;
				}
				Client.LocalUser.SendMessage(Channel, message);
			}

			public void RegisterFeature(EngineMessageHandlers features)
			{
                features.commands["uc"] = Execute;
				features.commands["unicafe"] = Execute;
			}

			public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
			{
			}
		}
	}
}
