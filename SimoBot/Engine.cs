using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
    public delegate void MessageHandler (Message msg);
    
    class Engine
    {
        EngineFeatures features;

        public Engine()
        {
            features = new EngineFeatures
            {
                commands = new Dictionary<string, MessageHandler>(),
                regexes = new Dictionary<string, MessageHandler>(),
                catchAlls = new Dictionary<string, MessageHandler>()
            };
        }
    }

    public class EngineFeatures
    {
        public Dictionary<string, MessageHandler> commands;
        public Dictionary<string, MessageHandler> regexes;
        public Dictionary<string, MessageHandler> catchAlls;
    }
}
