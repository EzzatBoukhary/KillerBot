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
                    guildList += $"Name: {g.Name}\n ID: {g.Id} \n Owner: {g.Owner} \n Owner ID: {g.OwnerId} \n \n";
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
        [Remarks("Usage: k!prefix forceleave {serverID}")]
        [RequireOwner]
        public async Task ForceLeaveAsync(ulong ServerId, [Remainder] string msg)
        {
            var target = Context.Client.Guilds.FirstOrDefault(g => g.Id == ServerId);
            if (string.IsNullOrWhiteSpace(msg))
                await ReplyAsync("You must provide a reason for leaving the server.");
            var client = Context.Client;
            var gld = client.GetGuild(ServerId);
            var ch = gld.DefaultChannel;
            var embed = new EmbedBuilder()
            {
                Description = $"KillerBot has been forced to leave this Server by its owner.\n**Reason:** {msg}",
                Color = new Color(255, 0, 0),
                Author = new EmbedAuthorBuilder()
                {
                    Name = Context.User.Username,
                    IconUrl = Context.User.GetAvatarUrl()
                }
            };
            try
            {
                await ch.SendMessageAsync("", embed: embed.Build());
            }
            catch
            {
                await ReplyAsync("Missing permissions of sending the leave message to the default channel.");
            }
            
            await Task.Delay(5000);
            await gld.LeaveAsync();
            await ReplyAsync($"KillerBot has left {ServerId}.");

        }
        [Command("shutdown")]
        [Summary("Turns off KillerBot")]
        [RequireOwner]
        public async Task Quit()
        {
            var todel = await ReplyAsync("Shutting down...");
            var channel = Context.Client.GetChannel(550072406505553921) as SocketTextChannel;
            var todol = channel.SendMessageAsync("<:KBfail:580129304592252995>");
            var task = Task.Run(async () =>
            {

                await Task.Delay(1000);
                Environment.Exit(0);
               
            });
        }
        [Command("logs")]
        [Summary("Show KillerBot logs")]
        [RequireOwner]
        public async Task ShowLogs()
        {
            var folder = Constants.LogFolder;
            var fileName = "Logs.log";
            await Context.Channel.SendFileAsync($"{folder}/{fileName}");
        }

        [Command("add-coins"),Alias("add-money")]
        [Summary("Adds specified money to a specific user. Bot owner only.")]
        [RequireOwner]
        public async Task addcoins(ulong user,string source ,ulong amount = 0)
        {
            
            if (amount == 0)
                throw new ArgumentException("Please specify an amount which is more than 0");
            var account = _globalUserAccounts.GetById(user);
            var emb = new EmbedBuilder();
            if (source == "wallet")
            {
                account.Coins += amount;
                _globalUserAccounts.SaveAccounts();
            }
            else if (source == "bank")
            {
                account.BankCoins += amount;
                _globalUserAccounts.SaveAccounts();
            }
            else
            {
                throw new ArgumentException("Source should be either wallet or bank");
            }
           
            emb.WithColor(Color.Green);
            emb.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
            emb.WithCurrentTimestamp();
            emb.WithDescription($"The user of ID **{account.Id}** has received **{amount} coins**. <a:KBtick:580851374070431774> ");
            emb.WithFooter($"New balance: {account.Coins} coins");
            await ReplyAsync("", false, emb.Build());
        }

        [Command("remove-coins"), Alias("remove-money")]
        [Summary("Remove specified money from a specific user. Bot owner only.")]
        [RequireOwner]
        public async Task removecoins(ulong user, string source, ulong amount = 0)
        {
            if (amount == 0)
                throw new ArgumentException("Amount specified is can't be 0");
            var account = _globalUserAccounts.GetById(user);

            if (amount > account.Coins)
                throw new ArgumentException($"Amount specified is not available ({amount} > {account.Coins})");
            var emb = new EmbedBuilder();
            if (source == "wallet")
            {
                account.Coins -= amount;
                _globalUserAccounts.SaveAccounts();
            }
            else if (source == "bank")
            {
                account.BankCoins -= amount;
                _globalUserAccounts.SaveAccounts();
            }
            else
            {
                throw new ArgumentException("Source should be either wallet or bank");
            }
            emb.WithColor(Color.Red);
            emb.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
            emb.WithCurrentTimestamp();
            emb.WithDescription($"The user of ID **{user}** forcefully lost **{amount} coins**. <a:KBtick:580851374070431774> ");
            emb.WithFooter($"New balance: {account.Coins} coins");
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