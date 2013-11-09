using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
    class HelpFeature
    {
        EngineMessageHandlers features;
        public void RegisterFeature(EngineMessageHandlers features)
        {
            this.features = features;
            features.commands["help"] = Execute;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
        }

        public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
        {
            string helpString = "";
            foreach (string command in features.commands.Keys)
            {
                helpString += command + " ";
            }

            helpString = "Available commands: " + helpString;

            Client.LocalUser.SendMessage(Channel, helpString);
        }
    }
}
