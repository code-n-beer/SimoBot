using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;

namespace SimoBot
{
    class UguuFeature : IFeature
    {

        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["uguu"] = Execute;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
        }

        public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string Message)
        {
            // Message sisältää argumentit
            // Jos Message on tyhjä, niin komennolle ei ole annettu argumentteja
            // Esimerkki:
            // !uguu -> Message on tyhjä
            // !uguu Crank -> Message == "Crank"
            // uguu u:kirjainten määrä vaihtelee satunnaisesti 2-17 välillä

            string text = "";
            string uguu = "";

            IrcUserCollection nicklist = Client.Channels.Client.Users;


            Random random = new Random();

            if (Message == "")
            {
                int rnd = random.Next(0,15);
                for (int i = 0; i < rnd; i++){
                    uguu += 'u';
                }
                uguu += '~';

                                string[] IllegalNicks = { "SIMOBOT", Sender.NickName};
                                int rndNick = random.Next(0, nicklist.Count);
                text = nicklist[rndNick].NickName + uguu;
                // text = getRandomNick() + uguu;
            }
            else
            {
                int rnd = random.Next(0,15);
                for (int i = 0; i < rnd; i++){
                    uguu += 'u';
                }
                uguu += '~';
                text = Message + uguu;
            }

            Client.LocalUser.SendMessage(Channel, text);
        }
    }
}
