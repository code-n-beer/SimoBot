using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
    interface IFeature
    {
        void RegisterFeature(EngineMessageHandlers features);
        void Initialize(ChannelConfigs configs);
        void Execute(
            IrcDotNet.IrcClient Client,
            string Channel,
            IrcDotNet.IrcUser Sender,
            string Message);
    }
}
