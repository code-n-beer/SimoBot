using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Timers;

namespace SimoBot
{
	class TimerHandler
	{
		string path;

		public Dictionary<string, List<SimoTimer>> nickTimerListDictionary;

		int maxTimers = 5;

		public Engine engine;

		public TimerHandler(string timerPath, Engine engine)
		{
			this.engine = engine;
			path = timerPath;
			nickTimerListDictionary = new Dictionary<string, List<SimoTimer>>();
			//Get old timers from text file
			readFromFile();
		}
		public static void TimerCallBack(object source, ElapsedEventArgs e)
		{
			SimoTimer st = (SimoTimer)source;
			st.th.engine.Say(st.nick + ": " + st.message);
			st.th.removeTimer(st);
			st.Dispose();
		}

		public static void TimerDisposed(object source, EventArgs e)
		{
			//Timer got disposed 

		}

		private void createNewTimer(string nick, string message, DateTime time, bool shouldAddToFile = true)
		{
			if (!nickTimerListDictionary.ContainsKey(nick))
			{
				nickTimerListDictionary[nick] = new List<SimoTimer>();
			}
			SimoTimer st = new SimoTimer(nick, message, time, this);
			nickTimerListDictionary[nick].Add(st);

			st.Elapsed += new ElapsedEventHandler(TimerCallBack);
			st.Disposed += new EventHandler(TimerDisposed);
			st.Interval = (time - DateTime.Now).TotalMilliseconds;
			st.Enabled = true;

			if (shouldAddToFile)
				addToFile(st);
		}

		private void removeTimer(SimoTimer st)
		{
			string nick = st.nick;
			SimoTimer curTimer;
			for (int i = 0; i < nickTimerListDictionary[nick].Count; i++)
			{
				curTimer = nickTimerListDictionary[nick][0];
				if (curTimer.nick == st.nick && curTimer.message == st.message && curTimer.time == st.time)
				{
					removeFromFile(curTimer);
					nickTimerListDictionary[nick].RemoveAt(i);
					return;
				}
			}
		}

		private void readFromFile()
		{
			StreamReader reader;
			String lines = "";
			try
			{
				reader = new StreamReader(path);
			}
			catch (FileNotFoundException)
			{
				Console.WriteLine("Timer file not found");
				return;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message + " - aids happened at TimerHandler");
				return;
			}

			string line = reader.ReadLine();
			while (line != null)
			{
				string[] splitLine = line.Split('|');
				string nick = splitLine[0];
				string message = splitLine[1];
				DateTime time;
				try
				{
					time = DateTime.Parse(splitLine[2]);
				}
				catch (Exception) //Gotta catch 'em all
				{
					Console.WriteLine("Couldn't convert datetime to string");
					line = reader.ReadLine();
					continue;
				}
				Console.Write((time - DateTime.Now).Milliseconds);
				if((time - DateTime.Now).Milliseconds < 0)
				{
					line = reader.ReadLine();
					continue;
				}

				createNewTimer(nick, message, time, false);

				lines += line + '\n';

				line = reader.ReadLine();
			}
			reader.Close();
			StreamWriter writer = new StreamWriter(path,false);
			writer.Write(lines);
			writer.Flush();
			writer.Close();
		}

		private void addToFile(SimoTimer st)
		{
			StreamWriter writer = new StreamWriter(path, true); //append
			writer.WriteLine(st.nick + "|" + st.message.Replace("|", "") + "|" + st.time); //replaced |'s from message so they don't break THE SYSTEM
			writer.Flush();
			writer.Close();
		}

		private void removeFromFile(SimoTimer st) //untested, might not work if line separators are not \n etc
		{
			StreamReader reader = new StreamReader(path);

			List<string> lines = new List<string>(reader.ReadToEnd().Split('\n'));
			lines.Remove(st.nick + "|" + st.message.Replace("|", "") + "|" + st.time);
			reader.Close();

			dumpListToFile(lines);
		}

		private void dumpListToFile(List<string> lines)
		{
			StreamWriter writer = new StreamWriter(path);
			string separatedLines = string.Join("\n", lines.ToArray());
			writer.Write(separatedLines);
			writer.Flush();
			writer.Close();
		}

		public string addTimer(string nick, string message, string time)
		{
			//After actually implementing calendar_style:
			//If adding a date, return error if it has already passed or is less than half minute from now and recommend using the alarm
			try
			{
				if (!Parser.isValidTime(time))
				{
					return "Time format not leggins";
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Parser.isValidTime(" + time + ") crashed");
				return "";
			}

			bool containsKey = nickTimerListDictionary.ContainsKey(nick);

			if (containsKey)
			{
				if (nickTimerListDictionary[nick].Count >= maxTimers && nick != "Tsarpf") //Hard coding ftw
				{
					return nick + " Max timer count reached, not added";
				}
			}

			if (!containsKey) //If it doesn't even have an entry by the nick, there can't be too many timers
			{
				nickTimerListDictionary[nick] = new List<SimoTimer>();
			}
			DateTime dt;
			try
			{
				dt = Parser.convertToDateTimeAndGetAlarmTime(time);
			}
			catch (Exception e)
			{
				Console.WriteLine("Parser.convertToDateTimeAndGetAlarmTime(" + time + ") crashed");
				Console.WriteLine(e.Message);
				return "";
			}

			try
			{
				createNewTimer(nick, message, dt);
			}
			catch (Exception e)
			{
				Console.WriteLine("createNewTimer(" + nick + ", " + message + ", " + dt + ") crashed");
				return "";
			}
			return "Alarm added, will trigger at: " + dt;

		}


		public string removeTimer(string nick, int idx) //untested, see comment below about disposing
		{
			if (nickTimerListDictionary.ContainsKey(nick))
			{
				if (nickTimerListDictionary[nick].Count - 1 >= idx)
				{
					string rString = "Removed '" + nickTimerListDictionary[nick][idx].message + "'";
					removeFromFile(nickTimerListDictionary[nick][idx]);
					nickTimerListDictionary[nick][idx].Dispose();
					nickTimerListDictionary[nick].RemoveAt(idx); //Not sure if we can do this after dispose
					return rString;
				}
			}

			return "Timer with given index not found";
		}

		public string removeTimer(string nick, string whatToDelete)
		{
			if(nickTimerListDictionary.ContainsKey(nick))
			{
				switch (whatToDelete)
				{
					case "first":
						if (nickTimerListDictionary[nick].Count >= 1)
						{
							int idx = 0;
							string rString = "Removed '" + nickTimerListDictionary[nick][idx].message + "'";
							removeFromFile(nickTimerListDictionary[nick][idx]);
							nickTimerListDictionary[nick][idx].Dispose();
							nickTimerListDictionary[nick].RemoveAt(idx); //Not sure if we can do this after dispose
							return rString;
						}
						break;
					case "last":
						if (nickTimerListDictionary[nick].Count >= 1)
						{
							int idx = nickTimerListDictionary[nick].Count - 1;
							string rString = "Removed '" + nickTimerListDictionary[nick][idx].message + "'";
							removeFromFile(nickTimerListDictionary[nick][idx]);
							nickTimerListDictionary[nick][idx].Dispose();
							nickTimerListDictionary[nick].RemoveAt(idx); //Not sure if we can do this after dispose
							return rString;
						}
						break;
					case "all":
						if (nickTimerListDictionary[nick].Count >= 1)
						{
							string rString = "";
							for (int idx = 0; idx < nickTimerListDictionary[nick].Count;)
							{
								rString += "Removed '" + nickTimerListDictionary[nick][idx].message + "' ";
								removeFromFile(nickTimerListDictionary[nick][idx]);
								nickTimerListDictionary[nick][idx].Dispose();
								nickTimerListDictionary[nick].RemoveAt(idx); //Not sure if we can do this after disposed
							}

							return rString;
						}
						break;
				}
			}
			return "";
		}


		/*
		public string removeTimer(string nick, string timerName)
		{
			//Loop through timers for user, check their name propery, when match, remove. Return ""
			//If not found, return error
			//remove from persistence text file
		}
		 */
	}
}
