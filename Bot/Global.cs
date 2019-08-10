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

namespace Bot
{
    public static class Global
    {
        internal static DiscordSocketClient Client { get; set; }
        internal static Dictionary<ulong, string> MessagesIdToTrack { get; set; }
        internal static Random Rng { get; set; } = new Random();
        internal static Slot Slot = new Slot();
        internal static RepeatedTaskHandler TaskHander = new RepeatedTaskHandler();
        internal static readonly String version = Assembly.GetExecutingAssembly().GetName().Version.ToString().TrimEnd('0').TrimEnd('.');
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
