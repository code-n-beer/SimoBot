using System;

namespace SimoBot
{
    interface IFeature
    {
        void Init(ISimo Simo);
    }

    interface ICommand
    {
        void Execute(IServer Server, IChannel Channel, string Sender, string Message);
    }

    interface IChannel
    {
        void Say(string Message);
    }

    interface IServer
    {
    }

    interface ISimo
    {
        void RegisterCommand(string Command, ICommand Handler);
    }
}
