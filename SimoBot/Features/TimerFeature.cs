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

        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["timer"] = Execute;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
        }
        public void Execute(IrcClient Client, string Channel, IrcUser Sender, string Message)
        {
            string message = "gänikoodi ei valmis XD";
            Client.LocalUser.SendMessage(Channel, message);
        }
    }
}
