using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Bot.Extensions;
using Discord.Addons.Interactive;
using Bot.Handlers;
using Bot.Preconditions;
using System.Collections.Generic;
using Discord.Net;
using System.Text;

namespace Bot.Modules
{
    public class Owner : ModuleBase<MiunieCommandContext>
    {
        private static readonly OverwritePermissions denyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);
        private int _fieldRange = 10;
        private CommandService _service;


        [Command("SetStream")]
        [Remarks("Usage: |prefix|setstream {streamer} {streamName}")]
        [RequireOwner]
        public async Task SetStreamAsync(string streamer, [Remainder] string streamName)
        {
            await Context.Client.SetGameAsync(streamName, $"https://twitch.tv/{streamer}", ActivityType.Streaming);
            await ReplyAsync(
                    $"Set the bot's stream to **{streamName}**, and the Twitch URL to **[{streamer}](https://twitch.tv/{streamer})**.")
               ;
        }
        [Command("Showser")]
        [Summary("Show the servers the bot is in")]
        [RequireOwner]
        public async Task ListGuilds()
        {

            var guilds = (Context.Client as DiscordSocketClient).Guilds.ToList();
            StringBuilder sb = new StringBuilder();
            var embed = new EmbedBuilder();
            foreach (var guild in guilds)
            {
                sb.AppendLine($"{guild.Name} - {guild.Id}"); // Owner: {guild.Owner}
            }
            embed.WithColor(new Color(0, 255, 0));
            embed.Title = "Server List:";
            embed.WithFooter($"Count: {Context.Client.Guilds.Count.ToString()}", Context.User.GetAvatarUrl());
            embed.WithCurrentTimestamp();
            embed.Description = sb.ToString();
            await ReplyAsync("", false, embed.Build());

        }
        [Command("setgame"), Alias("ChangeGame", "SetGame")]
        [Remarks("Change what the bot is currently playing.")]
        [RequireOwner]
        public async Task SetGame([Remainder] string gamename)
        {
            await Context.Client.SetGameAsync(gamename);
            await ReplyAsync($"Changed game to `{gamename}`");
        }
        [Command("ForceLeave")]
        [Remarks("Usage: |prefix|forceleave {serverName}")]
        [RequireOwner]
        public async Task ForceLeaveAsync([Remainder] string serverName)
        {
            var target = Context.Client.Guilds.FirstOrDefault(g => g.Name == serverName);
            if (target is null)
            {
                EmbedBuilder builder1 = new EmbedBuilder();
                builder1.Color = new Color(114, 137, 218);
                builder1.AddField("ForceLeave", $"I'm not in the guild **{serverName}**.");
                await ReplyAsync("", false, builder1.Build());


            }

            await target.LeaveAsync();
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = new Color(114, 137, 218);
            builder.AddField("ForceLeave", $"Successfully left **{target.Name}**");
            await ReplyAsync("", false, builder.Build());

        }


        [Command("setAvatar"), Remarks("Sets the bots Avatar")]
        [RequireOwner]
        public async Task SetAvatar(string link)
        {
            var s = Context.Message.DeleteAsync();

            try
            {
                var webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(link);

                var stream = new MemoryStream(imageBytes);

                var image = new Image(stream);
                await Context.Client.CurrentUser.ModifyAsync(k => k.Avatar = image);
            }
            catch (Exception)
            {
                var embed = EmbedHandler.CreateEmbed("Avatar", "Coult not set the avatar!", EmbedHandler.EmbedMessageType.Exception);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }
    }
}