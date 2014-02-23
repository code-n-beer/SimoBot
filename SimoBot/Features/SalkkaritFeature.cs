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
			if (fileName != null) {
				Console.WriteLine("Initializing salkkarit");
				ReadWords(fileName);
			}
		}

		private void ReadWords(string fileName)
		{
			System.IO.StreamReader reader;
			try
			{
				reader = new System.IO.StreamReader(fileName);
			}
			catch (System.IO.IOException e)
			{
				Console.WriteLine("Could not open salkkarit file: " + fileName);
				return;
			}

			words = new List<String>();
			string line = reader.ReadLine();
			while (line != null) {
				line = line.Trim();
				if (line.Length > 0 && line[0] != '#') {
					words.Add(line.Split('\t')[1]);
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
			int index = new Random().Next(words.Count);
			client.LocalUser.SendMessage(channel, words[index]);
		}
	}
}

