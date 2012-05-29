using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimoBot
{
    public struct Message
    {
        public string firstWord, message, nick, action, channel, time;
        public string[] messageAsArray;
        public Message(string[] parsedMsgArray)
        {
            time = parsedMsgArray[0];
            nick = parsedMsgArray[1];
            channel = parsedMsgArray[2];
            action = parsedMsgArray[3];
            message = parsedMsgArray[4];

            messageAsArray = message.Split(' ');

            firstWord = messageAsArray[0];
        }

        public Message(string action, string message)
        {
            this.message = message;
            this.action = action;

            //Null all other variables which shouldn't be used.
            firstWord = null; nick = null; channel = null; time = null; messageAsArray = null;
        }
    }
}
