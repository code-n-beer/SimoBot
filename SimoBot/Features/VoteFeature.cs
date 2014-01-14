using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot.Features
{
	class VoteFeature : IFeature
	{
		public void RegisterFeature(EngineMessageHandlers features)
		{
			features.commands["vote"] = Execute;
		}

		Dictionary<string, Vote> ongoingVotes;
		public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
		{
            //Todo load from file
			ongoingVotes = new Dictionary<string, Vote>();
		}

		public void Execute(IrcDotNet.IrcClient Client, string Channel, IrcDotNet.IrcUser Sender, string message)
		{

            string[] words = message.Split(' ');
			int argc = words.Length;

			string returnString = ""

			switch (argc)
			{
                case 0:
					string halpmsg = "Usage: to start a new vote, type \"!vote nameofvote description of vote that is over 1 word in length\"." +
					                 "To take part in a vote, type \"!vote nameofvote y/n\"." +
									 "To end a vote, type \"!vote nameofvote end\".";

					Client.LocalUser.SendMessage(Channel, halpmsg);
					break;
				case 1:
                    //Todo: list ongoing votes
					break;
				case 2:
                    //check if vote exists
					switch (words[1])
					{
						case "end":
							returnString = EndVote(words[0], Sender.NickName);
							break;
						case "y":

							break;
						case "n":
							break;
						default:
							break;
					}
					break;
				default:
                    //means the user wants to start a new vote.
					string voteName = words[0];
					string nick = Sender.NickName;

					var wordsList = new List<string>(words);
					wordsList.RemoveAt(0);
                    


					break;
			}
		}

		private string Vote(string voteName, string nick, bool ans)
		{
			if (!ongoingVotes.Keys.Contains(voteName))
				return "No such vote";

            voteName = voteName.ToLower();
			int penis = ongoingVotes[voteName].yesCount;

			Vote v = ongoingVotes[voteName];
			if (ans)
				v.yesCount++;
			else
				v.noCount++;

			ongoingVotes[voteName] = v;

			return voteName + ": y/n: " + v.yesCount + "/" + v.noCount + " out of " + (v.yesCount + v.noCount);
		}

		private string StartVote(string voteName, string nick, List<string> description)
		{
			if (ongoingVotes.Keys.Contains(voteName))
				return "Vote name already used";

			string desc = "";
			for (int i = 0; i < description.Count; i++)
			{
				desc += description[i] + " ";
			}
			desc = desc.Trim();


			Vote vote = new Vote();
			vote.name = voteName.ToLower();
			vote.nick = nick;
			vote.description = desc;

            //Todo write to a file.

			return "Created new vote " + voteName + ": " + desc;
		}

		private string EndVote(string vote, string nick)
		{
			return "not implemented"; 
		}

		struct Vote
		{
            public string name;
			public string nick;
			public string description;
			public int yesCount;
			public int noCount;
		}



	}
}
