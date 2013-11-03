using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
    public delegate void MessageHandler(Message msg);
    
    class Engine
    {
        EngineMessageHandlers handlers;
        List<IFeature> features;
        ChannelConfigs configs;

        public Engine(ChannelConfigs configs)
        {
            this.configs = configs;
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
                f.Initialize(configs);
            }
        }
    }

    public class EngineMessageHandlers
    {
        public Dictionary<string, MessageHandler> commands;
        public Dictionary<string, MessageHandler> regexes;
        public Dictionary<string, MessageHandler> catchAlls;
    }
}
