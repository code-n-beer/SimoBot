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
        class SimoTimer
        {
            private IrcClient Client;
            private string Channel;
            private string nick;
            
            private int delay;
            private string message;
            private System.Timers.Timer timer;
            
            public SimoTimer(IrcClient Client, string Channel, string nick, int delay, string message)
            {
                this.Client = Client;
                this.Channel = Channel;
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
            private void IrcSay(string message)
            {
                Client.LocalUser.SendMessage(Channel, message);
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
            new SimoTimer(Client, Channel, Sender.NickName, TimerIntParser(splits[0]), splits[1]);
        }
        private int TimerIntParser(string DelayAsString)
        {
            if (Regex.IsMatch(DelayAsString, @"^\d+m(in)?$"))
            {
                return int.Parse(Regex.Match(DelayAsString, @"\d+").Value) * 1000 * 60;
            }
            return int.Parse(DelayAsString);
        }
    }
}
