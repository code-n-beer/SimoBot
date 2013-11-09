using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SimoBot
{
    class PirkkanixFeature : IFeature
    {

        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["niksi"] = Execute;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
        }

        public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
        {
            WebClient client = new WebClient();
            string nixi = client.DownloadString("http://thermopylas.fi/ws/nicksit.php");
            byte[] bytes = Encoding.Default.GetBytes(nixi);
            nixi = Encoding.UTF8.GetString(bytes);
            nixi = nixi.Replace('\n', ' ').Trim();
            Client.LocalUser.SendMessage(Channel, nixi);
        }
    }
}

/*
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
            // replace "r" with !<this part> of the command
            features.commands["r"] = Execute; // Name the 
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> confs)
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

*/

/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimoBot;
using IrcDotNet;
using System.Net;

namespace PirkkanixFeature
{
    class Pirkkanix : ICommand
    {
        public void Execute(IrcClient Client, string Channel, IrcUser Sender, string Message)
        {
            WebClient client = new WebClient();
            string nixi = client.DownloadString("http://thermopylas.fi/ws/nicksit.php");
            byte[] bytes = Encoding.Default.GetBytes(nixi);
            nixi = Encoding.UTF8.GetString(bytes);
            nixi = nixi.Replace('\n',' ').Trim();
                /*
                 * byte[] bytes = Encoding.Default.GetBytes(myString);
myString = Encoding.UTF8.GetString(bytes);
                 
            Client.LocalUser.SendMessage(Channel, nixi);
        }
    }

    public class PirkkanixFeature : IFeature
    {
        public void Init(ISimo Simo)
        {
            Simo.RegisterCommand("niksi", new Pirkkanix());
        }
    }
}
*/