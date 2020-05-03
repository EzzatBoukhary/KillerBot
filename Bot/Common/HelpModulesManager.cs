using System.Collections.Generic;
using System.Linq;
using Bot.Configuration;
using Bot.Entities;

namespace Bot.Common
{
	public static class HelpModulesManager
	{
		private const string HelpModulesFile = "resources/HelpModules.json";
		private static List<HelpModule> _helpModules;

		static HelpModulesManager()
		{
			if (JsonDataStorage.SaveExists(HelpModulesFile))
			{
				_helpModules = JsonDataStorage.LoadHelpModules(HelpModulesFile).ToList();
			}
			else
			{
				_helpModules = DefaultHelpModules();
				SaveHelpModules();
			}
		}

		/// <summary>
		/// Saves help modules... obviously 
		/// </summary>
		public static void SaveHelpModules()
		{
			JsonDataStorage.SaveHelpModules(_helpModules, HelpModulesFile);
		}

		/// <summary>
		/// Gets all the help modules
		/// </summary>
		/// <returns></returns>
		public static List<HelpModule> GetHelpModules()
		{
			return _helpModules;
		}

		/// <summary>
		/// Resets help modules to their default state
		/// </summary>
		public static void ResetHelpModulesToDefault()
		{
			_helpModules = null;
			_helpModules = DefaultHelpModules();
		}

		private static List<HelpModule> DefaultHelpModules()
		{
			List<HelpModule> helpModules = new List<HelpModule>();

            HelpModule moderation = new HelpModule
            {
                Group = "Moderation",
                Modules = new List<string> { "moderation", "Prefix", "Announcements" }
            };
            helpModules.Add(moderation);

            HelpModule basic = new HelpModule
			{
				Group = "Basic",
				Modules = new List<string> { "Help", "Basics", "Misc", "QuoteCommand" }
			};
			helpModules.Add(basic);

			HelpModule utils = new HelpModule
			{
				Group = "Utils",
				Modules = new List<string> { "PollModule", "Blogs", "Tags", "WeatherReportCurrent", "Reminder", "auctions", "RoleByPhrase" }
			};
			helpModules.Add(utils);

			HelpModule account = new HelpModule
			{
				Group = "Economy",
				Modules = new List<string> { "Economy", "ManageUserAccount" }
			};
			helpModules.Add(account);

			HelpModule fun = new HelpModule
			{
				Group = "Fun",
				Modules = new List<string> {"GiphyCommands", "GoogleSearch", "YouTubeCommands", "combat", "XkcdModule", "MiniGames", "images", "Trivia"}
			};
			helpModules.Add(fun);


            HelpModule owner = new HelpModule
            {
                Group = "Bot Owner",
                Modules = new List<string> { "Owner" }
            };
            helpModules.Add(owner);

            return helpModules;
		}
	}
}