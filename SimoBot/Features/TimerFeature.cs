using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Text.RegularExpressions;

namespace SimoBot.Features
{
    using IrcDotNet;
    
    class TimerFeature : IFeature
    {
        public delegate void Spam(string message);
        
        class SimoTimer
        {
            private Spam IrcSay;
            private string nick;
            private int delay;
            private string message;
            private System.Timers.Timer timer;
            
            public SimoTimer(Spam IrcSay, string nick, int delay, string message)
            {
                this.IrcSay = IrcSay;
                this.nick = nick;
                this.delay = delay;
                this.message = message;
                
                timer = new System.Timers.Timer(delay);
                timer.Elapsed += new ElapsedEventHandler(TimerFiring);
                timer.Start();
                IrcSay(TimerMessage());
            }
            private string TimerMessage()
            {
                return String.Format("Timer set {0} {1} by {2}", TimeInString(), message, nick);
            }
            private void TimerFiring(object source, ElapsedEventArgs e)
            {
                timer.Stop();
                IrcSay(String.Format("{0}: {1}, {2} ago", nick, message, TimeInString()));
            }
            private string TimeInString()
            {
                return TimeSpan.FromMilliseconds(delay).ToString();
            }
        }

        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["timer"] = Execute;
        }
        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
        }
        public void Execute(IrcClient Client, string Channel, IrcUser Sender, string Message)
        {
            string[] splits = Message.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            new SimoTimer(delegate(string m) {Client.LocalUser.SendMessage(Channel, m);},
                Sender.NickName, TimerIntParser(splits[0]), splits[1]);
        }
        private int TimerIntParser(string DelayAsString)
        {
            if (Regex.IsMatch(DelayAsString, @"^\d+m(ins?)?$"))
            {
                return NumberInUse(DelayAsString) * 1000 * 60;
            }
            if (Regex.IsMatch(DelayAsString, @"^\d+h(ours?)?$"))
            {
                return NumberInUse(DelayAsString) * 1000 * 60 * 60;
            }
            if (Regex.IsMatch(DelayAsString, @"^\d+d(ays?)?$"))
            {
                return NumberInUse(DelayAsString) * 1000 * 60 * 60 * 24;
            }
            if (Regex.IsMatch(DelayAsString, @"^\d+y(ears?)?$"))
            {
                return NumberInUse(DelayAsString) * 1000 * 60 * 60 * 24 * 365;
            }
            return int.Parse(DelayAsString);
        }
        private int NumberInUse(string s)
        {
            int.Parse(Regex.Match(s, @"\d+").Value);
        }
    }
}
