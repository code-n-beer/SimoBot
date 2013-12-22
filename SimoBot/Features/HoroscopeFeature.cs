using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;
using System.Net;
using System.Text.RegularExpressions;

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

                string html = getHTML(signs[message]);

                string horoscope = dropHtmlPrecedingHoroscope(html, message);
                horoscope = dropHtmlTailingHoroscope(horoscope);
                horoscope = html2Txt(horoscope);

                Client.LocalUser.SendMessage(channel, horoscope);
            }
        }

        private string html2Txt(string horoscope)
        {
            horoscope = horoscope.Replace("&auml;", "ä");
            horoscope = horoscope.Replace("&ouml;", "ö");
            return horoscope;
        }

        private string dropHtmlPrecedingHoroscope(string html, string sign)
        {
            // disregard the first letter (too confusing trying to make it uppercase)
            sign = sign.Remove(0, 1);
            int tagCount = 2 * getDayOfWeek();

            if (tagCount > 14)
                return "Invalid date";

            Regex pTagRegex = new Regex(@"("+sign+"</h3><p>)");
            string[] htmlArray = pTagRegex.Split(html, tagCount + 1);
            string result = htmlArray[tagCount];

            if (result == null || result.Length < 20)
            {
                return "Failed to parse HTML (horoscope too short)";
            }

            return result;

        }

        private string dropHtmlTailingHoroscope(string html)
        {
            Regex pTagRegex2 = new Regex(@"(<br />)");
            string[] horoscopefinal = pTagRegex2.Split(html, 0);
            string result = horoscopefinal[0];

            if (result == null || result.Length > 500)
            {
                return "Failed to parse HTML (horoscope too long)";
            }

            return result;
        }

        private int getDayOfWeek()
        {
            return (DateTime.Today.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int)DateTime.Today.DayOfWeek;
        }

        private string getHTML(string sign)
        {
            string inputUrl = string.Format("http://www.astro.fi/future/weeklyForecast/sign/{0}", sign);
            var client = new WebClient();
            string content = client.DownloadString(inputUrl);
            return content;
        }
        private string horoscopeHelpMsg()
        {
            return "Usage: horos SIGN Where SIGN is the desired horoscope sign in Finnish (eg. neitsyt)";
        }
    }
}
