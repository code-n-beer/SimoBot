using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;

namespace SimoBotClient
{
    public delegate void MessageEventHandler(object sender, IrcMessageEventArgs e, IrcClient client);

    class Client
    {
        public event MessageEventHandler MsgEvent;

        private IrcUserRegistrationInfo regInfo;
        private IrcClient client;
        Dictionary<string, string> configs;

        string server;
        string[] channels;

        public Client(Dictionary<string, string> configs)
        {
            client = new IrcClient();
            this.configs = configs;
            SetEventHandlers();
            findClientConfs();
        }


        public void Connect()
        {
            client.Connect(server, false, regInfo);
        }

        private void OnRawMessage(object sender, IrcRawMessageEventArgs e)
        {
            Console.Write(e.RawContent);
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Connect(); // :D
        }

        private void OnRegistered(object sender, EventArgs e)
        {
            client.Channels.Join(channels);
            client.LocalUser.JoinedChannel += (s, a) =>
                {
                    a.Channel.MessageReceived += OnMessageReceived;
                };
        }

        protected virtual void OnMessageReceived(object sender, IrcMessageEventArgs e)
        {
            if (MsgEvent != null)
                MsgEvent(this, e, client);
        }

        private void findClientConfs()
        {
            regInfo = new IrcUserRegistrationInfo();
            regInfo.NickName = configs["nickname"];
            regInfo.Password = configs["password"];
            regInfo.RealName = configs["realname"];
            regInfo.UserName = configs["username"];
            regInfo.UserModes = new char[] { };

            server = configs["server"];
            channels = configs["channels"].Split('|');
        }

        private void SetEventHandlers()
        {
            client.Registered += OnRegistered;
            client.Disconnected += OnDisconnected;
            client.RawMessageReceived += OnRawMessage;
        }
    }
}

/*
Simo simo = new Simo("SimoBot", "SimoBot", "simobot", "fuckshit");
foreach (var feature in features)
{
    feature.Init(simo);
}

simo.Channels = new string[] { "#tkt-cocknballs" };
simo.Connect("openirc.snt.utwente.nl");

while (simo.Connected) ;    
*/