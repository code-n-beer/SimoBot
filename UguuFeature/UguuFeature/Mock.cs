using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimoBot;

namespace UguuFeature
{    class MockSimo : ISimo
    {
        private Dictionary<string, ICommand> commands;

        public MockSimo()
        {
            commands = new Dictionary<string, ICommand>();
        }

        public void RegisterCommand(string Command, ICommand Handler)
        {
            commands.Add(Command, Handler);
        }

        public void ExecuteCommand(string Command)
        {
            if (!Command.StartsWith("!"))
            {
                return;
            }

            string[] parts = Command.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string cmd = parts[0].Substring(1);

            string message = "";
            if (commands.ContainsKey(cmd))
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    message += parts[i];
                    if (i != parts.Length - 1)
                    {
                        message += " ";
                    }
                }
            }
            else 
            {
                return;
            }

            commands[cmd].Execute(null, new MockChannel(), "TestUser", message);
        }
    }

    class MockChannel : SimoBot.IChannel
    {
        public void Say(string Message)
        {
            Console.WriteLine(Message);
        }
    }
}
