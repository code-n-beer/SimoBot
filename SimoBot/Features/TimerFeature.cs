namespace SimoBot
{
    class TimerFeature : IFeature
    {

        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["timer"] = Execute;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
        }
        public void Execute(IrcClient Client, string Channel, IrcUser Sender, string Message)
        {
            string message = "gänikoodi ei valmis XD";
            Client.LocalUser.SendMessage(Channel, message);
        }
    }
}
