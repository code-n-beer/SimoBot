using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using LinqToTwitter;

namespace SimoBot
{
	class SimoTwitter
	{
		TwitterContext twitterContext;
		static string credentialsPath;
		public SimoTwitter(string path)
		{
			credentialsPath = path;
			ITwitterAuthorizer auth = performAuth();
			twitterContext = new TwitterContext(auth);
		}

		public string tweet(string tweetMsg)
		{
			try
			{
				var tweetReturn = twitterContext.UpdateStatus(tweetMsg);
				return "Success";
			}
			catch (Exception e)
			{
				Console.WriteLine("Tweeting '" + tweetMsg + "' failed: " + e.Message);
			}

			return "";
		}


		private static ITwitterAuthorizer performAuth()
		{
			InMemoryCredentials credentials;

			// validate that credentials are present
			if (!GetCredentials(out credentials))
			{
				return null;
			}

			// configure the OAuth object
			var auth = new PinAuthorizer
			{
				Credentials = credentials,
				UseCompression = true,
				GoToTwitterAuthorization = pageLink => Console.WriteLine(pageLink),
				//GoToTwitterAuthorization = pageLink => Process.Start(pageLink),
				GetPin = () =>
				{
					// this executes after user authorizes, which begins with the call to auth.Authorize() below.
					Console.WriteLine("\nAfter you authorize this application, Twitter will give you a 7-digit PIN Number.\n");
					Console.Write("Enter the PIN number here: ");
					return Console.ReadLine();
				}
			};

			// start the authorization process (launches Twitter authorization page).
			try
			{
				auth.Authorize();
			}
			catch (WebException wex)
			{
				Console.WriteLine(
					"Unable to authroize with Twitter right now. Please check pin number" +
					" and ensure your credential keys are correct. Exception received: " + wex.ToString());

				return null;
			}

			File.WriteAllLines(credentialsPath, new string[] { auth.Credentials.ToString() });

			return auth;
		}

		private static bool GetCredentials(out InMemoryCredentials credentials)
		{
			credentials = new InMemoryCredentials();

			if (File.Exists(credentialsPath))
			{
				string[] lines = File.ReadAllLines(credentialsPath);
				if (lines != null && lines.Length > 0)
				{
					credentials.Load(lines[0]);
					return true;
				}
			}

			// validate that credentials are present
			string consumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"].Trim();
			string consumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"].Trim();
			Console.WriteLine(consumerKey + " " + consumerSecret);
			if(consumerKey == "" ||	consumerSecret == "")
			//if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["twitterConsumerKey"]) ||
			//	string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["twitterConsumerSecret"]))
			{
				Console.WriteLine("\nCan't Run Yet\n" +
								  "-------------\n");
				Console.WriteLine("You need to set twitterConsumerKey and twitterConsumerSecret \n" +
								  "in App.config/appSettings.");
				Console.WriteLine();
				Console.WriteLine("Please visit http://dev.twitter.com/apps for more info.\n");

				return false;
			}

			credentials = new InMemoryCredentials
			{
				ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"],
				ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"]
			};

			return true;
		}
	}
}