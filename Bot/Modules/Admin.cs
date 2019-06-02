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
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using Bot.Entities;
using Bot.Features.GlobalAccounts;

namespace Bot.Modules
{
    public class Admin : ModuleBase<MiunieCommandContext>
    {
        private static readonly OverwritePermissions denyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);
        private int _fieldRange = 10;
        private CommandService _service;
        private readonly GlobalGuildAccounts _globalGuildAccounts;

        public Admin(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }



        [Command("purge", RunMode = RunMode.Async)]
        [Remarks("Purges An Amount Of Messages")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Clear(int amountOfMessagesToDelete)
        {
            try
            {
                var messages = await Context.Channel.GetMessagesAsync(amountOfMessagesToDelete + 1).FlattenAsync();
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                var m = await ReplyAsync($"Deleted {amountOfMessagesToDelete} Messages 👌");
                await Task.Delay(5000);
                await m.DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't delete message on {Context.Guild.Name}, Error: {ex.Message}");
                await ReplyAsync($"<:KBfail:580129304592252995> Error: {ex.Message}");
            }
        
    }


        [Command("purge")]
        [Remarks("Purges A User's Last Messages. Default Amount To Purge Is 100")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Clear(SocketGuildUser user, int amountOfMessagesToDelete = 100)
        {
            if (user == Context.User)
                amountOfMessagesToDelete++; //Because it will count the purge command as a message

            var messages = await Context.Message.Channel.GetMessagesAsync(amountOfMessagesToDelete).FlattenAsync();

            var result = messages.Where(x => x.Author.Id == user.Id && x.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14)));

            await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(result);
            var m = await ReplyAsync($"Deleted {amountOfMessagesToDelete} Messages 👌");
            await Task.Delay(5000);
            await m.DeleteAsync();
        }

        [Command("Kick"), Summary("Kick @Username This is a reason"), Remarks("Kicks a user from the guild")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickAsync([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy] SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user! <:KBfail:580129304592252995>");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }

            var embed = new EmbedBuilder()
                .WithTitle("===== Kicked User =====")
                .WithDescription($"**Username: **{user.Username} || {user.Discriminator}\n**Reason: **{reason}")
                .WithColor(new Color(232, 226, 53))
                .WithFooter(x =>
                {
                    x.Text = $"Kicked by {Context.User}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                });
            //var ModLog = await Context.Client.GetChannelAsync(log.ModLogChannelId) as ITextChannel;
            //await ModLog.SendMessageAsync("", embed: embed);
            try
            {
                var embed2 = new EmbedBuilder();
                embed2.Description = ($"You've been kicked from **{Context.Guild.Name}** for **{reason}**.");
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync("", false, embed2.Build());

            }
            catch (HttpException ignored) when (ignored.DiscordCode == 50007) { }
            await user.KickAsync($"{Context.User}: {reason}");
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT KICKED*** :ok_hand: ");
            await Context.Channel.SendMessageAsync("", false, embed.Build()); ///sends embed///
            Console.WriteLine($"{DateTime.Now}: {user} was kicked in {Context.Guild}");
            
        }
       
        /* [Command("AddRole")]
         [Remarks("Usage: |prefix|addrole {@user} {roleName}")]
         [RequireBotPermission(GuildPermission.ManageRoles)]
         [RequireUserPermission(GuildPermission.ManageRoles)]
         public async Task AddRoleAsync(SocketGuildUser user, [Remainder] SocketRole role)
         {

             await user.AddRoleAsync(role);
             await ReplyAsync($"Added **{role}** to {user.Mention}!");

         } 

         [Command("RemoveRole")]
         [Remarks("Usage: |prefix|addrole {@user} {roleName}")]
         [RequireBotPermission(GuildPermission.ManageRoles)]
         [RequireUserPermission(GuildPermission.ManageRoles)]
         public async Task RemoveRoleAsync([RequireBotHigherHirachy][RequireUserHierarchy]SocketGuildUser user, [Remainder] SocketRole role)
         {
             await user.RemoveRoleAsync(role);
             await ReplyAsync($"Removed **{role}** from {user.Mention}!");

         } */

        [Command("mute")]
        [Remarks("Mutes A User")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task Mute([NoSelf][RequireBotHigherHirachy] SocketGuildUser user = null, [Remainder] string reason = null)
        {

            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }

            var muteRole = await GetMuteRole(user.Guild);
            if (!user.Roles.Any(r => r.Id == muteRole.Id))
                await user.AddRoleAsync(muteRole).ConfigureAwait(false);
            var usr = Context.Guild.GetUser(user.Id);

            // await (usr as IGuildUser).ModifyAsync(x => x.Mute = true);
            await ReplyAsync($"**{user.Username}** has been muted. <a:KBtick:580851374070431774>");
            var embed = new EmbedBuilder();
            embed.Color = new Color(206, 47, 47);
            embed.Title = "=== Muted User ===";
            embed.Description = $"**Username: ** {user.Username} || {user.Discriminator}\n**Muted by: ** {Context.User}\n**Reason: **{reason}";
            await ReplyAsync("", false, embed.Build());

                var embed2 = new EmbedBuilder();
                embed2.Description = ($"You've been muted from **{Context.Guild.Name}** for **{reason}**.");
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync("", false, embed2.Build());

    }
        [Command("unmute")]
        [Remarks("Unmutes A User")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task Unmute([NoSelf][RequireBotHigherHirachy] SocketGuildUser user)
        {
            //await Context.Guild.GetUser(user.Id).ModifyAsync(x => x.Mute = false).ConfigureAwait(false);

            await user.RemoveRoleAsync(await GetMuteRole(user.Guild)).ConfigureAwait(false); 
            await ReplyAsync("User has been unmuted. <a:KBtick:580851374070431774> ");
        }

        [Command("Ban"), Summary("Usage: Ban @Username {Days to prune messages} Reason"), Remarks("Bans a user from the guild")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy] SocketGuildUser user = null, int pruneDays = 0, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user! <:KBfail:580129304592252995>");
            if (pruneDays == null)
                throw new ArgumentException("You must mention how many days of the user's messages you would like to prune.");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }
            //  if (string.IsNullOrWhiteSpace(reason))
            //      throw new ArgumentException("You must provide a reason");

            var gld = Context.Guild as SocketGuild;

            var embed = new EmbedBuilder();
            embed.Color = new Color(206, 47, 47);
            embed.Title = "=== Banned User ===";
            embed.Description = $"**Username: ** {user.Username} || {user.Discriminator}\n**Banned by: ** {Context.User}\n**Reason: **{reason}";
            embed.ImageUrl = "https://i.redd.it/psv0ndgiqrny.gif";
            //var ModLog = await Context.Client.GetChannelAsync(log.ModLogChannelId) as ITextChannel;
            //await ModLog.SendMessageAsync("", embed: embed);   
            try
            {
                var embed2 = new EmbedBuilder();
                embed2.Description = ($"You've been banned from **{Context.Guild.Name}** for **{reason}**.");
                    var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync("", false, embed2.Build());

            }
            catch (HttpException ignored) when (ignored.DiscordCode == 50007) { }
            await gld.AddBanAsync(user, pruneDays, $"{Context.User}: {reason}");
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT BANNED*** :hammer: ");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
            Console.WriteLine($"{DateTime.Now}: {user} was banned in {Context.Guild}");
        }
        [Command("unban")]
        [Remarks("Unban A User")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Unban([Remainder]string user)
        {
            var bans = await Context.Guild.GetBansAsync();

            var theUser = bans.FirstOrDefault(x => x.User.ToString().ToLowerInvariant() == user.ToLowerInvariant());

            await Context.Guild.RemoveBanAsync(theUser.User).ConfigureAwait(false);
        }

        [Command("unban")]
        [Remarks("Unban A User")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Unban(ulong id)
        {
            var bans = await Context.Guild.GetBansAsync();

            var theUser = bans.FirstOrDefault(x => x.User.Id == id);

            await Context.Guild.RemoveBanAsync(theUser.User);
            await ReplyAsync($"The user of ID `{id}` has been unbanned. <a:KBtick:580851374070431774> ");

        }

        [Command("changenick")]
        [Remarks("Set A User's Nickname")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ManageNicknames)]
        public async Task Nickname([RequireUserHierarchy][RequireBotHigherHirachy]SocketGuildUser username, [Remainder]string name)
        {
            await Context.Guild.GetUser(username.Id).ModifyAsync(x => x.Nickname = name);
            await ReplyAsync(" <a:KBtick:580851374070431774> Nickname changed. ");
        }

        [Command("createtext")]
        [Remarks("Make A Text Channel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task Text(string channelname)
        {
            await Context.Guild.CreateTextChannelAsync(channelname);
            await ReplyAsync("Text channel was created. <a:KBtick:580851374070431774> ");
        }

        [Command("createvoice")]
        [Remarks("Make A Voice Channel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task Voice([Remainder]string channelname)
        {
            await Context.Guild.CreateVoiceChannelAsync(channelname);
            await ReplyAsync("Voice channel was created. <a:KBtick:580851374070431774> ");
        }
       

        [Command("announce")]
        [Remarks("Make A Announcement")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        public async Task Announce([Remainder]string announcement)
        {
            var embed = EmbedHandler.CreateEmbed("Announcement By " + Context.Message.Author, announcement, EmbedHandler.EmbedMessageType.Info, true);

            await Context.Channel.SendMessageAsync("", false, embed);
            await Context.Message.DeleteAsync();
        }

        
        public async Task<IRole> GetMuteRole(IGuild guild)
        {
            const string defaultMuteRoleName = "Muted";

            var muteRoleName = "Muted";

            var muteRole = guild.Roles.FirstOrDefault(r => r.Name == muteRoleName);

            if (muteRole == null)
            {
                try
                {
                    muteRole = await guild.CreateRoleAsync(muteRoleName, GuildPermissions.None).ConfigureAwait(false);
                }
                catch
                {
                    muteRole = guild.Roles.FirstOrDefault(r => r.Name == muteRoleName) ?? await guild.CreateRoleAsync(defaultMuteRoleName, GuildPermissions.None).ConfigureAwait(false);
                }
            }

            foreach (var toOverwrite in (await guild.GetTextChannelsAsync()))
            {
                try
                {
                    if (!toOverwrite.PermissionOverwrites.Any(x => x.TargetId == muteRole.Id && x.TargetType == PermissionTarget.Role))
                    {
                        await toOverwrite.AddPermissionOverwriteAsync(muteRole, denyOverwrite)
                                .ConfigureAwait(false);

                        await Task.Delay(200).ConfigureAwait(false);
                    }
                }
                catch
                {

                }
            }

            return muteRole;
        }

        //WARN SYSTEM
        [Command("warn")]
        [Alias("strike")]
        [Summary("Direct message a user with a warning")]
        [Remarks("If you warn reason is a sentence you should put `\"(reason)\"`")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Warn(SocketGuildUser user = null, string reason = null)
        {
           
            //If no reason is provided
            if (reason == null)
            {
                reason = "[No reason was provided]";
            }
            //If no user is mentioned
            if (user == null)
                throw new ArgumentException("You must mention a user.");

            //If we're able to dm the user
            EmbedBuilder success = new EmbedBuilder()
                .WithDescription($"Successfully sent a warning to **{user.Username}#{user.Discriminator}**!")
                .WithColor(Color.Green)
                .WithFooter($"Warning by: {Context.User}", Context.User.GetAvatarUrl())
                .WithCurrentTimestamp();

            //if unable to dm the user
            EmbedBuilder error = new EmbedBuilder()
                .WithDescription("User was given a warning but not DMed. Unable to send this user a Direct Message.")
                .WithColor(Color.Orange)
                .WithFooter($"Warning by: {Context.User}", Context.User.GetAvatarUrl())
                .WithTitle("Error:")
                .WithCurrentTimestamp();


            EmbedBuilder output = new EmbedBuilder()  //warn message

                .WithTitle("Warning received!")
                .WithDescription($"**{Context.Guild.Name}** has issued you a server warning!")
                .AddField($"Sender: ", Context.User.Username + "#" + Context.User.Discriminator)
                .AddField($"Server Owner: ", Context.Guild.Owner.Username + "#" + Context.Guild.Owner.Discriminator)
                .AddField($"Warning message: ", reason)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTimestamp(DateTime.Now)
                .WithColor(Color.Red);
           
            var dm = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
            if (dm == null)
                
                await ReplyAsync("",false, error.Build());
            else try
            {
                await dm.SendMessageAsync("",false, output.Build()).ConfigureAwait(false);
                await ReplyAsync("",false, success.Build()).ConfigureAwait(false);
            }
                catch
                {
                    await ReplyAsync("", false, error.Build()).ConfigureAwait(false);
                }
            var server = Context.Guild.Name;
            var warned_user = $"{user.Username}#{user.Discriminator} \n({user.Id})";
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
            var moderator = $"{Context.User.Username}#{Context.User.Discriminator}";
            var time = DateTime.Now;
            var newWarn = new WarnEntry(moderator,warned_user,time, reason);
            guild.Warns.Add(newWarn);
            var warnings = guild.Warns.ToList();
            guild.Modify(g => g.SetWarns(warnings), _globalGuildAccounts);            

        }

        [Command("warnings"),Alias("strikes","warnlist","strikelist","listwarns","liststrikes")]
        [Summary("Lists the strikes/warnings of a user. Only staff can use it.")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ListWarns()
        {
           
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
            var Warns = _globalGuildAccounts.GetById(Context.Guild.Id).Warns;
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle("Warnings:")
                .WithFooter($"Requested by: {Context.User}", Context.User.GetAvatarUrl())
                .WithCurrentTimestamp()
                .WithDescription($"Count: {guild.Warns.Count} \n \n");
            for (var i = 0; i < Warns.Count; i++)
            {
                embed.AddField($"[{i + 1}] {Warns[i].Warned_user:f}", $"By: {Warns[i].Moderator} \nReason: {Warns[i].Reason} \nDate: {Warns[i].Time}", true);
            }
            await ReplyAsync("",false, embed.Build());

        }
       
        
        [Command("removewarn")]
        [Alias("removestrike", "deletestrike","deletewarn")]
        [Summary("Direct message a user with a warning")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task RemoveWarn(int i)
        {
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
            var Warns = _globalGuildAccounts.GetById(Context.Guild.Id).Warns;
            var responseString = "Specified warning doesn't exist, make sure to do `k!warnings` before trying to " +
                                "delete a warning.";
            if (i > 0 && i <= Warns.Count)
            {
                Warns.RemoveAt(i - 1);
                var warnings = guild.Warns.ToList();
                guild.Modify(g => g.SetWarns(warnings), _globalGuildAccounts);
                responseString = $"Deleted the warning with index **{i}**!";
            }

            await ReplyAsync(responseString);
        }

        //end
    }
}
