using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;
using SimoBot;

namespace SimoBotClient
{
    public delegate void MessageEventHandler(object sender, IrcMessageEventArgs e, IrcClient client);


    class Client
    {

        public event MessageEventHandler MsgEvent;

        private IrcUserRegistrationInfo regInfo;
        private IrcClient client;

        Server server;

        string[] channels;
        public Client(Server server)
        {
            client = new IrcClient();
            this.server = server;


            channels = getChannels(server);

            SetEventHandlers();
            findClientConfs();
        }

        private string[] getChannels(Server server)
        {
            string[] channels = new string[server.channels.Count];

            for(int i = 0; i < server.channels.Count; i++)
            {
                channels[i] = server.channels[i].channel;
            }

            return channels;
        }


        public void Connect()
        {
            client.Connect(server.server, false, regInfo);
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
            var conf = server.channels[0].config;

            regInfo.NickName = conf["nickname"];
            regInfo.Password = conf["password"];
            regInfo.RealName = conf["realname"];
            regInfo.UserName = conf["username"];
            regInfo.UserModes = new char[] { };

        }

        private void SetEventHandlers()
        {
            client.Registered += OnRegistered;
            client.Disconnected += OnDisconnected;
            client.RawMessageReceived += OnRawMessage;
        }
    }
}
