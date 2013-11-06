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
            ChannelConfigs confs = new ChannelConfigs();
            confs.channelConfigs = ConfigLoader.LoadConfig("config.txt");
			/*
            ChannelConfigs confs = new ChannelConfigs
            {
                //Todo: Load ChannelConfigs
				//derp herp
                channelConfigs = new Dictionary<string,Dictionary<string,string>>()
            };

            string placeholderchannel = "#testingk";
            
            confs.channelConfigs[placeholderchannel] = new Dictionary<string, string>();

            var kikkeli = new Dictionary<string, string>();

            kikkeli["nickname"] = "SimoBot";
            kikkeli["password"] = "kekkels";
            kikkeli["realname"] = "Simo Bot";
            kikkeli["username"] = "simobot";
            kikkeli["server"] = "openirc.snt.utwente.nl";
            kikkeli["channels"] = placeholderchannel;

            confs.channelConfigs[placeholderchannel] = kikkeli;
            */

            Engine engine = new Engine(confs);
            engine.LoadFeatures();
            engine.InitializeFeatures();
            engine.StartClients();

            while (true)
            {
                System.Threading.Thread.Sleep(50);
            }
        }

    }
}
