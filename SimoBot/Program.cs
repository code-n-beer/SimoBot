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
            var confs = ConfigLoader.LoadConfig("config.txt");

            List<Server> servers = ConfigLoader.OrganizeConfsPerServer(confs);

            Engine engine = new Engine(servers, confs);
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
