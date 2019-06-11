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
using Bot.Features.GlobalAccounts;

namespace Bot.Modules
{
    public class Owner : ModuleBase<MiunieCommandContext>
    {
        private static readonly OverwritePermissions denyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);
        private readonly GlobalUserAccounts _globalUserAccounts;

        public Owner(GlobalUserAccounts globalUserAccounts)
        {
            _globalUserAccounts = globalUserAccounts;
        }
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
        [Command("guildlist")]
        [RequireOwner]
        public async Task GuildList()
        {
            try
            {
                string guildList = $"Guild Count: {Context.Client.Guilds.Count.ToString()} \n \n";
                var guilds = (Context.Client as DiscordSocketClient).Guilds;
                foreach (var g in guilds)
                {
                    guildList += $"Name: {g.Name}\n Owner: {g.Owner} \n ID: {g.Id} \n \n";
                }
                File.WriteAllText("guildlist.txt", guildList);
                await Context.Channel.SendFileAsync("guildlist.txt", null, false, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               
            }
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
        [Remarks("Usage: k!prefix forceleave {serverName}")]
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
        [Command("add-coins"),Alias("add-money")]
        [Remarks("Adds specified money to a specific user. Bot owner only.")]
        [RequireOwner]
        public async Task givecoins(SocketGuildUser user = null, ulong amount = 0)
        {
            if (user == null)
                throw new ArgumentException("Please mention a user.");
            if (amount == 0)
                throw new ArgumentException("Please specify an amount which is more than 0");
            var account = _globalUserAccounts.GetById(user.Id);
            var emb = new EmbedBuilder();
            account.Coins += amount;
            _globalUserAccounts.SaveAccounts();
            emb.WithColor(Color.Green);
            emb.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
            emb.WithCurrentTimestamp();
            emb.WithDescription($"**{user}** has received **{amount} coins**. <a:KBtick:580851374070431774> ");
            await ReplyAsync("", false, emb.Build());
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