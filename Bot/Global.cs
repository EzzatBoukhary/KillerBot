using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
 using Bot.Features.Economy;
using Bot.Features.RepeatedTasks;
using Discord;
using Discord.WebSocket;
using System.Reflection;
using System.Linq;

namespace Bot
{
    public static class Global
    {
        internal static DiscordSocketClient Client { get; set; }
        internal static Dictionary<ulong, string> MessagesIdToTrack { get; set; }
        internal static Random Rng { get; set; } = new Random();
        internal static Slot Slot = new Slot();
        internal static RepeatedTaskHandler TaskHander = new RepeatedTaskHandler();
        internal static bool Headless = false;
        // Global Helper methods

        internal static string GetRandomDidYouKnow()
        {
            return Constants.DidYouKnows[Rng.Next(0, Constants.DidYouKnows.Count)];
        }
        
        public static string ReplacePlacehoderStrings(this string messageString, IGuildUser user = null)
        {
            var result = messageString;
            result = ReplaceGuildUserPlaceholderStrings(result, user);
            result = ReplaceClientPlaceholderStrings(result);
            return result;
        }

        private static string ReplaceGuildUserPlaceholderStrings(string messageString, IGuildUser user)
        {
            if (user == null) return messageString;
            return messageString.Replace("<username>", user.Username)
                .Replace("<userdiscriminator>", user.Discriminator)
                .Replace("<usermention>", user.Mention)
                .Replace("<guildname>", user.Guild.Name);
        }

        private static string ReplaceClientPlaceholderStrings(string messageString)
        {
            if (Client == null) return messageString;
            return messageString.Replace("<botmention>", Client.CurrentUser.Mention)
                .Replace("<botdiscriminator>", Client.CurrentUser.Discriminator)
                .Replace("<botname>", Client.CurrentUser.Username);
        }
        public static readonly string version = "0.3.6";

        public static readonly string aboutMessage = $"Pootis Bot --- | --- {version}\n" +
                                                     $"Created by Creepysin licensed under the MIT license. Visit/blob/master/LICENSE.md for more info.\n\n" +
                                                     "Pootis Robot icon by Valve\n" +
                                                     "Created with Discord.NET\n" +
                                                     "https://github.com/Creepysin/Pootis-Bot \n\n" +
                                                     "Thank you for using Pootis Bot";

        public static string BotName;
        public static string BotPrefix = "k!";
        public static string BotToken;
        public static string BotStatusText;

        /// <summary>
        /// The bot owner account
        /// </summary>
        public static IUser BotOwner;

        /// <summary>
        /// The bot's logged in account
        /// </summary>
        public static IUser BotUser;
        /// <summary>
		/// Gets a role in a guild
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="roleName"></param>
		/// <returns></returns>
		public static SocketRole GetGuildRole(SocketGuild guild, string roleName)
        {
            IEnumerable<SocketRole> result = from a in guild.Roles
                                             where a.Name == roleName
                                             select a;

            SocketRole role = result.FirstOrDefault();
            return role;
        }
        /// <summary>
		/// Gets a role in a guild
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public static SocketRole GetGuildRole(SocketGuild guild, ulong roleId)
        {
            IEnumerable<SocketRole> result = from a in guild.Roles
                                             where a.Id == roleId
                                             select a;

            SocketRole role = result.FirstOrDefault();
            return role;
        }
        /// <summary>
		/// Logs a message to the console
		/// </summary>
		/// <param name="msg">The message? Yea the message</param>
		/// <param name="color">The color of the message</param>
		public static void Log(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{TimeNow()}] " + msg);
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
		/// Gets... you guessed it, THE TIME NOW!!!! (12hr time)
		/// </summary>
		/// <returns></returns>
		public static string TimeNow()
        {
            return DateTime.Now.ToString("hh:mm:ss tt");
        }
        /// <summary>
        /// Logs a message to the console
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(string msg)
        {
            Console.WriteLine($"[{TimeNow()}] " + msg);
        }
        public static async Task<string> SendWebRequest(string requestUrl)
        {
            using (var client = new HttpClient(new HttpClientHandler()))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "KillerBoT");
                using (var response = await client.GetAsync(requestUrl))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        return response.StatusCode.ToString();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        internal static void WriteColoredLine(string text, ConsoleColor color, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
