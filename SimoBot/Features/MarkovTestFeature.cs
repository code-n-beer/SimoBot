using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis;

namespace SimoBot
{
	class MarkovTestFeature : IFeature
	{

		EngineMessageHandlers features;
		public void RegisterFeature(EngineMessageHandlers features)
		{
			this.features = features;
			features.commands["markov"] = Execute;
		}

		public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
		{
		}

		public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
		{

		}
	}
}
