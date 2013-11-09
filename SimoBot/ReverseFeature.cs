using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
    // Name it SomethingFeature plz.
    // implement the IFeature interface (":" is the same as Java's "implements")
    class ReverseFeature : IFeature
    {
        public void RegisterFeature(EngineMessageHandlers features)
        {
            // replace "r" with !<this part> of the command
            features.commands["r"] = Execute; // Name the 
        }

        public void Initialize(ChannelConfigs configs)
        {
            // If you need to initialize something, f.ex create a database connection using strings from the configuration file etc you probably should do it here.
            // Also if you need to keep a list of sometihng over multiple executions of the command or something you could instantiate the list here.


            // The configuration dictionary stuff is generated at startup. Probably from (a|the) file the name of which is supplied as a commandline argument thingy.

            // If you need to add something to the configuration file and do not have access to it
            // (f.ex because it's not included in the source repository and prolly resides right next to the executable),
            // please ask the ~administrator~ for halp

            // The ChannelConfigs contains a dictionary in which the key is the channel's name and the value is an another dictionary.
            // The dictionary within the channels dictionary contains the actual configuration strings.
            // It can be used like so:
            
            //string connectionString = configs.channelConfigs["#NameOfTheChannelInQuestion"]["exampleDatabaseConnectionStringOrWhatever"];

            //string anotherExampleString = configs.channelConfigs["#OtherExample"]["plainTextPassword"];

        }




        // The parameter 'message' contains the whole message without the command part ("!this" thing)

        // 'client' is used to send messages to the channel like I've done here.
        // You can also use it to get a list of nicks in the channel, topic of the channel, etc.

        // 'channel' just contains the name of the channel.

        // Please don't hardcode channel names or anything similar in here,
        // save the stuff in the configuration file and then use a variable to use it where necessary
        public void Execute(IrcDotNet.IrcClient client, string channel, IrcDotNet.IrcUser sender, string message)
        {
            // Do whatever you need to here
            string reverse = "";
            char[] cArray = message.ToCharArray();
            Array.Reverse(cArray);
            reverse = new string(cArray);

            // You can send a message to the channel from here.
            client.LocalUser.SendMessage(channel, reverse);
        }
    }
}
