using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimoBot
{
    interface IFeature
    {
        void RegisterFeature(EngineMessageHandlers features);
        void Initialize(Dictionary<string, Dictionary<string, string>> configs);
        void Execute(
            IrcDotNet.IrcClient Client,
            string Channel,
            IrcDotNet.IrcUser Sender,
            string Message);
    }
}
