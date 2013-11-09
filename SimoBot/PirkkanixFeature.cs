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
