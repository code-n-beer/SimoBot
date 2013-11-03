using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            ChannelConfigs confs = new ChannelConfigs
            {
                //Todo: Load ChannelConfigs
                channelConfigs = new Dictionary<string,Dictionary<string,string>>()
            };

            Engine engine = new Engine(confs);
            engine.LoadFeatures();
            engine.InitializeFeatures();


            //Todo: Load and initialize features
            //Todo: Connect
            //Todo: Begin "main loop"
        }

    }
}
