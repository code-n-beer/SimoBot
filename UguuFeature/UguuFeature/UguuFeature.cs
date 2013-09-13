using System;
using SimoBot;

namespace UguuFeature
{
    class Uguu : ICommand
    {
        public void Execute(IServer Server, IChannel Channel, string Sender, string Message)
        {
            // Message sisältää argumentit
            // Jos Message on tyhjä, niin komennolle ei ole annettu argumentteja
            // Esimerkki:
            // !uguu -> Message on tyhjä
            // !uguu Crank -> Message == "Crank"

            string text;

            if (Message == "")
            {
                text = Sender + ": uguu~";
            }
            else
            {
                text = Message + ": uguu~";
            }

            Channel.Say(text);
        }
    }

    class UguuFeature : IFeature
    {
        // Initissä rekisteröidään komennot
        public void Init(ISimo Simo)
        {
            // Muista komento /ilman/ huutomerkkiä.
            Simo.RegisterCommand("uguu", new Uguu());
        }
    }
}
