using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
    class ReverseFeature : IFeature
    {
        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["r"] = this.Execute;
        }

        public void Initialize(ChannelConfigs configs)
        {
            
        }

        public void Execute(IrcDotNet.IrcClient client, string channel, IrcDotNet.IrcUser sender, string message)
        {
            string reverse = "";
            char[] cArray = message.ToCharArray();
            Array.Reverse(cArray);
            reverse = new string(cArray);
            client.LocalUser.SendMessage(channel, reverse);
        }
    }
}
