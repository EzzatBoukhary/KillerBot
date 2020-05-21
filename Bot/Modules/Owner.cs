﻿using Discord.Commands;
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
        private readonly Logger _logger;

        public Owner(GlobalUserAccounts globalUserAccounts, Logger logger)
        {
            _globalUserAccounts = globalUserAccounts;
            _logger = logger;
        }
        [Command("SetStream")]
        [Remarks("Usage: k!setstream {streamer} {streamName}")]
        [RequireOwner]
        public async Task SetStreamAsync(string streamer, [Remainder] string streamName)
        {
            await Context.Client.SetGameAsync(streamName, $"https://twitch.tv/{streamer}", ActivityType.Streaming);
            await ReplyAsync(
                    $"Set the bot's stream to **{streamName}**, and the Twitch URL to **[{streamer}](https://twitch.tv/{streamer})**.")
               ;
        }
       
        [Command("guildlistnew")]
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
                await ReplyAsync("<a:KBtick:580851374070431774> Guild List was sent to your DMs!");
                var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                await dmChannel.SendFileAsync("guildlist.txt", null, false, null);
            }
            catch (Exception e)
            {
                await _logger.Log(LogSeverity.Error, "[Owner > guildlist]", $"{e}");

            }
        }
        [Command("owner-serverinfo"), Alias("o-sinfo")]
        [RequireOwner]
        [Summary("Shows server information.")]
        public async Task ownersinfo(ulong ID)
        {
            var channel = (ITextChannel)Context.Channel;
            var guild = Context.Client.GetGuild(ID) as SocketGuild;
            var ownername = guild.GetUser(guild.OwnerId);
            var textchn = guild.TextChannels.Count();
            var voicechn = guild.VoiceChannels.Count();
            var mem = guild.MemberCount;
            var botlist = guild.Users.Count(x => x.IsBot);
            var guildusers = mem - botlist;
            string mem2 = $"<:online2:704873654722232361> **{guild.Users.Count(x => x.Status == UserStatus.Online)}** - <:away2:704873837883555902> **{guild.Users.Count(x => x.Status == UserStatus.Idle)}** - <:dnd2:704873772406014104> **{guild.Users.Count(x => x.Status == UserStatus.DoNotDisturb)}** - <:offline2:704873716114260009> **{guild.Users.Count(x => x.Status == UserStatus.Offline)}** (<:members:704874969083019354> **{mem}** - 👤 **{guildusers}** - <:botTag:704886277920784445> **{botlist}**)";

            var createdAt = new DateTime(2015, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(guild.Id >> 22);
            var features = string.Join(", ", guild.Features);
            if (string.IsNullOrWhiteSpace(features))
                features = "-";
            var dot = "<:KBdot:580470791251034123>";
            var embed = new EmbedBuilder();
            embed.WithAuthor("== SERVER INFORMATION ==");
            embed.WithColor(Color.Blue);
            embed.WithTitle(guild.Name);
            embed.Description = $"{dot} ID: **{guild.Id}**" +
            $"\n{dot} Owner: **{ownername.ToString()}**" +
            $"\n{dot} Creation: **{guild.CreatedAt.DateTime.ToLongDateString()} {guild.CreatedAt.DateTime.ToLongTimeString()} UTC**" +
            $"\n{dot} Region: **{guild.VoiceRegionId.ToString()}**" +
            $"\n \n{dot} Members: {mem2}" +
            $"\n{dot} Roles: **{(guild.Roles.Count - 1).ToString()}**" +
            $"\n{dot} Channels: Text: **{textchn.ToString()}** - Voice: **{voicechn.ToString()}** - Categories: **{guild.CategoryChannels.Count}**" +
            $"\n \n{dot} Verification: **{guild.VerificationLevel.ToString()}**";

            if (guild.ExplicitContentFilter.ToString() == "MembersWithoutRoles")
                embed.Description += $"\n{dot} Explicit Content Filter: **Scans messages from members without roles**";

            else if (guild.ExplicitContentFilter.ToString() == "Disabled")
                embed.Description += $"\n{dot} Explicit Content Filter: **No message scanning**";

            else if (guild.ExplicitContentFilter.ToString() == "AllMembers")
                embed.Description += $"\n{dot} Explicit Content Filter: **Scans messages from all members**";

            if (Uri.IsWellFormedUriString(guild.IconUrl, UriKind.Absolute))
                embed.WithThumbnailUrl(guild.IconUrl);
            if (features != "-")
            {
                embed.Description += $"\n{dot} Features: **{features}**";
            }

            if (guild.Emotes.Any())
            {
                embed.Description += $"\n{dot} Custom Emojis `({guild.Emotes.Count})`: ";
                embed.Description += (string.Join(" ", guild.Emotes
                        .Take(11)
                        .Select(e => $"{e.ToString()}")));
            }
            if (features.Contains("INVITE_SPLASH") && (guild.SplashUrl != null))
            {
                embed.Description += $"\n{dot} [Splash Icon:]({guild.SplashUrl})";
                embed.WithImageUrl(guild.SplashUrl);
            }
            await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("getinvite")]
        [RequireOwner]
        [Remarks("Provides invite link to a given server by its id")]
        public async Task Join(ulong id)
        {
            var guild = Context.Client.GetGuild(id);
            if (guild == null)
            {
                await ReplyAsync("No server found");
                return;
            }
            try
            {
                var invites = await guild.DefaultChannel.CreateInviteAsync(86400, null, false, true);
                var invite = invites.Url.ToString();
                await ReplyAsync(invite);
            }
            catch
            {
                await ReplyAsync("I don't have enough permissions to create that invite link");
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
        [Remarks("Usage: k!prefix forceleave {serverID} {message}")]
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
            if (channel == null)
            {
                await ReplyAsync("<:KBfail:580129304592252995> The bot is offline.");
                Environment.Exit(0);
                return;
            }
            var todol = channel.SendMessageAsync("<:KBfail:580129304592252995>");
            var task = Task.Run(async () =>
            {

                await Task.Delay(1000);
                Environment.Exit(0);
               
            });
        }
        [Command("blacklist")]
        [RequireOwner]
        public async Task Blacklist(ulong id, string reason = null)
        {
            if (reason == null)
                reason = "[No reason was specified]";
            var UserToBlacklist = _globalUserAccounts.GetById(id);
            if (UserToBlacklist.Blacklisted == true)
            {
                throw new ArgumentException("User is already blacklisted");
            }
            else
            {
                UserToBlacklist.Blacklisted = true;
                _globalUserAccounts.SaveAccounts(id);
                var username = Context.Client.GetUser(id).Username;
                var discrim = Context.Client.GetUser(id).Discriminator;
                var emb = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithTitle("=== Blacklist ===")
                    .WithCurrentTimestamp()
                    .WithFooter($"Blacklisted by {Context.User}", Context.User.GetAvatarUrl())
                    .WithDescription($"Blacklisted user: **{username}#{discrim}** \nReason: {reason}");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
        [Command("whitelist"), Alias("unblacklist")]
        [RequireOwner]
        public async Task UnBlacklist(ulong id, string reason = null)
        {
            if (reason == null)
                reason = "[No reason was specified]";
            var UserToBlacklist = _globalUserAccounts.GetById(id);
            if (UserToBlacklist.Blacklisted == true)
            {
                UserToBlacklist.Blacklisted = false;
                _globalUserAccounts.SaveAccounts(id);
                var username = Context.Client.GetUser(id).Username;
                var discrim = Context.Client.GetUser(id).Discriminator;
                var emb = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("=== Whitelist ===")
                    .WithCurrentTimestamp()
                    .WithFooter($"Whitelisted by {Context.User}", Context.User.GetAvatarUrl())
                    .WithDescription($"Whitelisted user: **{username}#{discrim}** \nReason: {reason}");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
            else
            {
                await ReplyAsync($"User is not blacklisted.");
            }
        }
        [Command("blacklists")]
        [RequireOwner]
        public async Task Blacklists()
        {
            var accounts = _globalUserAccounts.GetAllAccounts();
            var emb = new EmbedBuilder()
                           .WithTitle("Blacklisted users");
            foreach (var acc in accounts.ToList())
            {
                
                if (acc.Blacklisted == true)
                {
                    var username = Context.Client.GetUser(acc.Id).Username;
                    var discrim = Context.Client.GetUser(acc.Id).Discriminator;
                    emb.WithColor(new Color(0,0,0));
                    emb.AddField($"{username}#{discrim}",$"ID: {acc.Id}", false);
                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                    return;
                }
            }
            emb.WithColor(Color.Green);
            emb.Description = "No users are blacklisted at the moment.";
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }
        [Command("logs")]
        [Summary("Show KillerBot logs")]
        [RequireOwner]
        public async Task ShowLogs()
        {
            var folder = Constants.LogFolder;
            var fileName = "Logs2.log";
            await ReplyAsync("<a:KBtick:580851374070431774> The logs file was sent to your DMs!");
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            await dmChannel.SendFileAsync($"{folder}/{fileName}");
        }

        [Command("user-data"), Alias("userdata")]
        [RequireOwner]
        public async Task GetAccountFile(ulong id)
        {
            var userFilePath = _globalUserAccounts.GetAccountFilePath(id);
            if (String.IsNullOrEmpty(userFilePath))
            {
                await Context.Channel.SendMessageAsync("I don't have any information about them.");
                return;
            }

            await Context.User.SendFileAsync(userFilePath, $"This is the information I have on them.");
            await Context.Channel.SendMessageAsync($"{Context.User.Mention} DM sent!");
        }

        [Command("add-coins"),Alias("add-money")]
        [Summary("Adds specified money to a specific user. Bot owner only.")]
        [RequireOwner]
        public async Task addcoins(ulong user,string source ,long amount = 0)
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
        public async Task removecoins(ulong user, string source, long amount = 0)
        {
            if (amount == 0)
                throw new ArgumentException("Amount specified is can't be 0");
            var account = _globalUserAccounts.GetById(user);

            if ((source == "wallet" & amount > account.Coins) || (source == "bank" & amount > account.BankCoins))
                throw new ArgumentException($"Amount specified is not available ({amount} > {account.Coins})");
            if ((source == "bank" & amount > account.BankCoins))
                throw new ArgumentException($"Amount specified is not available ({amount} > {account.BankCoins})");
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
