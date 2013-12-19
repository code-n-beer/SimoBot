using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimoBot
{
    class IgnoreFeature : IFeature
    {
        Dictionary<string, HashSet<string>> ignoredHosts;

        // General settings
        string configIgnoNameKey = "ignofile";
        Regex nickRegex = new Regex(@"^[a-zA-Z_\-\[\]\^\{\}\|`][a-zA-Z0-9_\-\[\]\^\{\}\|`]*$");
        bool nowrite = false;

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
            ignoredHosts = new Dictionary<string, HashSet<string>>();
            foreach (var channel in configs)
            {
                try
                {
                    ignoredHosts[channel.Key] = loadIgnoredHosts(channel.Value[configIgnoNameKey]);
                }
                catch (KeyNotFoundException)
                {
                    Console.WriteLine("Ignorefile for " + channel.Key + " not defined, switching to no-write mode");
                    nowrite = true;
                }
            }
        }

        public void Execute(IrcDotNet.IrcClient Client, string channel, IrcDotNet.IrcUser Sender, string message)
        {
            message = message.Trim();
            if (message == "")
            {
                Client.LocalUser.SendMessage(channel, ignoreHelpMsg());
            }
            else
            {
                ignore(Client, channel, Sender, message);
            }
        }

        public void ExecuteUnignore(IrcDotNet.IrcClient Client, string channel, IrcDotNet.IrcUser Sender, string message)
        {
            message = message.Trim();
            if (message == "")
            {
                Client.LocalUser.SendMessage(channel, unignoreHelpMsg());
            }
            else
            {
                unignore(Client, channel, Sender, message);
            }
        }

        private void ignore(IrcDotNet.IrcClient Client, string channel, IrcDotNet.IrcUser Sender, string message)
        {
            if (!nickRegex.IsMatch(message))
            {
                Client.LocalUser.SendMessage(channel, "Messing around, eh?");
                ignore(Client, channel, Sender, Sender.NickName);
                return;
            }

            IrcDotNet.IrcUser user = getUserByName(Client, channel, message);
            if (user == null)
            {
                Client.LocalUser.SendMessage(channel, "Who now?");
                return;
            }

            if (IsIgnored(user, channel))
            {
                Client.LocalUser.SendMessage(channel, "They're already ignored");
                return;
            }
                user.NickNameChanged += (b, d) =>
                    {
                        ignoredHosts[channel].Add(user.NickName);
                        if (!nowrite)
                            refreshFile(configs[channel][configIgnoNameKey], channel);
                    };

                Client.LocalUser.SendMessage(channel, "Ignored " + message + " ^o^");
                ignoredHosts[channel].Add(user.NickName);
                

                if (!nowrite)
                    refreshFile(configs[channel][configIgnoNameKey], channel);

        }

        private void unignore(IrcDotNet.IrcClient Client, string channel, IrcDotNet.IrcUser Sender, string message)
        {
            if (!nickRegex.IsMatch(message))
            {
                Client.LocalUser.SendMessage(channel, "Messing around, eh?");
                ignore(Client, channel, Sender, Sender.NickName);
                return;
            }

            if (!ignoredHosts[channel].Contains(message))
            {
                Client.LocalUser.SendMessage(channel, "They're not ignored");
                return;
            }

            Client.LocalUser.SendMessage(channel, "Unignored " + message);
            ignoredHosts[channel].Remove(message);

            if (!nowrite)
                refreshFile(configs[channel][configIgnoNameKey], channel);
        }

        private IrcDotNet.IrcUser getUserByName(IrcDotNet.IrcClient Client, string channel, string nick)
        {
            IrcDotNet.IrcUser user = null;
            foreach (IrcDotNet.IrcChannel c in Client.Channels)
            {
                if (c.Name == channel)
                {
                    foreach (IrcDotNet.IrcChannelUser u in c.Users)
                    {
                        if (u.User.NickName == nick)
                        {
                            user = u.User;
                            break;
                        }
                    }
                    break;
                }
            }
            return user;
        }

        public bool IsIgnored(IrcDotNet.IrcUser Disguy, string channel)
        {
            if (!ignoredHosts.ContainsKey(channel))
            {
                return false;
            }

            return ignoredHosts[channel].Contains(Disguy.NickName);
        }

        private string ignoreHelpMsg()
        {
            return "Usage: ignore NICK Prevents the bot from accepting commands from NICK";
        }

        private string unignoreHelpMsg()
        {
 	        return "Usage unignore NICK Start accepting commands from NICK again";
        }

        private HashSet<string> loadIgnoredHosts(string filename)
        {
            StreamReader reader;
            HashSet<string> channelIgnoredHosts = new HashSet<string>();
            try
            {
                reader = new StreamReader(filename);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ignorefile " + filename + " not found");
                return channelIgnoredHosts;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - aids happened");
                return channelIgnoredHosts;
            }

            string line = reader.ReadLine();
            while (line != null)
            {
                    channelIgnoredHosts.Add(line);

                line = reader.ReadLine();
            }

            reader.Close();
            return channelIgnoredHosts;
        }

        private void refreshFile(string filename, string channel)
        {
            StreamWriter writer = new StreamWriter(filename, false);
            foreach (string host in ignoredHosts[channel])
            {
                writer.WriteLine(host);
            }
            writer.Flush();
            writer.Close();
        }

    }
}
