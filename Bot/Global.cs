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
            return messageString.Replace("<username>", user.Nickname ?? user.Username)
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

        public static string GetMiuniesCountReaction(ulong value, string mention)
        {
            if (value > 100000)
            {
                return $"Wow, {mention}! You're either cheating or you're really dedicated.";
            }
            if (value > 50000)
            {
                return $"You must be here often to get that amount, {mention}. Thinkin of givin some out?";
            }
            if (value > 20000)
            {
                return $"That's enough to buy a house... \n\nFor me, not for you, shut up, {mention}!";
            }
            if (value > 10000)
            {
                return $"{mention} is kinda getting rich. Do we rob them or what?";
            }
            if (value > 5000)
            {
                return $"Is it just me or is {mention} taking this economy a little too seriously?";
            }
            if (value > 2500)
            {
                return $"Great, {mention}! Now you give me those coins.";
            }
            if (value > 1100)
            {
                return $"Looks like {mention} is showing their wealth on the internet again.";
            }
            if (value > 800)
            {
                return $"Alright, {mention}. Put the coins away and nobody gets hurt.";
            }
            if (value > 550)
            {
                return $"I like how {mention} thinks that's impressive.";
            }
            if (value > 200)
            {
                return $"Ouch, {mention}! If I knew that is all you've got, I would just DM you the amount. Embarrassing!";
            }
            if (value == 0)
            {
                return $"Yea, {mention} is broke. What a surprise.";
            }

            return $"Yea, {mention} you still have a lot to go.";
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
