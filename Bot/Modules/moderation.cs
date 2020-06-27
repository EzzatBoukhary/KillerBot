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
using System.Globalization;

namespace Bot.Modules
{
    public class moderation : InteractiveBase
    {
        private static readonly OverwritePermissions denyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);
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
        [Example("k!purge 30")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task Clear(
            [Summary("Amount of messages you want to delete from the channel")]string amountOfMessagesToDelete = null)
        {
            int msgs;
            if (amountOfMessagesToDelete == null || !int.TryParse(amountOfMessagesToDelete, out msgs))
            {
                await ReplyAsync($"{Constants.fail} Please enter the amount of messages you want to delete.");
                return;
            }
            try
            {
                var messages = await Context.Channel.GetMessagesAsync(msgs + 1).FlattenAsync();
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                var m = await ReplyAsync($"Deleted {messages.Count() - 1} Messages 👌");
                await Task.Delay(3000);
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
        [Example("k!purge @Panda#8822 15")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task Clear(
            [Summary("OPTIONAL: The user you want to delete their messages")]SocketGuildUser user,
            [Summary("Amount of messages you want to delete")]string amountOfMessagesToDelete = null)
        {
            int msgs;
            if (!int.TryParse(amountOfMessagesToDelete, out msgs))
            {
                await ReplyAsync($"{Constants.fail} Please enter the amount of messages you want to delete.");
                return;
            }
            await Context.Message.DeleteAsync();

            var messages = await Context.Message.Channel.GetMessagesAsync().FlattenAsync();

            var result = messages.Where(x => x.Author.Id == user.Id && x.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14)));
            var result2 = result.Take(msgs);
            try
            {
                await (Context.Message.Channel as SocketTextChannel).DeleteMessagesAsync(result2);
                var m = await ReplyAsync($"Deleted {result2.Count()} Messages 👌");
                await Task.Delay(3000);
                await m.DeleteAsync();
            }
            catch (Exception ex)
            {
                await _logger.Log(LogSeverity.Warning, "Error", $"Couldn't delete message in {Context.Guild.Name} for: {ex.Message}");
                await ReplyAsync($"<:KBfail:580129304592252995> Error: {ex.Message}");
            }
        }

        [Command("Kick"), Summary("Kicks a user from the guild")]
        [Example("k!kick @Panda#8822 Good bye")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task KickAsync([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy]
        [Summary("The user you want to kick")]SocketGuildUser user = null, [Remainder]
        [Summary("OPTIONAL: The reason behind the kick")]string reason = null)
        {
            if (user == null)
                throw new ArgumentException("<:KBfail:580129304592252995> You must mention a user!");
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
        [Summary("Adds a role to a user.")]
        [Example("k!addrole @Panda#8822 Members")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task AddRoleAsync([RequireBotHigherHirachy][RequireUserHierarchy]SocketGuildUser user, [Remainder] SocketRole role)
        {
            var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(exec);
            if (highestRole == null && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            else if (highestRole == null && Context.User.Id == Context.Guild.OwnerId)
            {
                await user.AddRoleAsync(role, options: new RequestOptions { AuditLogReason = $"Added by {Context.User.Username}#{Context.User.Discriminator}" });
                await ReplyAsync($"Added **{role}** to {user.Mention}!");
                return;
            }
            if ((role != null) && (highestRole.Position == role.Position) && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");

            if ((role != null) && (highestRole.Position < role.Position) && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            await user.AddRoleAsync(role, options: new RequestOptions { AuditLogReason = $"Added by {Context.User.Username}#{Context.User.Discriminator}" });
            await ReplyAsync($"Added **{role}** to {user.Mention}!");
         }

        [Command("AddRole")]
        [Remarks("Usage: k!addrole {@role/ rolename (NO SPACES)} {@user/ user's name}")]
        [Summary("Adds a role to a user.")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddRoleAsync([RequireBotHigherHirachy][RequireUserHierarchy] SocketRole role, [Remainder] SocketGuildUser user)
        {
            var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(exec);
            if (highestRole == null && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            else if (highestRole == null && Context.User.Id == Context.Guild.OwnerId)
            {
                await user.AddRoleAsync(role, options: new RequestOptions { AuditLogReason = $"Added by {Context.User.Username}#{Context.User.Discriminator}" });
                await ReplyAsync($"Added **{role}** to {user.Mention}!");
                return;
            }
            if ((role != null) && (highestRole.Position == role.Position) && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");

            if ((role != null) && (highestRole.Position < role.Position) && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            await user.AddRoleAsync(role, options: new RequestOptions { AuditLogReason = $"Added by {Context.User.Username}#{Context.User.Discriminator}" });
            await ReplyAsync($"Added **{role}** to {user.Mention}!");

        }

        [Command("RemoveRole")]
         [Remarks("Usage: k!addrole {@user/ user's name (NO SPACES)} {@role/ roleName}")]
        [Summary("Removes a role from a user.")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
         [RequireUserPermission(GuildPermission.ManageRoles)]
         public async Task RemoveRoleAsync([RequireBotHigherHirachy][RequireUserHierarchy]SocketGuildUser user, [Remainder] SocketRole role)
         {
            var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(exec);
            if (highestRole == null && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            else if (highestRole == null && Context.User.Id == Context.Guild.OwnerId)
            {
                await user.RemoveRoleAsync(role, options: new RequestOptions { AuditLogReason = $"Removed by {Context.User.Username}#{Context.User.Discriminator}" });
                await ReplyAsync($"Removed **{role}** from {user.Mention}!");
                return;
            }
            if ((role != null) && (highestRole.Position == role.Position) && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");

            if ((role != null) && (highestRole.Position < role.Position) && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            await user.RemoveRoleAsync(role, options: new RequestOptions { AuditLogReason = $"Removed by {Context.User.Username}#{Context.User.Discriminator}" });
             await ReplyAsync($"Removed **{role}** from {user.Mention}!");

         }

        [Command("RemoveRole")]
        [Remarks("Usage: k!addrole {rolename/@role (NO SPACES)} {@user/ user's name}")]
        [Summary("Removes a role from a user.")]
        [Example("k!removerole Members Panda")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRoleAsync([RequireBotHigherHirachy][RequireUserHierarchy]SocketRole role, [Remainder] SocketGuildUser user)
        {
            var exec = (Context.Guild as SocketGuild).GetUser(Context.User.Id);
            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(exec);
            if (highestRole == null && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            else if (highestRole == null && Context.User.Id == Context.Guild.OwnerId)
            {
                await user.RemoveRoleAsync(role, options: new RequestOptions { AuditLogReason = $"Removed by {Context.User.Username}#{Context.User.Discriminator}" });
                await ReplyAsync($"Removed **{role}** from {user.Mention}!");
                return;
            }
            if ((role != null) && (highestRole.Position == role.Position) && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");

            if ((role != null) && (highestRole.Position < role.Position) && Context.User.Id != Context.Guild.OwnerId)
                throw new ArgumentException("You don't have enough permissions due to role hierarchy");
            await user.RemoveRoleAsync(role, options: new RequestOptions { AuditLogReason = $"Removed by {Context.User.Username}#{Context.User.Discriminator}" });
            await ReplyAsync($"Removed **{role}** from {user.Mention}!");

        }

        [Command("mute")]
        [Summary("Mutes A User")]
        [Example("k!mute @Panda#8822 Have fun in silence.")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireContext(ContextType.Guild)]
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
                if (exec_role == null && Context.User.Id != Context.Guild.OwnerId)
                    throw new ArgumentException("You don't have enough permissions due to role hierarchy");
                if ((muteRole != null) && (exec_role.Position < muteRole.Position) && Context.User.Id != Context.Guild.OwnerId)
                    throw new ArgumentException("You don't have enough permissions due to role hierarchy");

                await user.AddRoleAsync(muteRole, options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" }).ConfigureAwait(false);
                var usr = Context.Guild.GetUser(user.Id);

                // await (usr as IGuildUser).ModifyAsync(x => x.Mute = true);
                await ReplyAsync($"<a:SuccessKB:639875484972351508> **{user.Username}** has been muted.");
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
        [Summary("Mutes a user for a limited time")]
        [Example("k!tempmute @Panda#8822 10 I SAID NO!")]
        [RequireContext(ContextType.Guild)]
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
                    if (exec_role == null && Context.User.Id != Context.Guild.OwnerId)
                        throw new ArgumentException("You don't have enough permissions due to role hierarchy");
                    if ((muteRole != null) && (exec_role.Position < muteRole.Position) && Context.User.Id != Context.Guild.OwnerId)
                        throw new ArgumentException("You don't have enough permissions due to role hierarchy");

                    await user.AddRoleAsync(muteRole, options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" }).ConfigureAwait(false);
                    await ReplyAsync($"<a:SuccessKB:639875484972351508> **{user.Username}** has been muted.");
                    await ReplyAsync("", false, embed.Build());
                    try
                    {
                        await dmChannel.SendMessageAsync("", false, embed2.Build());
                    }
                    catch (HttpException ignored) when (ignored.DiscordCode == 50007) { }
                    await Task.Delay(time * 60000);
                    await user.RemoveRoleAsync(await GetMuteRole(user.Guild), options: new RequestOptions { AuditLogReason = $"[Temp-mute time done]" }).ConfigureAwait(false);
                    await ReplyAsync($"<a:SuccessKB:639875484972351508> **{user}** has been unmuted.");
                }
                else
                {
                    await ReplyAsync($"**{user.Username}#{user.Discriminator}** is already muted.");
                }
            }

        }

        //======end of time mute======


        [Command("unmute")]
        [Summary("Unmutes a user")]
        [Example("k!unmute @Panda#8822 You're a good boy now.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task Unmute([NoSelf][RequireUserHierarchy][RequireBotHigherHirachy][Summary("REQUIRED: The user you want to unmute")] SocketGuildUser user = null, [Summary("OPTIONAL: The reason behind the unmute")] [Remainder] string reason = null)
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
                if (exec_role == null && Context.User.Id != Context.Guild.OwnerId)
                    throw new ArgumentException("You don't have enough permissions due to role hierarchy");
                if ((muteRole != null) && (exec_role.Position < muteRole.Position) && Context.User.Id != Context.Guild.OwnerId)
                    throw new ArgumentException("You don't have enough permissions due to role hierarchy");

                await user.RemoveRoleAsync(await GetMuteRole(user.Guild), options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" }).ConfigureAwait(false);
                await ReplyAsync($"<a:SuccessKB:639875484972351508> **{user}** has been unmuted by **{Context.User}** for **{reason}**");
            }
            else
                throw new ArgumentException("User is not muted");
        }

        //forced ban with prune days
        [Command("ban"), Summary("Usage: k!ban {User ID} {Prune days} Reason"), Remarks("Force bans a user from the guild."), Example("k!ban 223530903773773824 You're not Welcome here ever.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ForceBan([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy] [Summary("The ID of user you want to ban")] ulong id,
           [Summary("OPTIONAL: The amount of days of messages you want to delete from that user")]  int pruneDays = 0,
           [Summary("OPTIONAL: The Reason behind the ban")] [Remainder] string reason = null)
        {
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }
            var gld = Context.Guild as SocketGuild;
            try
            {
                var oldbans = await Context.Guild.GetBansAsync();
                var theUser = oldbans.FirstOrDefault(x => x.User.Id == id);
                if (theUser != null)
                {
                    await ReplyAsync("<:KBfail:580129304592252995> The user is already banned!");
                    return;
                }
                await gld.AddBanAsync(id, pruneDays, $"{Context.User}: {reason}");
                var newbans = await Context.Guild.GetBansAsync();
                foreach (var ban in newbans)
                {
                    if (ban.User.Id == id)
                    {
                        var user = ban.User;
                        var embed = new EmbedBuilder();
                        embed.Color = new Color(206, 47, 47);
                        embed.Title = "=== Forced Ban ===";
                        embed.Description = $"**Banned User: ** {user.Username}#{user.Discriminator} || {user.Id} \n**Banned by: ** {Context.User}\n**Reason: **{reason} \n**Days Pruned:** {pruneDays}";
                        embed.ImageUrl = "https://i.redd.it/psv0ndgiqrny.gif";
                        await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT FORCEFULLY BANNED*** :hammer: :fire: ");
                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            catch
            {
                await Context.Channel.SendMessageAsync("Something went wrong... Is the user banned already?");
            }
        }

        [Command("ban"), Remarks("Do k!help ban about the parameters. | Usage: Ban @Username {Days to prune messages} Reason"), Summary("Bans a user from the guild."), Example("k!ban @Panda#8822 3 Being a bad boy.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task BanAsync([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy] [Summary("The user you want to ban")] SocketGuildUser user = null,
          [Summary("OPTIONAL: The amount of days of messages you want to delete from that user")]  int pruneDays = 0,
           [Summary("OPTIONAL: The Reason behind the ban")] [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("<:KBfail:580129304592252995> You must mention a user!");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }
            var gld = Context.Guild as SocketGuild;

            var embed = new EmbedBuilder();
            embed.Color = new Color(206, 47, 47);
            embed.Title = "=== Banned User ===";
            embed.Description = $"**Banned User: ** {user.Username}#{user.Discriminator} || {user.Id} \n**Banned by: ** {Context.User}\n**Reason: **{reason} \n**Days Pruned:** {pruneDays}";
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
        [Command("ban"), Remarks("Do k!help ban about the parameters. | Usage: Ban @Username {Days to prune messages} Reason"), Summary("Bans a user from the guild."), Example("k!ban @Panda#8822 Being a bad boy.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task BanAsync2([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy] [Summary("The user you want to ban")]SocketGuildUser user = null,
           [Summary("OPTIONAL: The reason behind the ban")] [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("<:KBfail:580129304592252995> You must mention a user");
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
        //forced ban with no prunedays
        [Command("ban"), Summary("Usage: k!ban {User ID} Reason"), Remarks("Force bans a user from the guild."), Example("k!ban 223530903773773824 You're not Welcome here ever.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ForceBan2([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy] [Summary("The ID of user you want to ban")] ulong id,
           [Summary("OPTIONAL: The Reason behind the ban")] [Remainder] string reason = null)
        {
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }
            var gld = Context.Guild as SocketGuild;
            try
            {
                var oldbans = await Context.Guild.GetBansAsync();
                var theUser = oldbans.FirstOrDefault(x => x.User.Id == id);
                if (theUser != null)
                {
                    await ReplyAsync("<:KBfail:580129304592252995> The user is already banned!");
                    return;
                }
                await gld.AddBanAsync(id, 0, $"{Context.User}: {reason}");
                var newbans = await Context.Guild.GetBansAsync();
                foreach (var ban in newbans)
                {
                    if (ban.User.Id == id)
                    {
                        var user = ban.User;
                        var embed = new EmbedBuilder();
                        embed.Color = new Color(206, 47, 47);
                        embed.Title = "=== Forced Ban ===";
                        embed.Description = $"**Banned User: ** {user.Username}#{user.Discriminator} || {user.Id} \n**Banned by: ** {Context.User}\n**Reason: **{reason}";
                        embed.ImageUrl = "https://i.redd.it/psv0ndgiqrny.gif";
                        await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT FORCEFULLY BANNED*** :hammer: :fire: ");
                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            catch
            {
                await Context.Channel.SendMessageAsync("Something went wrong... Is the user banned already?");
            }
        }
        [Command("unban")]
        [Remarks("Unbans a user. However if you want the user's name has spaces I'd recommend using the ID method (you need to enable developer mode)")]
        [RequireContext(ContextType.Guild)]
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
                await ReplyAsync($"<a:SuccessKB:639875484972351508> {user} has been unbanned.");
            }
            else if (user == $"{UsernameUser.User.Username}")
            {
                await Context.Guild.RemoveBanAsync(UsernameUser.User, options: new RequestOptions { AuditLogReason = $"{Context.User.Username}#{Context.User.Discriminator}: {reason}" }).ConfigureAwait(false);
                await ReplyAsync($"{Constants.success} {user} has been unbanned.");
            }
            
        }

        [Command("unban")]
        [Remarks("Unban a user with their ID. (If you don't know how search it up or use the name method)")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
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
            await ReplyAsync($"{Constants.success} **{theUser.User}** of ID ({id}) has been unbanned. ");
             
        }

        [Command("sban"), Alias("soft", "softban")]
        [Summary("Bans and unbans a user instantly. The purpose of this is to get rid of the user's messages.")]
        [Example("k!softban @Panda#8822 2 Spamming")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task SoftBan([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy][Summary("The user to softban")] SocketGuildUser user = null,
                                  [Summary("Number of days for which to prune the user's messages (1 day is default)")] int pruneDays = 1,
                                  [Summary("Reason for softban")] [Remainder] string reason = null)
                                  
        {
            if (user == null)
                throw new ArgumentException("<:KBfail:580129304592252995> You must mention a user");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }

            var gld = Context.Guild as SocketGuild;

            var embed = new EmbedBuilder();
            embed.Color = Color.Orange;
            embed.Title = "=== Soft Ban ===";
            embed.Description = $"**Soft banned User: ** {user.Username}#{user.Discriminator} || {user.Id}\n**Soft Banned by: ** {Context.User}\n**Reason: **{reason}";
            try
            {
                var embed2 = new EmbedBuilder();
                embed2.Description = ($"You've been soft banned from **{Context.Guild.Name}** for **{reason}**.");
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync("", false, embed2.Build());

            }
            catch (HttpException ignored) when (ignored.DiscordCode == 50007) { }
            await gld.AddBanAsync(user, pruneDays, $"{Context.User}: {reason}");
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT SOFT BANNED!***");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
            await Context.Guild.RemoveBanAsync(user, options: new RequestOptions { AuditLogReason = $"Soft banned by: {Context.User.Username}#{Context.User.Discriminator}" }).ConfigureAwait(false);

        }

        [Command("sban"), Alias("soft", "softban")]
        [Example("k!sban @Panda#8822 Spam harder next time.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Summary("Bans a user and immediately unbans them.")]
        public async Task SoftBan2([NoSelf][RequireBotHigherHirachy][RequireUserHierarchy][Summary("The user to softban")] SocketGuildUser user = null,
                                  [Remainder][Summary("Reason for softban")] string reason = null)

        {
            if (user == null)
                throw new ArgumentException("<:KBfail:580129304592252995> You must mention a user");
            if (string.IsNullOrEmpty(reason))
            {
                reason = "[No reason was provided]";
            }

            var gld = Context.Guild as SocketGuild;

            var embed = new EmbedBuilder();
            embed.Color = Color.Orange;
            embed.Title = "=== Soft Ban ===";
            embed.Description = $"**Soft banned User: ** {user.Username}#{user.Discriminator} || {user.Id}\n**Soft Banned by: ** {Context.User}\n**Reason: **{reason}";
            try
            {
                var embed2 = new EmbedBuilder();
                embed2.Description = ($"You've been soft banned from **{Context.Guild.Name}** for **{reason}**.");
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync("", false, embed2.Build());

            }
            catch (HttpException ignored) when (ignored.DiscordCode == 50007) { }
            await gld.AddBanAsync(user, 1, $"{Context.User}: {reason}");
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT SOFT BANNED!***");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
            await Context.Guild.RemoveBanAsync(user, options: new RequestOptions { AuditLogReason = $"Soft Banned by: {Context.User.Username}#{Context.User.Discriminator}" }).ConfigureAwait(false);

        }

        [Command("changenick"), Alias("setnick", "change-nick")]
        [Summary("Set A User's Nickname")]
        [Example("k!setnick Panda NotPanda")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ManageNicknames)]
        [RequireContext(ContextType.Guild)]
        public async Task Nickname([RequireUserHierarchy][RequireBotHigherHirachy]SocketGuildUser username = null, [Remainder]string name = null)
        {
            if (username == null)
                throw new ArgumentException("Please mention the user you want to change their nickname");
            if (name == null)
                throw new ArgumentException("You should specify the new nickname you need to set to the user");
            await Context.Guild.GetUser(username.Id).ModifyAsync(x => x.Nickname = name);
            await ReplyAsync("<a:SuccessKB:639875484972351508> Nickname changed. ");
        }

        [Command("list-bans", RunMode = RunMode.Async)]
        [Alias("banlist", "ban-list", "listbans", "serverbans", "server-bans")]
        [Example("k!list-bans")]
        [Summary("List the users currently banned on the server")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task ListBans()
        {
            List<PaginatedMessage.Page> pages = new List<PaginatedMessage.Page>();
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            try
            {
                var bans = await Context.Guild.GetBansAsync();
                if (bans.Count > 0)
                {
                    foreach (var ban in bans)
                    {
                        string reason = ban.Reason;
                        if (string.IsNullOrEmpty(reason))
                        {
                            reason = "[No Reason was set]";
                        }
                        fields.Add(new EmbedFieldBuilder()
                        {
                            Name = ban.User.Username,
                            Value = $"ID: **{ban.User.Id}** \nReason: **{reason}**",
                            IsInline = false
                        });
                        if (fields.Count > 4)
                        {
                            List<EmbedFieldBuilder> firstpart = new List<EmbedFieldBuilder>();
                            firstpart.AddRange(fields);
                            fields.RemoveRange(0, 4);
                            firstpart.RemoveRange(4, fields.Count);
                            pages.Add(new PaginatedMessage.Page
                            {
                                Author = new EmbedAuthorBuilder { Name = $"Ban list of \"{Context.Guild.Name}\":" },
                                Description = $"Count: {bans.Count} \nOnly the user who used the command can use the reactions, and if you want to jump through pages react with :1234: and send the page's number.",
                                Fields = new List<EmbedFieldBuilder>(firstpart)
                            });

                        }
                    }
                    pages.Add(new PaginatedMessage.Page
                    {
                        Author = new EmbedAuthorBuilder { Name = $"Ban list of \"{Context.Guild.Name}\":" },
                        Description = $"Count: {bans.Count} \nOnly the user who used the command can use the reactions, and if you want to jump through pages react with :1234: and send the page's number.",
                        Fields = new List<EmbedFieldBuilder>(fields)
                    });
                    var pager = new PaginatedMessage
                    {
                        Pages = pages,
                        Color = Color.DarkGreen,
                        FooterOverride = null,
                        Options = PaginatedAppearanceOptions.Default,
                    };
                        await PagedReplyAsync(pager, new ReactionList
                        {
                            Forward = true,
                            Backward = true,
                            Jump = true,
                            Trash = true
                        }, true);
                }
                else
                {
                    await ReplyAsync("Looks like its empty in here... \nI guess there's much space to fill :wink:");
                }
            }
            catch (Exception e)
            {
                await ReplyAsync($"Error: {e.Message}");
            }
        }

        [Command("createtext")]
        [Remarks("Make A Text Channel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public async Task Text(string channelname = null)
        {
            if (channelname == null)
                throw new ArgumentException("Please type the name of the channel you want to create");
            await Context.Guild.CreateTextChannelAsync(channelname);
            await ReplyAsync("<a:SuccessKB:639875484972351508> Text channel was created.");
        }

        [Command("createvoice")]
        [Remarks("Make A Voice Channel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public async Task Voice([Remainder]string channelname = null)
        {
            if (channelname == null)
                throw new ArgumentException("Please type the name of the channel you want to create");
            await Context.Guild.CreateVoiceChannelAsync(channelname);
            await ReplyAsync("<a:SuccessKB:639875484972351508> Voice channel was created.");
        }
       

        [Command("announce")]
        [Remarks("Make A Announcement")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireContext(ContextType.Guild)]
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
        [Command("emojisteal")]
        [Alias("stealemoji")]
        [Summary("Put an emoji after the command and the bot will steal it and add it to the server with the same name.")]
        [Remarks("You and the bot should be able to Manage Emojis for this.")]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        [RequireBotPermission(GuildPermission.ManageEmojis)]
        [RequireContext(ContextType.Guild)]
        public async Task Emojisteal([Summary("The emoji you want to steal.")]string input = null)
        {
            if (input == null)
            {
                await ReplyAsync($"{Constants.fail} Please include the emoji you want to steal. 😈");
                return;
            }
            try
            {
                var emote = Emote.Parse(input);
                var msg = "";
                using (var webclient = new WebClient())
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(emote.Url);
                    HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    Stream stream = httpWebReponse.GetResponseStream();
                    try
                    {
                        var emoji = await Context.Guild.CreateEmoteAsync(emote.Name, new Image(stream));
                        msg = $"<a:SuccessKB:639875484972351508> Succesfully added {emoji} with the name of \"**{emoji.Name}**\"";
                    }
                    catch
                    {
                        await ReplyAsync($"Something went wrong... Possible errors: Maximum amount of emojis was reached or not a custom emoji.");
                    }
                    stream.Dispose();
                }
                await ReplyAsync(msg);
            }
            catch (Exception e)
            {
                if (Context.Message.Attachments.Count == 1)
                {
                    try
                    {
                        var msg = "";
                        foreach (var attach in Context.Message.Attachments)
                        {
                            using (var webclient = new WebClient())
                            {
                                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(attach.Url);
                                HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                Stream stream = httpWebReponse.GetResponseStream();

                                var emoji = await Context.Guild.CreateEmoteAsync(input, new Image(stream));
                                msg = $"<a:SuccessKB:639875484972351508> Succesfully added {emoji} with the name of \"**{input}**\"";
                                stream.Dispose();
                            }
                            await ReplyAsync(msg);
                        }
                    }
                    catch
                    {
                        await ReplyAsync("Something went wrong... Possible errors: Maximum amount of emojis was reached, invalid characters for the name, or the file size is too big for an emoji.");
                    }
                }
            }
        }

        [Command("move"), Summary("Moves a message from one channel to another."), Example("k!move 663803219440566272 #boo cool reason")]
        [Cooldown(10, true)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task Run(ulong messageId, SocketTextChannel channel, [Remainder] string reason = null)
        {
            // Ignore bots and same channel-to-channel requests
            if (Context.User.IsBot) return;

            IMessage message = null;

            try
            {
                message = await channel.GetMessageAsync(messageId);

                if (message == null)
                    message = await FindMessageInUnknownChannel(messageId);
                if (message.Channel.Id == channel.Id)
                {
                    await ReplyAsync("You can't move the message to the same channel it is in! DUH.");
                    return;
                }
            }
            catch (Exception e)
            {
                await ReplyAsync($"Failed fetching message for Move command, ran by {Context.User} with a Message ID of **{messageId}**");
            }

            // Format output
            var builder = new EmbedBuilder()
                    .WithColor(new Color(95, 186, 125))
                    .WithTimestamp(message.Timestamp)
                    .WithAuthor(message.Author.ToString(), message.Author.GetAvatarUrl())
                    .WithDescription(message.Content);

            if (!string.IsNullOrWhiteSpace(reason))
            {
                builder.AddField("Reason", reason, true);
            }
            if (message.Attachments.Count > 0)
            {
                builder.WithImageUrl(message.Attachments.ElementAt(0).Url);
            }
            var mover = $"{Context.User}";
            builder.WithFooter($"Message was moved from #{message.Channel.Name} by {mover} | Message Date:");

            await channel.SendMessageAsync("", embed: builder.Build());

            // Delete the source message, and the command message that started this request
            await message.DeleteAsync();
            await Context.Message.DeleteAsync();
            if (reason == null)
            {
                await message.Channel.SendMessageAsync($"Message: **{messageId}** sent by **{message.Author}** was moved by **{mover}** to <#{channel.Id}>");
            }
            else
            {
                await message.Channel.SendMessageAsync($"Message: **{messageId}** sent by **{message.Author}** was moved by **{mover}** to <#{channel.Id}> \n**Reason**: {reason}");
            }
        }

        [Command("move"), Summary("Moves a message from one channel to another."), Example("k!move #boo 663803219440566272 Message shouldn't be here")]
        [Cooldown(10, true)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task Run(SocketTextChannel channel, ulong messageId, [Remainder] string reason = null)
            => await Run(messageId, channel, reason);

        public static string[] Formats =
        {
            // Used to parse stuff like 1d14h2m11s and 1d 14h 2m 11s could add/remove more if needed

            "d'd'",
            "d'd'm'm'", "d'd 'm'm'",
            "d'd'h'h'", "d'd 'h'h'",
            "d'd'h'h's's'", "d'd 'h'h 's's'",
            "d'd'm'm's's'", "d'd 'm'm 's's'",
            "d'd'h'h'm'm'", "d'd 'h'h 'm'm'",
            "d'd'h'h'm'm's's'", "d'd 'h'h 'm'm 's's'",

            "h'h'",
            "h'h'm'm'", "h'h m'm'",
            "h'h'm'm's's'", "h'h 'm'm 's's'",
            "h'h's's'", "h'h s's'",
            "h'h'm'm'", "h'h 'm'm'",
            "h'h's's'", "h'h 's's'",

            "m'm'",
            "m'm's's'", "m'm 's's'",

            "s's'"
        };
        #region Channel locking/unlocking
        [Command("lock")]
        [Summary("Locks a channel")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(10)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task ChannelLock([Summary("The text channel you want to lock")]IGuildChannel textChannel = null, [Summary("OPTIONAL: The reason behind locking the channel")]string reason = null, [Summary("OPTIONAL: The time you want to lock the channel for")]string time = null)
        {
            if (reason == null)
                reason = "[No reason was provided]";
            if (time == null)
            {
                try
                {
                    textChannel = textChannel ?? (IGuildChannel)Context.Channel;
                    //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Description = $"🔒  <#{textChannel.Id}> Locked \n \n**Reason:** {reason}",
                        Color = Color.Orange,

                    };
                    foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                    {
                        var perms = textChannel.GetPermissionOverwrite(role).GetValueOrDefault();

                        await textChannel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Deny));
                    }
                    await Context.Channel.SendMessageAsync("", false, builder.Build());
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Something went wrong!");
                }
            }
            else //if time is specified
            {
                try
                {
                    //sends locked message
                    var timeset = TimeSpan.ParseExact(time, Formats, CultureInfo.CurrentCulture);
                    textChannel = textChannel ?? (IGuildChannel)Context.Channel;
                    //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Description = $"🔒  <#{textChannel.Id}> Locked \n \n**Reason:** {reason} \n**Duration:** {timeset}",
                        Color = Color.Orange,

                    };
                    foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                    {
                        var perms = textChannel.GetPermissionOverwrite(role).GetValueOrDefault();

                        await textChannel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Deny));
                    }
                    await Context.Channel.SendMessageAsync("", false, builder.Build());

                    await Task.Delay(timeset);
                    //unlocks after the time passes
                    try
                    {
                        textChannel = textChannel ?? (IGuildChannel)Context.Channel;
                        //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                        EmbedBuilder builder2 = new EmbedBuilder
                        {
                            Description = $"🔓  <#{textChannel.Id}> Unlocked \n \n**Reason:** Lock duration finished.",
                            Color = Color.DarkGreen,

                        };
                        foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                        {
                            var perms = textChannel.GetPermissionOverwrite(role).GetValueOrDefault();
                            if (role.Name != "Muted")
                                await textChannel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Allow));
                        }
                        await Context.Channel.SendMessageAsync("", false, builder2.Build());
                    }
                    catch
                    {
                        await Context.Channel.SendMessageAsync("Something went wrong!");
                    }

                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Something went wrong!");
                }
            }
        }

        [Command("lock")]
        [Summary("Locks a channel")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(10)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task ChannelLock([Summary("OPTIONAL: The reason behind locking the channel")]string reason = null, [Summary("OPTIONAL: The time you want to lock the channel for")]string time = null)
        {
            if (reason == null)
                reason = "[No reason was provided]";
            if (time == null)
            {
                try
                {
                    var textChannel = (IGuildChannel)Context.Channel;
                    //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Description = $"🔒  <#{textChannel.Id}> Locked \n \n**Reason:** {reason}",
                        Color = Color.Orange,

                    };
                    foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                    {
                        var perms = textChannel.GetPermissionOverwrite(role).GetValueOrDefault();

                        await textChannel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Deny));
                    }
                    await Context.Channel.SendMessageAsync("", false, builder.Build());
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Something went wrong!");
                }
            }
            else //if time is specified
            {
                try
                {
                    //sends locked message
                    var timeset = TimeSpan.ParseExact(time, Formats, CultureInfo.CurrentCulture);
                    var textChannel = (IGuildChannel)Context.Channel;
                    //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Description = $"🔒  <#{textChannel.Id}> Locked \n \n**Reason:** {reason} \n**Duration:** {timeset}",
                        Color = Color.Orange,

                    };
                    foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                    {
                        var perms = textChannel.GetPermissionOverwrite(role).GetValueOrDefault();

                        await textChannel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Deny));
                    }
                    await Context.Channel.SendMessageAsync("", false, builder.Build());

                    await Task.Delay(timeset);
                    //unlocks after the time passes
                    try
                    {
                        textChannel = textChannel ?? (IGuildChannel)Context.Channel;
                        //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                        EmbedBuilder builder2 = new EmbedBuilder
                        {
                            Description = $"🔓  <#{textChannel.Id}> Unlocked \n \n**Reason:** Lock duration finished.",
                            Color = Color.DarkGreen,

                        };
                        foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                        {
                            var perms = textChannel.GetPermissionOverwrite(role).GetValueOrDefault();
                            if (role.Name != "Muted")
                                await textChannel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Allow));
                        }
                        await Context.Channel.SendMessageAsync("", false, builder2.Build());
                    }
                    catch
                    {
                        await Context.Channel.SendMessageAsync("Something went wrong!");
                    }

                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Something went wrong!");
                }
            }
        }

        [Command("unlock")]
        [Summary("Unlocks a channel")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(10)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task ChannelUnLock([Summary("The text channel you want to unlock")]IGuildChannel textChannel = null, [Summary("OPTIONAL: The reason behind unlocking the channel")]string reason = null)
        {
            if (reason == null)
                reason = "[No reason was provided]";
            try
            {
                textChannel = textChannel ?? (IGuildChannel)Context.Channel;
                //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                EmbedBuilder builder = new EmbedBuilder
                {
                    Description = $"🔓  <#{textChannel.Id}> Unlocked \n \n**Reason:** {reason}",
                    Color = Color.DarkGreen,

                };
                foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                {
                    var perms = textChannel.GetPermissionOverwrite(role).GetValueOrDefault();
                    if (role.Name != "Muted")
                        await textChannel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Allow));
                }
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
            catch
            {
                await Context.Channel.SendMessageAsync("Something went wrong!");
            }
        }

        [Command("unlock")]
        [Summary("Unlocks a channel")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(10)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task ChannelUnLock([Summary("OPTIONAL: The reason behind unlocking the channel")]string reason = null)
        {
            if (reason == null)
                reason = "[No reason was provided]";
            try
            {
                var textChannel = (IGuildChannel)Context.Channel;
                //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                EmbedBuilder builder = new EmbedBuilder
                {
                    Description = $"🔓  <#{textChannel.Id}> Unlocked \n \n**Reason:** {reason}",
                    Color = Color.DarkGreen,

                };
                foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                {
                    var perms = textChannel.GetPermissionOverwrite(role).GetValueOrDefault();
                    if (role.Name != "Muted")
                        await textChannel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Allow));
                }
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
            catch
            {
                await Context.Channel.SendMessageAsync("Something went wrong!");
            }
        }
        #endregion

        #region Server locking/unlocking
        [Command("lock-server")]
        [Summary("Locks the whole server.")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(15)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task ServerLock([Summary("OPTIONAL: The reason behind locking the server")]string reason = null, [Summary("OPTIONAL: The time you want to lock the server for")]string time = null)
        {
            if (reason == null)
                reason = "[No reason was provided]";
            var guild = Context.Guild;
            EmbedBuilder emb = new EmbedBuilder()
                .WithColor(Color.DarkerGrey)
                .WithDescription("🔐 Locking server... this may take some time.");
            var msg = await ReplyAsync("", false, emb.Build());
            if (time == null)
            {
                try
                {
                    //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Description = $"🔒  **{guild.Name}** Locked \n \n**Reason:** {reason}",
                        Color = Color.Orange,

                    };
                    foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                    {
                        foreach (var channel in guild.TextChannels)
                        {
                            var perms = channel.GetPermissionOverwrite(role).GetValueOrDefault();

                            await channel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Deny));
                        }
                        foreach (var channel in guild.VoiceChannels)
                        {
                            var perms = channel.GetPermissionOverwrite(role).GetValueOrDefault();

                            await channel.AddPermissionOverwriteAsync(role, perms.Modify(connect: PermValue.Deny));
                        }
                    }
                    await msg.ModifyAsync(m => m.Embed = builder.Build());
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Something went wrong!");
                }
            }
            else //if time is specified
            {
                try
                {
                    //sends locked message
                    var timeset = TimeSpan.ParseExact(time, Formats, CultureInfo.CurrentCulture);
                    //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Description = $"🔒  **{guild.Name}** Locked \n \n**Reason:** {reason} \n**Duration:** {timeset}",
                        Color = Color.Orange,

                    };
                    foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                    {
                        foreach (var channel in guild.TextChannels)
                        {
                            var perms = channel.GetPermissionOverwrite(role).GetValueOrDefault();

                            await channel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Deny));
                        }
                        foreach (var channel in guild.VoiceChannels)
                        {
                            var perms = channel.GetPermissionOverwrite(role).GetValueOrDefault();

                            await channel.AddPermissionOverwriteAsync(role, perms.Modify(connect: PermValue.Deny));
                        }
                    }
                    await msg.ModifyAsync(m => m.Embed = builder.Build());

                    await Task.Delay(timeset);
                    //unlocks after the time passes
                    try
                    {
                        //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                        EmbedBuilder builder2 = new EmbedBuilder
                        {
                            Description = $"🔓  **{guild.Name}** Unlocked \n \n**Reason:** Lock duration finished.",
                            Color = Color.DarkGreen,

                        };
                        foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                        {
                            foreach (var channel in guild.TextChannels)
                            {
                                var perms = channel.GetPermissionOverwrite(role).GetValueOrDefault();

                                if (role.Name != "Muted")
                                    await channel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Allow));
                            }
                            foreach (var channel in guild.VoiceChannels)
                            {
                                var perms = channel.GetPermissionOverwrite(role).GetValueOrDefault();

                                if (role.Name != "Muted")
                                    await channel.AddPermissionOverwriteAsync(role, perms.Modify(connect: PermValue.Allow));
                            }
                        }
                        await Context.Channel.SendMessageAsync("", false, builder2.Build());
                    }
                    catch
                    {
                        await Context.Channel.SendMessageAsync("Something went wrong!");
                    }

                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Something went wrong!");
                }
            }
        }

        [Command("unlock-server")]
        [Summary("Unlocks the whole server")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(15)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task ServerUnLock([Summary("OPTIONAL: The reason behind unlocking the server")]string reason = null)
        {
            if (reason == null)
                reason = "[No reason was provided]";
            EmbedBuilder emb = new EmbedBuilder()
                .WithColor(Color.DarkerGrey)
                .WithDescription("🔐 Unlocking server... this may take some time.");
            var msg = await ReplyAsync("", false, emb.Build());
            var guild = Context.Guild;
            try
            {
                //IGuildChannel textChannel = (IGuildChannel)Context.Channel;
                EmbedBuilder builder = new EmbedBuilder
                {
                    Description = $"🔓  **{guild.Name}** Unlocked \n \n**Reason:** {reason}",
                    Color = Color.DarkGreen,

                };
                foreach (IRole role in Context.Guild.Roles.Where(r => !r.Permissions.ManageMessages))
                {
                    foreach (var channel in guild.TextChannels)
                    {
                        var perms = channel.GetPermissionOverwrite(role).GetValueOrDefault();

                        if (role.Name != "Muted")
                            await channel.AddPermissionOverwriteAsync(role, perms.Modify(sendMessages: PermValue.Allow));
                    }
                    foreach (var channel in guild.VoiceChannels)
                    {
                        var perms = channel.GetPermissionOverwrite(role).GetValueOrDefault();

                        if (role.Name != "Muted")
                            await channel.AddPermissionOverwriteAsync(role, perms.Modify(connect: PermValue.Allow));
                    }
                }
                await msg.ModifyAsync(m => m.Embed = builder.Build());
            }
            catch
            {
                await Context.Channel.SendMessageAsync("Something went wrong!");
            }
        }
        #endregion


        private async Task<IMessage> FindMessageInUnknownChannel(ulong messageId)
        {
            IMessage message = null;
            var guild = Context.Guild as SocketGuild;
            // We haven't found a message, now fetch all text
            // channels and attempt to find the message

            var channels = guild.TextChannels.ToList();

            foreach (var channel in channels)
            {
                try
                {
                    message = await channel.GetMessageAsync(messageId);

                    if (message != null)
                        break;
                }
                catch (Exception e)
                {
                    await ReplyAsync($"Failed accessing channel {channel.Name} when searching for message **{messageId}**");
                }
            }

            return message;
        }
        //WARN SYSTEM
        [Command("warn")]
        [Alias("strike")]
        [Summary("Direct message a user with a warning")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
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
            var warned_username = $"{user}";
            var warned_userid = user.Id;
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
            var moderator = $"{Context.User.Username}#{Context.User.Discriminator}";
            var time = DateTime.UtcNow;
            var newWarn = new WarnEntry(moderator,warned_username,warned_userid,time, reason);
            guild.Warns.Add(newWarn);
            var warnings = guild.Warns.ToList();
            guild.Modify(g => g.SetWarns(warnings), _globalGuildAccounts);            

        }

        [Command("warnings"),Alias("strikes", "warns","warnlist","strikelist","listwarns","liststrikes")]
        [Summary("Lists the strikes/warnings of a user. Only staff can use it.")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task ListWarns([Remainder] [Summary("OPTIONAL: The user you want to see their warnings.")] SocketGuildUser user = null)
        {
            if (user == null)
            {
                var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
                var Warns = _globalGuildAccounts.GetById(Context.Guild.Id).Warns;
                List<PaginatedMessage.Page> pages = new List<PaginatedMessage.Page>();
                List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
                for (var i = 0; i < Warns.Count; i++)
                {

                    //List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = $"[{i + 1}] {Warns[i].Warned_username}\n({Warns[i].Warned_userid})",
                        Value = $"By: {Warns[i].Moderator} \nReason: {Warns[i].Reason} \nDate: {Warns[i].Time}",
                        IsInline = false
                    });
                    if (fields.Count > 3)
                    {
                        List<EmbedFieldBuilder> firstpart = new List<EmbedFieldBuilder>();
                        firstpart.AddRange(fields);
                        fields.RemoveRange(0, 3);
                        firstpart.RemoveRange(3, fields.Count);
                        pages.Add(new PaginatedMessage.Page
                        {
                            Author = new EmbedAuthorBuilder { Name = "Server Warnings:" },
                            Description = $"Count: {guild.Warns.Count} \n\nDates are in mm/dd format and time is in UTC. Only the user who used the command can use the reactions, and if you want to jump through pages react with :1234: and send the page's number",
                            Fields = new List<EmbedFieldBuilder>(firstpart)
                        });

                    }
                }
                pages.Add(new PaginatedMessage.Page
                {
                    Author = new EmbedAuthorBuilder { Name = "Server Warnings:" },
                    Description = $"Count: {guild.Warns.Count} \n \nDates are in mm/dd format and time is in UTC. Only the user who used the command can use the reactions, and if you want to jump through pages react with :1234: and send the page's number",
                    Fields = new List<EmbedFieldBuilder>(fields)
                });
                var pager = new PaginatedMessage
                {
                    Pages = pages,
                    Color = Color.DarkGreen,
                    FooterOverride = null,
                    Options = PaginatedAppearanceOptions.Default,
                };
                if (Warns.Count != 0)
                {
                    await PagedReplyAsync(pager, new ReactionList
                    {
                        Forward = true,
                        Backward = true,
                        First = true,
                        Last = true,
                        Jump = true,
                        Trash = true
                    }, true);
                }
                else
                {
                    await PagedReplyAsync(pager, new ReactionList
                    {
                        Forward = false,
                        Backward = false,
                        Jump = false,
                        Trash = true
                    }, true);
                }
            }
            else
            {
                var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
                var Warns = _globalGuildAccounts.GetById(Context.Guild.Id).Warns;
                List<WarnEntry> userwarns = Warns.Where(x => x.Warned_userid == user.Id).ToList();
                List<PaginatedMessage.Page> pages = new List<PaginatedMessage.Page>();
                List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
                for (var i = 0; i < Warns.Count; i++)
                {
                    if (userwarns.Count == 0)
                        break;
                    if (Warns[i].Warned_userid != user.Id)
                        i += 1;
                    if (Warns[i].Warned_userid == user.Id)
                    {

                        fields.Add(new EmbedFieldBuilder()
                        {
                            Name = $"[{i + 1}]",
                            Value = $"By: {Warns[i].Moderator} \nReason: {Warns[i].Reason} \nDate: {Warns[i].Time}",
                            IsInline = false
                        });
                        if (fields.Count > 3)
                        {
                            List<EmbedFieldBuilder> firstpart = new List<EmbedFieldBuilder>();
                            firstpart.AddRange(fields);
                            fields.RemoveRange(0, 3);
                            firstpart.RemoveRange(3, fields.Count);
                            pages.Add(new PaginatedMessage.Page
                            {
                                Author = new EmbedAuthorBuilder { Name = $"{user} Warnings:" },
                                Description = $"Count: {userwarns.Count} \n\nDates are in mm/dd format and time is in UTC. Only the user who used the command can use the reactions, and if you want to jump through pages react with :1234: and send the page's number",
                                Fields = new List<EmbedFieldBuilder>(firstpart)
                            });

                        }
                    }
                }
                pages.Add(new PaginatedMessage.Page
                {
                    Author = new EmbedAuthorBuilder { Name = $"{user} Warnings:" },
                    Description = $"Count: {userwarns.Count} \n \nDates are in mm/dd format and time is in UTC. Only the user who used the command can use the reactions, and if you want to jump through pages react with :1234: and send the page's number",
                    Fields = new List<EmbedFieldBuilder>(fields)
                });
                var pager = new PaginatedMessage
                {
                    Pages = pages,
                    Color = Color.DarkGreen,
                    FooterOverride = null,
                    Options = PaginatedAppearanceOptions.Default,
                };
                if (userwarns.Count != 0)
                {
                    await PagedReplyAsync(pager, new ReactionList
                    {
                        Forward = true,
                        Backward = true,
                        Jump = true,
                        Trash = true
                    }, true);
                }
                else if (userwarns.Count == 0)
                {
                    await PagedReplyAsync(pager, new ReactionList
                    {
                        Forward = false,
                        Backward = false,
                        Jump = false,
                        Trash = true
                    }, true);
                }
            }
           
        }

        [Command("mywarnings"), Alias("mystrikes", "mywarns", "mywarnlist", "mystrikelist", "mylistwarns", "myliststrikes")]
        [Summary("Lists your strikes/warnings (if any). Any user can use this command.")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireContext(ContextType.Guild)]
        public async Task MyListWarns()
        {
            var server = _globalGuildAccounts.GetById(Context.Guild.Id);
            var warns = server.Warns.ToList();
            List<WarnEntry> userwarns = warns.Where(x => x.Warned_userid == Context.User.Id).ToList();
            List<PaginatedMessage.Page> pages = new List<PaginatedMessage.Page>();
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            for (var i = 0; i < warns.Count; i++)
            {
                if (userwarns.Count == 0)
                    break;
                if (warns[i].Warned_userid != Context.User.Id)
                    i += 1;
                if (warns[i].Warned_userid == Context.User.Id)
                {
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = $"[{i + 1}]",
                        Value = $"By: {warns[i].Moderator} \nReason: {warns[i].Reason} \nDate: {warns[i].Time}",
                        IsInline = false
                    });
                    if (fields.Count > 3)
                    {
                        List<EmbedFieldBuilder> firstpart = new List<EmbedFieldBuilder>();
                        firstpart.AddRange(fields);
                        fields.RemoveRange(0, 3);
                        firstpart.RemoveRange(3, fields.Count);
                        pages.Add(new PaginatedMessage.Page
                        {
                            Author = new EmbedAuthorBuilder { Name = "Your Warnings:" },
                            Description = $"Count: {userwarns.Count} \n\nDates are in mm/dd format and time is in UTC. Only the user who used the command can use the reactions, and if you want to jump through pages react with :1234: and send the page's number",
                            Fields = new List<EmbedFieldBuilder>(firstpart)
                        });

                    }
                }
            }
            pages.Add(new PaginatedMessage.Page
            {
                Author = new EmbedAuthorBuilder { Name = "Your Warnings:" },
                Description = $"Count: {userwarns.Count} \n \nDates are in mm/dd format and time is in UTC. Only the user who used the command can use the reactions, and if you want to jump through pages react with :1234: and send the page's number",
                Fields = new List<EmbedFieldBuilder>(fields)
            });
            var pager = new PaginatedMessage
            {
                Pages = pages,
                Color = Color.DarkGreen,
                FooterOverride = null,
                Options = PaginatedAppearanceOptions.Default,
            };
            if (userwarns.Count != 0)
            {
                await PagedReplyAsync(pager, new ReactionList
                {
                    Forward = true,
                    Backward = true,
                    First = true,
                    Last = true,
                    Jump = true,
                    Trash = true
                }, true);
            }
            else if (userwarns.Count == 0)
            {
                await PagedReplyAsync(pager, new ReactionList
                {
                    Forward = false,
                    Backward = false,
                    Jump = false,
                    Trash = true
                }, true);
            }
        }

        [Command("removewarn")]
        [Alias("removestrike", "deletestrike","deletewarn", "remove-warn", "delete-warn", "remove-strike", "delete-strike", "clear-warn", "clear-strike")]
        [Summary("Remove a strike/warning using the index found between the [] in the k!warnings")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
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
                _globalGuildAccounts.SaveAccounts(guild.Id);
                responseString = $"Deleted the warning with index **{i}**!";
            }

            await ReplyAsync(responseString);
        }
        [Command("clearwarnings")]
        [Alias("clear-warns", "clearstrikes", "clear-strikes", "clearwarns" , "clear-warnings")]
        [Summary("Clear all warnings for the server OR for specific user.")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task ClearWarns([Summary("OPTIONAL: The user you want to clear their warns/strikes.")] SocketUser user = null)
        {
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
            var Warns = _globalGuildAccounts.GetById(Context.Guild.Id).Warns;
            if (user != null)
            {
                var userwarns = Warns.ToList().Where(w => w.Warned_userid == user.Id).ToList();
                if (userwarns.Count != 0)
                {
                    foreach (var warn in userwarns)
                    {
                        Warns.Remove(warn);
                    }
                    guild.Modify(g => g.SetWarns(Warns.ToList()), _globalGuildAccounts);
                    _globalGuildAccounts.SaveAccounts(guild.Id);
                    await ReplyAsync($"{Constants.success} Successfully cleared all warnings for **{user}** ({user.Id})");
                }
                else
                {
                    await ReplyAsync($"{Constants.fail} **{user}** has no warnings to clear!");
                }
            }
            else
            {
                if (Warns.Count != 0)
                {
                    Warns = new List<WarnEntry>();
                    guild.Modify(g => g.SetWarns(Warns.ToList()), _globalGuildAccounts);
                    _globalGuildAccounts.SaveAccounts(guild.Id);
                    await ReplyAsync($"{Constants.success} Successfully cleared all warnings for this server!");
                }
                else
                {
                    await ReplyAsync($"{Constants.fail} This server has no warnings to clear!");
                }
            }
        }
        [Command("clearwarnings")]
        [Alias("clear-warns", "clearstrikes", "clear-strikes", "clearwarns", "clear-warnings")]
        [Summary("Clear all warnings for a specific user.")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task ClearWarns([Summary("The user you want to clear their warns/strikes.")] SocketUser user, int amount)
        {
            if (amount == 0)
            {
                await ReplyAsync($"{Constants.fail} You want to remove 0 warnings? Well I can only remove 1+");
            }
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
            var Warns = _globalGuildAccounts.GetById(Context.Guild.Id).Warns;
            var userwarns = Warns.ToList().Where(w => w.Warned_userid == user.Id).ToList();
            if (userwarns.Count != 0 && userwarns.Count >= amount)
            {
                foreach (var warn in userwarns.Take(amount).ToList())
                {
                    Warns.Remove(warn);
                }
                guild.Modify(g => g.SetWarns(Warns.ToList()), _globalGuildAccounts);
                _globalGuildAccounts.SaveAccounts(guild.Id);
                await ReplyAsync($"{Constants.success} Successfully cleared {amount} warning(s) for **{user}** ({user.Id})");
            }
            else if (userwarns.Count != 0 && userwarns.Count < amount)
            {
                await ReplyAsync($"{Constants.fail} **{user}** doesn't have {amount} warnings, they only have {userwarns.Count}.");
            }
            else
            {
                await ReplyAsync($"{Constants.fail} **{user}** has no warnings to clear!");
            }
        }
        //end
    }
}
