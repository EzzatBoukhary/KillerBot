using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using ColorThiefDotNet;
using Discord.WebSocket;

namespace Bot.Helpers
{
    public static class DiscordHelpers
    {
        public static IRole GetUsersHigherstRole(IGuildUser user)
        {
            IReadOnlyCollection<ulong> roleIds = user.RoleIds;

            IRole highestRole = null;
            foreach (ulong id in roleIds)
            {
                IRole role = user.Guild.GetRole(id);
                if (role.Name == "@everyone")
                    continue;
                if (role.Color.RawValue == 0)
                    continue;

                if (highestRole == null)
                    highestRole = role;
                else if (role.Position > highestRole.Position)
                    highestRole = role;
            }

            return highestRole;
        }

        public static string GetListOfUsersRoles(IGuildUser user)
        {
            string roles = "";

            IReadOnlyCollection<ulong> roleIds = user.RoleIds;

            foreach (ulong id in roleIds)
            {
                IRole role = user.Guild.GetRole(id);
                if (role.Name == "@everyone")
                    continue;

                roles += $"`{role.Name}`, ";
            }

            if (string.IsNullOrEmpty(roles) == false)
                roles = roles.Remove(roles.Length - 2, 2);

            return roles;
        }
        private static readonly string[] regionalIndicatorLetters = "🇦|🇧|🇨|🇩|🇪|🇫|🇬|🇭|🇮|🇯|🇰|🇱|🇲|🇳|🇴|🇵|🇶|🇷|🇸|🇹|🇺|🇻|🇼|🇽|🇾|🇿|".Split('|');

        public static string GetUnicodeRegionalLetter(int index)
        {
            if (index < 0 || index >= regionalIndicatorLetters.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return regionalIndicatorLetters[index];
        }
        public readonly static Discord.Color Red = new Discord.Color(231, 76, 60);
        public readonly static Discord.Color Green = new Discord.Color(31, 139, 76);
        public readonly static Discord.Color ClearColor = new Discord.Color(54, 57, 63);

        // Universal Web Client
        public static readonly WebClient webClient = new WebClient();

        // Color Thief (gets the dominant color of an image, makes my embeds look pretty)
        private static ColorThief colorThief = new ColorThief();

        // Get a random number
        public static readonly Random getrandom = new Random();
        public static int GetRandomNumber(int min, int max)
        {
            lock (getrandom) { return getrandom.Next(min, max); }
        }

        // Generic Embed template
        public static Embed Embed(string t, string d, Discord.Color c, string f, string thURL) => new EmbedBuilder()
            .WithTitle(t)
            .WithDescription(d)
            .WithColor(c)
            .WithFooter(f)
            .WithThumbnailUrl(thURL)
            .Build();

        // Generic Image Embed template
        public static Embed ImageEmbed(string t, string d, Discord.Color c, string f, string imageURL) => new EmbedBuilder()
            .WithTitle(t)
            .WithDescription(d)
            .WithColor(c)
            .WithFooter(f)
            .WithImageUrl(imageURL)
            .Build();

        // Print a success message
        public static async Task PrintSuccess(ISocketMessageChannel channel, string description) => await SendEmbed(channel, "Success", description, Green, "", "").ConfigureAwait(false);

        // Print an error
        public static async Task PrintError(ISocketMessageChannel channel, string description) => await SendEmbed(channel, "Error", description, Red, "", "").ConfigureAwait(false);

        // Get a dominant color from an image (url)
        public static Discord.Color DomColorFromURL(string url)
        {
            using (Bitmap bitmap = new Bitmap(DownloadImage(url)))
            {
                // Remove the '#' from the string and get the hexadecimal
                return HexToRGB(colorThief.GetColor(bitmap).Color.ToString().Substring(1));
            }
        }

        public static System.Drawing.Image DownloadImage(string url)
        {
            using (WebClient client = new WebClient())
            using (MemoryStream ms = new MemoryStream(client.DownloadData(url)))
                return System.Drawing.Image.FromStream(ms);
        }

        // Convert a hexidecimal to an RGB value (input does not include the '#')
        public static Discord.Color HexToRGB(string hex)
        {
            // First two values of the hex
            int r = int.Parse(hex.Substring(0, hex.Length - 4), System.Globalization.NumberStyles.AllowHexSpecifier);

            // Get the middle two values of the hex
            int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            // Final two values
            int b = int.Parse(hex.Substring(4), System.Globalization.NumberStyles.AllowHexSpecifier);

            return new Discord.Color(r, g, b);
        }

        // Send an embed to a channel
        public static async Task SendEmbed(ISocketMessageChannel channel, string title, string description, Discord.Color color, string footer, string thumbnailURL)
        {
            await channel.SendMessageAsync(null, false, Embed(title, description, color, footer, thumbnailURL)).ConfigureAwait(false);
        }

        // Send an embed to a channel
        public static async Task SendDomColorEmbed(ISocketMessageChannel channel, string title, string description, string imageURL, string footer = null)
        {
            await channel.SendMessageAsync(null, false, Embed(title, description, DomColorFromURL(imageURL), footer, imageURL)).ConfigureAwait(false);
        }
    }
}