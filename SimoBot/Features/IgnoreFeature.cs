using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimoBot
{
    class IgnoreFeature : IFeature
    {
        HashSet<String> ignoredHosts;

        string configIgnoNameKey = "ignofile";
        Regex nickRegex = new Regex(@"^[a-zA-Z_\-\[\]\^\{\}\|`][a-zA-Z0-9_\-\[\]\^\{\}\|`]*$");

        Dictionary<string, Dictionary<string, string>> configs;

        public void RegisterFeature(EngineMessageHandlers features)
        {
            features.commands["ignore"] = Execute;
            features.commands["unignore"] = ExecuteUnignore;
            features.commands["ig"] = Execute;
            features.commands["unig"] = ExecuteUnignore;
        }

        public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
        {
            this.configs = configs;
            ignoredHosts = new HashSet<String>();
            foreach (var channel in configs)
            {
                try
                {
                    loadIgnoredHosts(channel.Value[configIgnoNameKey]);
                }
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine("Ignorefile for " + channel.Key + " not defined");
                }
            }
        }

        public void Execute(IrcDotNet.IrcClient Client, string channel, IrcDotNet.IrcUser Sender, string message)
        {
            message = message.Trim();
            if (message == "")
            {
                Client.LocalUser.SendMessage(channel, ignoHelpMsg());
            }
            else
            {
                Client.LocalUser.SendMessage(channel, ignore(Client, channel, Sender, message));

                // do this here to make simo more ~responsive~
                refreshFile(configs[channel][configIgnoNameKey]);
            }
        }

        public void ExecuteUnignore(IrcDotNet.IrcClient Client, string channel, IrcDotNet.IrcUser Sender, string message)
        {
            message = message.Trim();
            if (message == "")
            {
                Client.LocalUser.SendMessage(channel, unignoHelpMsg());
            }
            else
            {
                Client.LocalUser.SendMessage(channel, unignore(Client, channel, Sender, message));

                // do this here to make simo more ~responsive~
                refreshFile(configs[channel][configIgnoNameKey]);
            }
        }

        private string ignore(IrcDotNet.IrcClient Client, string channel, IrcDotNet.IrcUser Sender, string message)
        {
            
            if (!nickRegex.IsMatch(message))
            {
                return "Regex failure ^^'";
                ignore(Client, channel, Sender, Sender.NickName);
            }

            string userathost = whois(Client, message);

            if (userathost == "")
            {
                return "I maybe didn't find that nick";
            }

            ignoredHosts.Add(userathost);

            return "Ignored " + message + " ^o^";

        }

        private string unignore(IrcDotNet.IrcClient Client, string channel, IrcDotNet.IrcUser Sender, string message)
        {
            if (!nickRegex.IsMatch(message))
            {
                return "Regex failure ^^'";
                ignore(Client, channel, Sender, Sender.NickName);
            }


            string userathost = whois(Client, message);

            if (userathost == "")
            {
                return "I maybe didn't find that nick";
            }

            if (ignoredHosts.Contains(userathost))
            {
                ignoredHosts.Remove(userathost);
                return "Unignored " + message;
            }
            return "Nick was not ignored...";
        }

        private string whois(IrcDotNet.IrcClient Client, string nick)
        {
            string host = "";
            string username = "";
            string result = "";
            Client.QueryWhoIs(nick);
            Client.WhoIsReplyReceived += (s, a) =>
            {
                if (a.User == null || !a.User.IsOnline)
                {
                    result = "ei";
                }
                host = a.User.HostName;
                username = a.User.UserName;
            };

            result = (result == "") ? "" : username + '@' + host;
            return result;
        }

        public bool IsIgnored(IrcDotNet.IrcUser Disguy)
        {
            string host = Disguy.HostName;
            string username = Disguy.UserName;
            return ignoredHosts.Contains(username + '@' + host);
        }

        private String ignoHelpMsg()
        {
            return "Usage: ignore NICK Prevents the bot from accepting commands from NICK";
        }

        private string unignoHelpMsg()
        {
 	        return "Usage unignore NICK Start accepting commands from NICK again";
        }
        private void loadIgnoredHosts(string filename)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            StreamReader reader;
            try
            {
                reader = new StreamReader(filename);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ignorefile " + filename + " not found");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - aids happened");
                return;
            }

            string line = reader.ReadLine();
            while (line != null)
            {
                if (line.Contains('@') && line.Contains('.'))
                {
                    ignoredHosts.Add(line);
                }
                else Console.WriteLine("Ignorefile " + filename + " contains incomprehensible crap, continuing");

                line = reader.ReadLine();
            }

            reader.Close();
        }

        private void refreshFile(string filename)
        {
            StreamWriter writer = new StreamWriter(filename, false);
            foreach (String host in ignoredHosts)
            {
                writer.WriteLine(host);
            }
            writer.Flush();
            writer.Close();
        }

    }
}
