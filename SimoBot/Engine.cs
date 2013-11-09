using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;

namespace SimoBot
{
    public delegate void MessageHandler(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message);
    
    class Engine
    {
        EngineMessageHandlers handlers;
        List<IFeature> features;
		List<Server> servers;
        Dictionary<string, Dictionary<string, string>> confs;
        
        public Engine(List<Server> servers, Dictionary<string, Dictionary<string, string>> confs)
        {
			this.servers = servers;

            this.confs = confs;

            handlers = new EngineMessageHandlers
            {
                commands = new Dictionary<string, MessageHandler>(),
                regexes = new Dictionary<string, MessageHandler>(),
                catchAlls = new Dictionary<string, MessageHandler>()
            };
        }
        
        public void LoadFeatures()
        {
            //Finds all classes that implement the IFeature interface
            var interfaceType = typeof(IFeature);
            var all = AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(x => x.GetTypes())
              .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
              .Select(x => Activator.CreateInstance(x));

            features = new List<IFeature>();
            foreach (Object o in all)
            {
                IFeature f = (IFeature)o;
                f.RegisterFeature(handlers);
                features.Add(f);
            }
        }

        public void InitializeFeatures()
        {
            foreach (IFeature f in features)
            {
                f.Initialize(confs);
            }
        }
            
        public void StartClients()
        {
            /*
            foreach (var channel in configs.channelConfigs.Values)
            {
                var client = new SimoBotClient.Client(channel);
                client.Connect();
                client.MsgEvent += new SimoBotClient.MessageEventHandler(MessageReceived);
            }
            */
            foreach (Server server in servers)
			{
                var client = new SimoBotClient.Client(server);
                client.Connect();
                client.MsgEvent += new SimoBotClient.MessageEventHandler(MessageReceived);
			}
        }

        private void MessageReceived(object sender, IrcMessageEventArgs e, IrcClient client)
        {
            var parts = e.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].Substring(1);

            if(!handlers.commands.ContainsKey(cmd))
            {
                return;
            }

            var processedMessage = "";
            if (e.Text.Length >= (cmd.Length + 2))
            {
                processedMessage = e.Text.Substring(cmd.Length + 2);
            }

            string channel = "";

            foreach (var messageTarget in e.Targets)
            {
                if (messageTarget is IrcChannel)
                {
                    channel = messageTarget.Name;
                    break;
                }
            }

            if (channel == "")
            {
                return;
            }

            handlers.commands[cmd](client, channel, e.Source as IrcUser, processedMessage);
        }
    }

    public class EngineMessageHandlers
    {
        public Dictionary<string, MessageHandler> commands;
        public Dictionary<string, MessageHandler> regexes;
        public Dictionary<string, MessageHandler> catchAlls;
    }
}
