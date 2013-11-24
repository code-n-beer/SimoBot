using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SimoBot
{
    
    using IrcDotNet;
    
    class TimerFeature : IFeature
    {
        class SimoTimer
        {
            int _Delay;
            string _Message;
            IrcClient _Client;
            string _Channel;
            string _Nick;
            System.Timers.Timer aTimer;
            
            public SimoTimer(IrcClient Client, string Channel, string Nick, int Delay, string Message)
            {
                _Client = Client;
                _Channel = Channel;
                _Nick = Nick;
                _Delay = Delay;
                _Message = Message;
                aTimer = new System.Timers.Timer(_Delay);
                aTimer.Elapsed += new ElapsedEventHandler(TimerFiring);
                aTimer.Enabled = true;
                IrcSay(TimerMessage());
            }
            
            private string TimerMessage()
            {
                return String.Format("Timer set {0} {1}", _Delay, _Message);
            }
            
            private void TimerFiring(object source, ElapsedEventArgs e)
            {
                aTimer.Enabled = false;
                IrcSay(String.Format("{0}: {1}", _Nick, _Message));
            }
            
            private void IrcSay(string Message)
            {
                _Client.LocalUser.SendMessage(_Channel, Message);
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
            new SimoTimer(Client, Channel, Sender.NickName, int.Parse(splits[0]), splits[1]);
        }
    }
}
