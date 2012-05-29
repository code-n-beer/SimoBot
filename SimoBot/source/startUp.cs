using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimoBot
{
    class startUp
    {
        Engine engine;
        public startUp(Engine SimoEngine)
        {
            engine = SimoEngine;
        }

        public bool connectQs()
        {
            string server = "irc.censored.org";
            int port = 6667;
            string nick = "censored";
            string channel = "#censored";

            engine.RunIrc(server, port, channel, nick);

            return true;
        }

    }
}
