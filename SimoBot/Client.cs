using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;

namespace SimoBot
{
    class Client
    {
        private IrcUserRegistrationInfo regInfo;
        IrcClient client;
        public Client(string nick, string realName, string userName, string password)
        {
            regInfo = new IrcUserRegistrationInfo();
            regInfo.NickName = nick;
            regInfo.Password = password;
            regInfo.RealName = realName;
            regInfo.UserName = userName;
            regInfo.UserModes = new char[] {};
            client = new IrcClient();
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