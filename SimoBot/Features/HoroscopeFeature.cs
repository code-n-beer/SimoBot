using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;
using System.Xml;
using System.Net;
using System.Compat.Web;

namespace SimoBot
{
    class HoroscopeFeature : IFeature
    {
        private Dictionary<string, string> signs;
        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["horos"] = Execute;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
            signs = new Dictionary<string, string>();
            signs.Add("oinas", "aries");
            signs.Add("härkä", "taurus");
            signs.Add("kaksoset", "gemini");
            signs.Add("rapu", "cancer");
            signs.Add("leijona", "leo");
            signs.Add("neitsyt", "virgo");
            signs.Add("vaaka", "libra");
            signs.Add("skorpioni", "scorpion");
            signs.Add("jousimies", "sagittarius");
            signs.Add("kauris", "capricorn");
            signs.Add("vesimies", "aquarius");
            signs.Add("kalat", "pisces");
        }

        public void Execute(IrcClient Client, string channel, IrcUser Sender, string message)
        {
            message = message.Trim();
            if (message == "")
            {
                Client.LocalUser.SendMessage(channel, horoscopeHelpMsg());
            }
            else
            {
                if (!signs.ContainsKey(message))
                {
                    Client.LocalUser.SendMessage(channel, "Why won't you give me a sign~");
                    return;
                }
                XmlDocument xml = getXml(signs[message]);
                int dayOfWeek = getDayOfWeek();
                int magic1 = 1 + 2 * dayOfWeek;
                var days = xml.DocumentElement.GetElementsByTagName("p")[magic1];

                if (days == null || days.InnerText.Length < 20)
                {
                    Client.LocalUser.SendMessage(channel, "Misunderstood HTML");
                    return;
                }

                string horoscope = HttpUtility.HtmlDecode(days.InnerText);
                Client.LocalUser.SendMessage(channel, horoscope);
            }
        }

        private int getDayOfWeek()
        {

            return (int)DateTime.Today.DayOfWeek + 1;
        }

        private XmlDocument getXml(string sign)
        {
            string inputUrl = string.Format("http://www.astro.fi/future/weeklyForecast/sign/{0}", sign);
            var client = new WebClient();
            string content = client.DownloadString(inputUrl);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            return doc;
        }
        private string horoscopeHelpMsg()
        {
            return "Usage: horoskooppi SIGN Where SIGN is the desired horoscope sign in finnish";
        }
    }
}
