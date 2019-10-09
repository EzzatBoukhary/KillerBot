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
using Bot.Helpers;

namespace Bot.Modules
{
    public class moderation : ModuleBase<MiunieCommandContext>
    {
        private static readonly OverwritePermissions denyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);
        private int _fieldRange = 10;
        private CommandService _service;
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        private readonly Logger _logger;
        public moderation(GlobalGuildAccounts globalGuildAccounts, Logger logger)
        {
            _globalGuildAccounts = globalGuildAccounts;
            _logger = logger;
        }



        [Command("purge", RunMode = RunMode.Async)]
        [Remarks("Purges An Amount Of Messages")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Clear(
            [Summary("Amount of messages you want to delete from the channel")]int amountOfMessagesToDelete)
        {
            try
            {
                await Context.Message.DeleteAsync();
                var messages = await Context.Channel.GetMessagesAsync(amountOfMessagesToDelete).FlattenAsync();
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                var m = await ReplyAsync($"Deleted {amountOfMessagesToDelete} Messages 👌");
                await Task.Delay(5000);
                await m.DeleteAsync();
            }
            catch (Exception ex)
            {
                await _logger.Log(LogSeverity.Warning, "Error", $"Couldn't delete message in {Context.Guild.Name} for: {ex.Message}");
                await ReplyAsync($"<:KBfail:580129304592252995> Error: {ex.Message}");
            }
        
    }

        [Command("purge")]
        [Remarks("Purges A User's Last Messages. Default Amount To Purge Is 100")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Clear(
            [Summary("OPTIONAL: The user you want to delete their messages")]SocketGuildUser user,
            [Summary("Amount of messages you want to delete")]int amountOfMessagesToDelete = 100)
        {
            await Context.Message.DeleteAsync();
            if (user == Context.User)
                amountOfMessagesToDelete++; //Because it will count the purge command as a message

            var messages = await Context.Message.Channel.GetMessagesAsync(amountOfMessagesToDelete).FlattenAsync();

            var result = messages.Where(x => x.Author.Id == user.Id && x.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14)));

            try
            {
                await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(result);
                var m = await ReplyAsync($"Deleted {amountOfMessagesToDelete - 1} Messages 👌");
                await Task.Delay(5000);
                await m.DeleteAsync();
            }
            catch (Exception ex)
            {
                await _logger.Log(LogSeverity.Warning, "Error", $"Couldn't delete message in {Context.Guild.Name} for: {ex.Message}");
                await ReplyAsync($"<:KBfail:580129304592252995> Error: {ex.Message}");
            }
        }

        [Command("Kick"), Summary("Kick @Username This is a reason"), Remarks("Kicks a user from the guild")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickAsync([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy]
        [Summary("The user you want to kick")]SocketGuildUser user = null, [Remainder]
        [Summary("OPTIONAL: The reason behind the kick")]string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user! <:KBfail:580129304592252995>");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }

            var embed = new EmbedBuilder()
                .WithTitle("===== Kicked User =====")
                .WithDescription($"**Kicked User: ** {user.Username}#{user.Discriminator} || {user.Id} \n**Kicked by: ** {Context.User} \n**Reason: **{reason}")
                .WithColor(new Color(232, 226, 53))
                .WithCurrentTimestamp();
            /* var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
            if (guild.ServerActivityLog == 1)
            {
                var Modlog = Context.Guild.GetTextChannel(guild.LogChannelId);
                await Modlog.SendMessageAsync("",false, embed.Build());
            } */
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
            await Context.Channel.SendMessageAsync("", false, embed.Build());
            
        }
       
         [Command("AddRole")]
         [Remarks("Usage: k!addrole {@user/ user's name (NO SPACES)} {@role/ roleName}")]
         [RequireBotPermission(GuildPermission.ManageRoles)]
         [RequireUserPermission(GuildPermission.ManageRoles)]
         public async Task AddRoleAsync([RequireBotHigherHirachy][RequireUserHierarchy]SocketGuildUser user, [Remainder] SocketRole role)
         {
            var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(exec);
            if (highestRole == null)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            if ((role != null) && (highestRole.Position == role.Position))
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");

            if ((role != null) && (highestRole.Position < role.Position))
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            await user.AddRoleAsync(role);
             await ReplyAsync($"Added **{role}** to {user.Mention}!");

         }

        [Command("AddRole")]
        [Remarks("Usage: k!addrole {@role/ rolename (NO SPACES)} {@user/ user's name}")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddRoleAsync([RequireBotHigherHirachy][RequireUserHierarchy] SocketRole role, [Remainder] SocketGuildUser user)
        {
            var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(exec);
            if (highestRole == null)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            if ((role != null) && (highestRole.Position == role.Position))
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");

            if ((role != null) && (highestRole.Position < role.Position))
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            await user.AddRoleAsync(role);
            await ReplyAsync($"Added **{role}** to {user.Mention}!");

        }

        [Command("RemoveRole")]
         [Remarks("Usage: k!addrole {@user/ user's name (NO SPACES)} {@role/ roleName}")]
         [RequireBotPermission(GuildPermission.ManageRoles)]
         [RequireUserPermission(GuildPermission.ManageRoles)]
         public async Task RemoveRoleAsync([RequireBotHigherHirachy][RequireUserHierarchy]SocketGuildUser user, [Remainder] SocketRole role)
         {
            var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(exec);
            if (highestRole == null)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            if ((role != null) && (highestRole.Position == role.Position))
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");

            if ((role != null) && (highestRole.Position < role.Position))
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            await user.RemoveRoleAsync(role);
             await ReplyAsync($"Removed **{role}** from {user.Mention}!");

         }

        [Command("RemoveRole")]
        [Remarks("Usage: k!addrole {rolename/@role (NO SPACES)} {@user/ user's name}")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRoleAsync([RequireBotHigherHirachy][RequireUserHierarchy]SocketRole role, [Remainder] SocketGuildUser user)
        {
            var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(exec);
            if (highestRole == null)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            if ((role != null) && (highestRole.Position == role.Position))
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");

            if ((role != null) && (highestRole.Position < role.Position))
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            await user.RemoveRoleAsync(role);
            await ReplyAsync($"Removed **{role}** from {user.Mention}!");

        }

        [Command("mute")]
        [Remarks("Mutes A User")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task Mute([NoSelf][RequireUserHierarchy][RequireBotHigherHirachy]
        [Summary("The user you want to mute")]SocketGuildUser user = null,
            [Summary("OPTIONAL: The reason behind the mute")][Remainder] string reason = null)
        {

            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }

            var muteRole = await GetMuteRole(user.Guild);
            
            if (!user.Roles.Any(r => r.Id == muteRole.Id))
            {
                var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
                IRole exec_role = DiscordHelpers.GetUsersHigherstRole(exec);
                if (exec_role == null)
                    throw new ArgumentException("You don't have enough permissions due to role hierarchy");
                if ((muteRole != null) && (exec_role.Position < muteRole.Position))
                    throw new ArgumentException("You don't have enough permissions due to role hierarchy");

                await user.AddRoleAsync(muteRole, options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" }).ConfigureAwait(false);
                var usr = Context.Guild.GetUser(user.Id);

                // await (usr as IGuildUser).ModifyAsync(x => x.Mute = true);
                await ReplyAsync($"**{user.Username}** has been muted. <a:KBtick:580851374070431774>");
                var embed = new EmbedBuilder();
                embed.Color = new Color(206, 47, 47);
                embed.Title = "=== Muted User ===";
                embed.Description = $"**User: ** {user.Username}#{user.Discriminator} || {user.Discriminator} \n**Muted by: ** {Context.User}\n**Reason: **{reason}";
                await ReplyAsync("", false, embed.Build());

                try
                {
                    var embed2 = new EmbedBuilder();
                    embed2.Description = ($"You've been muted from **{Context.Guild.Name}** for **{reason}**.");
                    var dmChannel = await user.GetOrCreateDMChannelAsync();
                    await dmChannel.SendMessageAsync("", false, embed2.Build());
                }
                catch (HttpException ignored) when (ignored.DiscordCode == 50007) { }
            }
            else
            {
                await ReplyAsync($"**{user.Username}#{user.Discriminator}** is already muted.");
            }
        }
        //=====mute with time=====
        [Command("tempmute")]
        [Remarks("Mutes a user for a limited time")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task MuteTime([NoSelf][RequireUserHierarchy][RequireBotHigherHirachy]
        [Summary("The user you want to tempmute")]SocketGuildUser user = null,
            [Summary("Time in minutes you want to mute the user for")]int time = 1,
            [Summary("OPTIONAL: The reason behind the tempmute")][Remainder] string reason = null)
        {

            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }

            var muteRole = await GetMuteRole(user.Guild);
            var embed = new EmbedBuilder();
            embed.Color = new Color(206, 47, 47);
            embed.Title = "=== Muted User ===";
            embed.Description = $"**User: ** {user.Username}#{user.Discriminator} || {user.Discriminator} \n**Muted by: ** {Context.User}\n**Reason: **{reason}\n**Duration: **{time}m";
            

            var embed2 = new EmbedBuilder();
            embed2.Description = ($"You've been muted for {time}m from **{Context.Guild.Name}**: **{reason}**");
            var dmChannel = await user.GetOrCreateDMChannelAsync();

            if (time == 0)
            {
                var use = await Context.Channel.SendMessageAsync("Time has to be more than 0! \nUsage:``k!mute @user {time in minutes} {reason}`` reason is optional.");
                await Task.Delay(5000);
                await use.DeleteAsync();
            }
            else
            {
                if (!user.Roles.Any(r => r.Id == muteRole.Id))
                {
                    var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
                    IRole exec_role = DiscordHelpers.GetUsersHigherstRole(exec);
                    if (exec_role == null)
                        throw new ArgumentException("You don't have enough permissions due to role hierarchy");
                    if ((muteRole != null) && (exec_role.Position < muteRole.Position))
                        throw new ArgumentException("You don't have enough permissions due to role hierarchy");

                    await user.AddRoleAsync(muteRole, options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" }).ConfigureAwait(false);
                    await ReplyAsync($"**{user.Username}** has been muted. <a:KBtick:580851374070431774>");
                    await ReplyAsync("", false, embed.Build());
                    try
                    {
                        await dmChannel.SendMessageAsync("", false, embed2.Build());
                    }
                    catch (HttpException ignored) when (ignored.DiscordCode == 50007) { }
                    await Task.Delay(time * 60000);
                    await user.RemoveRoleAsync(await GetMuteRole(user.Guild), options: new RequestOptions { AuditLogReason = $"[Temp-mute time done]" }).ConfigureAwait(false);
                    await ReplyAsync($"**{user}** has been unmuted. <a:KBtick:580851374070431774> ");
                }
                else
                {
                    await ReplyAsync($"**{user.Username}#{user.Discriminator}** is already muted.");
                }
            }

        }
       
        //======end of time mute======


        [Command("unmute")]
        [Remarks("Unmutes A User")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task Unmute([NoSelf][RequireUserHierarchy][RequireBotHigherHirachy][Summary("REQUIRED: The user you want to unmute")] SocketGuildUser user = null, [Summary("OPTIONAL: The reason behind the tempmute")] [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("Please mention a user to unmute");

            if (string.IsNullOrEmpty(reason))
                reason = "[No reason was provided]";

            //await Context.Guild.GetUser(user.Id).ModifyAsync(x => x.Mute = false).ConfigureAwait(false);
            var muteRole = await GetMuteRole(user.Guild);
            if (user.Roles.Any(r => r.Id == muteRole.Id))
            {
                var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
                IRole exec_role = DiscordHelpers.GetUsersHigherstRole(exec);
                if (exec_role == null)
                    throw new ArgumentException("You don't have enough permissions due to role hierarchy");
                if ((muteRole != null) && (exec_role.Position < muteRole.Position))
                    throw new ArgumentException("You don't have enough permissions due to role hierarchy");

                await user.RemoveRoleAsync(await GetMuteRole(user.Guild), options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" }).ConfigureAwait(false);
                await ReplyAsync($"<a:KBtick:580851374070431774> **{user}** has been unmuted by **{Context.User}** for **{reason}**");
            }
            else
                throw new ArgumentException("User is not muted");
        }

        [Command("Ban"), Summary("Usage: Ban @Username {Days to prune messages} Reason"), Remarks("Bans a user from the guild")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy] [Summary("The user you want to ban")] SocketGuildUser user = null,
          [Summary("OPTIONAL: The amount of days of messages you want to delete from that user")]  int pruneDays = 0,
           [Summary("OPTIONAL: The Reason behind the ban")] [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user! <:KBfail:580129304592252995>");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }
            var gld = Context.Guild as SocketGuild;

            var embed = new EmbedBuilder();
            embed.Color = new Color(206, 47, 47);
            embed.Title = "=== Banned User ===";
            embed.Description = $"**Banned User: ** {user.Username}#{user.Discriminator} || {user.Id} \n**Banned by: ** {Context.User}\n**Reason: **{reason}";
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
        }

        //Ban with no prune days
        [Command("Ban"), Summary("Usage: Ban @Username {Days to prune messages} Reason"), Remarks("Bans a user from the guild")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync2([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy] [Summary("The user you want to ban")]SocketGuildUser user = null,
           [Summary("OPTIONAL: The reason behind the ban")] [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user! <:KBfail:580129304592252995>");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }

            var gld = Context.Guild as SocketGuild;

            var embed = new EmbedBuilder();
            embed.Color = new Color(206, 47, 47);
            embed.Title = "=== Banned User ===";
            embed.Description = $"**Banned User: ** {user.Username}#{user.Discriminator} || {user.Id}\n**Banned by: ** {Context.User}\n**Reason: **{reason}";
            embed.ImageUrl = "https://i.redd.it/psv0ndgiqrny.gif";  
            try
            {
                var embed2 = new EmbedBuilder();
                embed2.Description = ($"You've been banned from **{Context.Guild.Name}** for **{reason}**.");
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync("", false, embed2.Build());

            }
            catch (HttpException ignored) when (ignored.DiscordCode == 50007) { }
            await gld.AddBanAsync(user, 0, $"{Context.User}: {reason}");
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT BANNED*** :hammer: ");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("unban")]
        [Remarks("Unbans a user. However if you want the user's name has spaces I'd recommend using the ID method (you need to enable developer mode)")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task UnbanName([Summary("The name of the user you want to unban")] string user = null, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("Please enter the the username#discriminator/username/ID of the user you want to unban.");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }
            var bans = await Context.Guild.GetBansAsync();
     
            var theUser = bans.FirstOrDefault(x => x.User.ToString().ToLowerInvariant() == user.ToLowerInvariant());
            var UsernameUser = bans.FirstOrDefault(x => x.User.Username.ToString().ToLowerInvariant() == user.ToLowerInvariant());
            if (theUser == null)
                theUser = UsernameUser;
            if (UsernameUser == null)
                throw new ArgumentException("User not found. Please enter the the username#discriminator/username/ID of the user you want to unban.");
        
            if (user == $"{theUser.User.Username}#{theUser.User.Discriminator}")
            {
                await Context.Guild.RemoveBanAsync(theUser.User, options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" }).ConfigureAwait(false);
                await ReplyAsync($"{user} has been unbanned. <a:KBtick:580851374070431774>");
            }
            else if (user == $"{UsernameUser.User.Username}")
            {
                await Context.Guild.RemoveBanAsync(UsernameUser.User, options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" }).ConfigureAwait(false);
                await ReplyAsync($"{user} has been unbanned. <a:KBtick:580851374070431774>");
            }
            
        }

        [Command("unban")]
        [Remarks("Unban a user with their ID. (If you don't know how search it up or use the name method)")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Unban([Summary("The ID of the user you want to unban")]ulong id, [Remainder] string reason = null)
        {
            var bans = await Context.Guild.GetBansAsync();

            var theUser = bans.FirstOrDefault(x => x.User.Id == id);
            if (theUser == null)
            {
                throw new ArgumentException("User not found. Please enter the the username#discriminator/username/ID of the user you want to unban.");
            }
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }
            await Context.Guild.RemoveBanAsync(theUser.User, options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" });
            await ReplyAsync($"The user of ID `{id}` has been unbanned. <a:KBtick:580851374070431774> ");
             
        }

        [Command("changenick"), Alias("setnick", "change-nick")]
        [Remarks("Set A User's Nickname")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ManageNicknames)]
        public async Task Nickname([RequireUserHierarchy][RequireBotHigherHirachy]SocketGuildUser username = null, [Remainder]string name = null)
        {
            if (username == null)
                throw new ArgumentException("Please mention the user you want to change their nickname");
            if (name == null)
                throw new ArgumentException("You should specify the new nickname you need to set to the user");
            await Context.Guild.GetUser(username.Id).ModifyAsync(x => x.Nickname = name);
            await ReplyAsync(" <a:KBtick:580851374070431774> Nickname changed. ");
        }

        [Command("createtext")]
        [Remarks("Make A Text Channel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task Text(string channelname = null)
        {
            if (channelname == null)
                throw new ArgumentException("Please type the name of the channel you want to create");
            await Context.Guild.CreateTextChannelAsync(channelname);
            await ReplyAsync("Text channel was created. <a:KBtick:580851374070431774> ");
        }

        [Command("createvoice")]
        [Remarks("Make A Voice Channel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task Voice([Remainder]string channelname = null)
        {
            if (channelname == null)
                throw new ArgumentException("Please type the name of the channel you want to create");
            await Context.Guild.CreateVoiceChannelAsync(channelname);
            await ReplyAsync("Voice channel was created. <a:KBtick:580851374070431774> ");
        }
       

        [Command("announce")]
        [Remarks("Make A Announcement")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        public async Task Announce([Remainder]string announcement = null)
        {
            if (announcement == null)
                throw new ArgumentException("Please type your announcement");
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
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Warn([RequireUserHierarchy][Summary("The user you want to warn/strike")]SocketGuildUser user = null, 
           [Summary("OPTIONAL: The reason behind the warning")] [Remainder] string reason = null)
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
                embed.AddField($"\n[{i + 1}] {Warns[i].Warned_user:f}", $"By: {Warns[i].Moderator} \nReason: {Warns[i].Reason} \nDate: {Warns[i].Time}", false);
            }
            await ReplyAsync("",false, embed.Build());

        }
       
        
        [Command("removewarn")]
        [Alias("removestrike", "deletestrike","deletewarn", "remove-warn", "delete-warn", "remove-strike", "delete-strike", "clear-warn", "clear-strike")]
        [Summary("Direct message a user with a warning")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task RemoveWarn([Summary("The index of the warning/strike you want to delete")]int i)
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
