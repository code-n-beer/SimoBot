using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
    
    using IrcDotNet;
    
    class TimerFeature : IFeature
    {
        class SimoTimer
        {
            int _Delay;
            string _Message;
            
            public SimoTimer(IrcClient Client, string Channel, int Delay, string Message)
            {
                _Delay = Delay;
                _Message = Message;
                Client.LocalUser.SendMessage(Channel, TimerMessage());
            }
            
            private string TimerMessage()
            {
                return String.Format("Timer set {0} {1}", _Delay, _Message);
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
            new SimoTimer(Client, Channel, int.Parse(splits[0]), splits[1]);
        }
    }
}
