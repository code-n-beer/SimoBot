using System;
using System.Collections.Generic;

namespace SimoBot.Features
{
	class SalkkaritFeature : IFeature
	{
		private List<string> words;

		public void Initialize(Dictionary<string, Dictionary<string, string>> configs)
		{
			string fileName = ConfigLoader.FindValueFromNestedDictionary(configs, "salkkarit");
			if (fileName == null) {
				Console.WriteLine("Could not read salkkarit configuration");
				return;
			}
			ReadSalkkariFile(fileName);
		}

		private void ReadSalkkariFile(string fileName)
		{
			try
			{
				System.IO.StreamReader reader = new System.IO.StreamReader(fileName);
				ReadLines(reader);
			}
			catch (System.IO.IOException e)
			{
				Console.WriteLine("Could not open salkkarit file: " + fileName);
			}
		}

		void ReadLines(System.IO.StreamReader reader)
		{
			words = new List<String>();
			string line = reader.ReadLine();
			while (line != null) {
				string[] splitLine = line.Trim().Split('\t');
				if (splitLine.Length >= 2) {
					words.Add(splitLine[1]);
				}
				line = reader.ReadLine();
			}
		}

		public void RegisterFeature(EngineMessageHandlers features)
		{
			features.commands["salkkarit"] = Execute;
		}

		public void Execute(IrcDotNet.IrcClient client, string channel, IrcDotNet.IrcUser sender, string message)
		{
			if (words == null) {
				client.LocalUser.SendMessage(channel, "salkkari data base not initialized :<");
				return;
			}
			int index = new Random().Next(words.Count);
			client.LocalUser.SendMessage(channel, words[index]);
		}
	}
}

