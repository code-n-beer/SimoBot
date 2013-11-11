using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot.Features
{
	class RandomFeature : IFeature
	{
        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["random"] = Execute;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
        }

        public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
        {
			string[] words = Message.Trim().Split(' ');
			if (words.Length < 2 || words.Length > 2)
			{
				Client.LocalUser.SendMessage(Channel, "!random <start> <end>");
				return;
			}

            int start, end;
			try
			{
				start = Convert.ToInt32(words[1]);
				end = Convert.ToInt32(words[2]);
			}
			catch (Exception)
			{
				Client.LocalUser.SendMessage(Channel, "Numbers please");
				return;
			}

			int random = new Random().Next(start, end + 1);

			Client.LocalUser.SendMessage(Channel, random.ToString());
        }
	}
}
