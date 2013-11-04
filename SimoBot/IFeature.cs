using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
    interface IFeature
    {
        public void RegisterFeature(EngineMessageHandlers features);
        public void Initialize(ChannelConfigs configs);
        public void Execute(Message msg);
    }
}
