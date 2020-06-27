using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;
using Bot.Preconditions;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace Bot.Modules
{
    public class ServerSetup : InteractiveBase
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        private readonly Logger _logger;

        public ServerSetup(Logger logger, GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
            _logger = logger;
        }
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
        /*
        [Command("offLog")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task SetServerActivivtyLogOff()
        {
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
            guild.LogChannelId = 0;
            guild.ServerActivityLog = 0;
            _globalGuildAccounts.SaveAccounts(Context.Guild.Id);

            await ReplyAsync("No more Logging");

        }

        /// <summary>
        /// by saying "SetLog" it will create a   channel itself, you may move and rname it
        /// by saying "SetLog ID" it will set channel "ID" as Logging Channel
        /// by saying "SetLog" again, it will turn off Logging, but will not delete it from the file
        /// </summary>
        /// <param name="logChannel"></param>
        /// <returns></returns>
        [Command("SetLog")]
        [Alias("SetLogs")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task SetServerActivivtyLog(SocketTextChannel logChannel = null)
        {
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);

            if (logChannel != null)
            {
                try
                {
                    var channel = Context.Guild.GetTextChannel(logChannel.Id);
                    guild.LogChannelId = channel.Id;
                    guild.ServerActivityLog = 1;
                    _globalGuildAccounts.SaveAccounts(Context.Guild.Id);

                }
                catch
                {
//
                }

                return;
            }
            switch (guild.ServerActivityLog)
            {
                case 1:
                    guild.ServerActivityLog = 0;
                    guild.LogChannelId = 0;
                    _globalGuildAccounts.SaveAccounts(Context.Guild.Id);


                        await ReplyAsync("Logging was turned off.");

                    return;
                case 0:
                    try
                    {
                        try
                        {
                            var tryChannel = Context.Guild.GetTextChannel(guild.LogChannelId);
                            if (tryChannel.Name != null)
                            {
                                guild.LogChannelId = tryChannel.Id;
                                guild.ServerActivityLog = 1;
                                _globalGuildAccounts.SaveAccounts(Context.Guild.Id);

                                await ReplyAsync(
                                    $"Created a new logging channel {tryChannel.Mention}, you may rename and move it.");
                            }
                        }
                        catch
                        {

                            var channel = Context.Guild.CreateTextChannelAsync("KBlogs");
                            guild.LogChannelId = channel.Result.Id;
                            guild.ServerActivityLog = 1;
                            _globalGuildAccounts.SaveAccounts(Context.Guild.Id);

                            await ReplyAsync(
                                $"Created a new logging channel {channel.Result.Mention}, you may rename and move it.");
                        }
                    }
                    catch
                    {
                     //ignored
                    }
                    break;
            }
        }

        */
        [Command("autorole-toggle")]
        [Alias("toggle-autorole", "ar-toggle", "toggle-ar")]
        [Summary("Toggle/turn the autorole on or off.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task ToggleAutoRole(string option = null)
        {
            var guild = _globalGuildAccounts.GetById(Context.Guild.Id);
            if (option == null)
            {
                await ReplyAsync($"{Constants.fail} Do you want to toggle auto role **on** or **off**? Please provide the option with the command.");
                return;
            }
            else if (option.ToLower() == "on")
            {
                if (guild.RoleOnJoinToggle == true)
                {
                    await ReplyAsync($"{Constants.fail} Sorry but the Auto Role is already turned on.");
                }
                else
                {
                    guild.RoleOnJoinToggle = true;
                    await ReplyAsync($"{Constants.success} Successfully toggled on Auto Role. Make sure you have setted it up before so I can automatically give roles, by using `k!autorole-setup`.");
                }
            }
            else if (option.ToLower() == "off")
            {
                if (guild.RoleOnJoinToggle == false)
                {
                    await ReplyAsync($"{Constants.fail} Sorry but the Auto Role is already turned off.");
                }
                else
                {
                    guild.RoleOnJoinToggle = false;
                    await ReplyAsync($"{Constants.success} Successfully toggled off Auto Role. I will no longer automatically give roles to users.");
                }
            }
            else
            {
                await ReplyAsync($"{Constants.fail} Do you want to toggle auto role **on** or **off**? Please provide the option with the command.");
            }
            _globalGuildAccounts.SaveAccounts(Context.Guild.Id);
        }

        [Command("autorole-setup", RunMode = RunMode.Async)]
        [Alias("setup-autorole", "auto-role-setup", "setup-autorole", "setup-auto-role", "ar-setup", "setup-ar")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task SetRoleOnJoin()
        {
            var guild = _globalGuildAccounts.GetById(Context.Guild.Id);
            var message = "";

            #region Role Setup
            if (guild.RoleOnJoin != 0 && Context.Guild.GetRole(guild.RoleOnJoin) != null && guild.RoleOnJoinMethod != null)
                message = $"{Constants.fail} This server already has an auto role set up! (Role: **{Context.Guild.GetRole(guild.RoleOnJoin).Name}** , Method: **{guild.RoleOnJoinMethod}**) \n \nIf you would like to edit anything with it continue with this.";

            else if (guild.RoleOnJoin != 0 && Context.Guild.GetRole(guild.RoleOnJoin) == null)
                message = $"{Constants.fail} This server had an auto role set up but I can't find the role anymore! Consider setting a new auto role instead.";

            message += "\n \nYou are about to setup the auto role for this server where depending on the method chosen I will give a specific role automatically! \n \n**What role would you like to give users automatically?**";

            EmbedBuilder emb = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithFooter("You have 2 minutes to answer this before it times out. Type \"KBcancel\" to cancel.")
                .WithTitle("Auto Role Setup")
                .WithDescription(message);

            var setup = await ReplyAsync("", false, emb.Build());
            var role = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
            bool RoleSet = false;

            if (role.Content.ToLower() == "kbcancel")
            {
                await ReplyAsync($"{Constants.success} Auto Role Setup has been canceled.");
                return;
            }

            EmbedBuilder setrole = new EmbedBuilder()
                .WithTitle("Auto Role Setup")
                .WithFooter("You have 2 minutes to answer this before it times out. Type \"KBcancel\" to cancel.");

            var ServerRoles = Context.Guild.Roles.ToList();
            ulong roleid = 0;
            for (int i = 0; i < ServerRoles.Count; i++)
            {
                if (role.Content == ServerRoles[i].Id.ToString() || role.Content.ToLower() == ServerRoles[i].Name.ToLower())
                {
                    var roles = ServerRoles.ToList().Where(r => r.Name == role.Content);
                    if (roles.Count() > 1)
                    {
                        setrole.WithColor(Color.Orange);
                        setrole.WithDescription($"You are about to setup the auto role for this server where depending on the method chosen I will give a specific role automatically! \n \n{Constants.fail} Multiple role matches found. Please use the role's ID instead.");
                        await setup.ModifyAsync(x => x.Embed = setrole.Build());
                        role = null;
                        i = -1;
                        role = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
                    }
                    else
                    {
                        roleid = ServerRoles[i].Id;
                        RoleSet = true;
                        setrole.WithColor(Color.Green)
                        .WithDescription($"You are about to setup the auto role for this server where depending on the method chosen I will give a specific role automatically! \n \n{Constants.success} Succesfully added **{ServerRoles[i].Name}** as an Auto Role! \n \n**Which Auto Role method would you like to use? Make sure to type the method name properly.** \n(Options Available: __Instant__ , __Timed__ , __Phrased__ , __Message__ , __PhrasedTimed__ , __MessageTimed__)");
                    }
                }
                if (role.Content.ToLower() == "kbcancel")
                {
                    await ReplyAsync($"{Constants.success} Auto Role Setup has been canceled.");
                    return;
                }
                else if (RoleSet == false && i == ServerRoles.Count - 1)
                {
                    setrole.WithColor(Color.Red)
                        .WithDescription($"You are about to setup the auto role for this server where depending on the method chosen I will give a specific role automatically! \n \n{Constants.fail} **I couldn't find that role ({role.Content})! Make sure you provide the __name__ or __ID__ of a role in this server.**");
                    await setup.ModifyAsync(x => x.Embed = setrole.Build());
                    role = null;
                    i = -1;
                    role = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
                }
            }
            await setup.ModifyAsync(x => x.Embed = setrole.Build());
            #endregion

            #region Method Setup
            if (RoleSet == true && roleid != 0)
            {
                guild.RoleOnJoin = roleid;
                guild.Modify(g => g.SetRoleOnJoin(roleid), _globalGuildAccounts);
                _globalGuildAccounts.SaveAccounts(Context.Guild.Id);
                var methodchosen = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
                bool MethodRight = false;
                if (methodchosen.Content.ToLower() == "kbcancel")
                {
                    await ReplyAsync($"{Constants.success} Auto Role Setup has been canceled.");
                    return;
                }
                EmbedBuilder methodmsg = new EmbedBuilder()
                    .WithFooter("You have 2 minutes to answer this before it times out.")
                    .WithTitle("Auto Role Setup");
                methodmsg.Description = "";
                if (methodchosen.Content.ToLower() != "instant" && methodchosen.Content.ToLower() != "timed" && methodchosen.Content.ToLower() != "phrased" && methodchosen.Content.ToLower() != "message" && methodchosen.Content.ToLower() != "phrasedtimed" && methodchosen.Content.ToLower() != "messagetimed")
                {
                    methodmsg.WithColor(Color.Red);
                    methodmsg.Description += $"{Constants.fail} Please enter a valid method and make sure to type it as mentioned. \n(Methods Available: __Instant__ , __Timed__ , __Phrased__ , __Message__ , __PhrasedTimed__ , __MessageTimed__)";
                    await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    methodchosen = null;
                    methodchosen = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
                }
                if (methodchosen.Content.ToLower() == "instant" || methodchosen.Content.ToLower() == "timed" || methodchosen.Content.ToLower() == "phrased" || methodchosen.Content.ToLower() == "message" || methodchosen.Content.ToLower() == "phrasedtimed" || methodchosen.Content.ToLower() == "messagetimed")
                {
                    guild.RoleOnJoinMethod = methodchosen.Content.ToLower();
                    _globalGuildAccounts.SaveAccounts(guild.Id);
                    MethodRight = true;
                }
                //INSTANT METHOD
                if (methodchosen.Content.ToLower() == "instant")
                {
                    methodmsg.WithColor(Color.Green);
                    methodmsg.Description = $"\n \n{Constants.success} You have succesfully setup the auto role for **{Context.Guild.GetRole(guild.RoleOnJoin).Name}**! I will give new members this role __INSTANTLY__. \n \n(**Note: Make sure to check `k!autorole-info` for more information.**)";
                }
                //TIMED METHOD
                else if (methodchosen.Content.ToLower() == "timed")
                {
                    methodmsg.WithColor(Color.Green);
                    methodmsg.Description += $"\n \n{Constants.success} **After how much time would you like to give new members this role?** \nExample: 1h 15m 30s";
                    await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    var timemsg = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
                    if (timemsg.Content.ToLower() == "kbcancel")
                    {
                        await ReplyAsync($"{Constants.success} Auto Role Setup has been canceled.");
                        return;
                    }
                    try
                    {
                        var time = TimeSpan.ParseExact(timemsg.Content, Formats, CultureInfo.CurrentCulture);
                        guild.RoleOnJoinTime = time;
                        _globalGuildAccounts.SaveAccounts(guild.Id);
                        methodmsg.Description = $"**__Auto role has been setup__! If you'd like to change anything consider doing the command again.** \n \n{Constants.success} Successfully added time span {guild.RoleOnJoinTime}! \n \n(**Note: Make sure to check `k!autorole-info` for more information.**)";
                    }
                    catch
                    {
                        methodmsg.WithColor(Color.Red);
                        methodmsg.Description += $"\n \n{Constants.fail} Invalid time, try to follow the above example please and try again.";
                        await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    }

                }
                //PHRASED METHOD
                else if (methodchosen.Content.ToLower() == "phrased")
                {
                    methodmsg.Description = $"\n \n{Constants.success} **What phrase should new members type in order to get the role?** \nYou can have multiple phrases but make sure to separate them with a space.";
                    methodmsg.WithColor(Color.Green);
                    await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    var usermsg = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
                    if (usermsg.Content.ToLower() == "kbcancel")
                    {
                        await ReplyAsync($"{Constants.success} Auto Role Setup has been canceled.");
                        return;
                    }
                    if (usermsg.Content != null)
                    {
                        var phrases = usermsg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        guild.RoleOnJoinPhrase = phrases.ToList();
                        _globalGuildAccounts.SaveAccounts(guild.Id);
                        var phraseslist = "";
                        foreach (var phrase in phrases)
                        {
                            if (phrases.Length > 1)
                            {
                                phraseslist += $"`{phrase}`, ";
                                methodmsg.Description = $"**__Auto role has been setup__! If you'd like to change anything consider doing the command again.** \n \n{Constants.success} **Successfully added {phraseslist} as phrases!** \n \n(**Note: Make sure to check `k!autorole-info` for more information.**)";
                            }
                            else
                            {
                                phraseslist += $"`{phrase}`";
                                methodmsg.Description = $"**__Auto role has been setup__! If you'd like to change anything consider doing the command again.** \n \n{Constants.success} **Successfully added {phraseslist} as a phrase!** \n \n(**Note: Make sure to check `k!autorole-info` for more information.**)";
                            }
                        }
                        await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    }
                }
                //ON MESSAGE METHOD
                else if (methodchosen.Content.ToLower() == "message")
                {
                    methodmsg.WithColor(Color.Green);
                    methodmsg.Description = $"**__Auto role has been setup__! If you'd like to change anything consider doing the command again.** \n \n{Constants.success} **Success! I will now automatically give this role to users (non-bots) who send any __MESSAGE__** \n \n(**Note: Make sure to check `k!autorole-info` for more information.**)";
                }
                // PHRASED TIMED METHOD
                else if (methodchosen.Content.ToLower() == "phrasedtimed")
                {
                    methodmsg.WithColor(Color.Green);
                    methodmsg.Description = $"\n \n{Constants.success} **After how much time would you like to give new members this role?** \nExample: 1h 15m 30s";
                    await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    var timemsg = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
                    if (timemsg.Content.ToLower() == "kbcancel")
                    {
                        await ReplyAsync($"{Constants.success} Auto Role Setup has been canceled.");
                        return;
                    }
                    try
                    {
                        var time = TimeSpan.ParseExact(timemsg.Content, Formats, CultureInfo.CurrentCulture);
                        guild.RoleOnJoinTime = time;
                        _globalGuildAccounts.SaveAccounts(guild.Id);
                        methodmsg.Description = $"\n \n{Constants.success} **Succesfully added the time span ({guild.RoleOnJoinTime}). What phrase should new members type in order to get the role?** \nYou can have multiple phrases but make sure to separate them with a space.";
                        methodmsg.WithColor(Color.Green);
                        await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                        var usermsg = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
                        if (usermsg.Content.ToLower() == "kbcancel")
                        {
                            await ReplyAsync($"{Constants.success} Auto Role Setup has been canceled.");
                            return;
                        }
                        if (usermsg.Content != null)
                        {
                            var phrases = usermsg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            guild.RoleOnJoinPhrase = phrases.ToList();
                            _globalGuildAccounts.SaveAccounts(guild.Id);
                            var phraseslist = "";
                            foreach (var phrase in phrases)
                            {
                                if (phrases.Length > 1)
                                {
                                    phraseslist += $"`{phrase}`, ";
                                    methodmsg.Description = $"**__Auto role has been setup__! If you'd like to change anything consider doing the command again.** \n \n{Constants.success} **Successfully added {phraseslist} as phrases!** \n \n(**Note: Make sure to check `k!autorole-info` for more information.**)";
                                }
                                else
                                {
                                    phraseslist += $"`{phrase}`";
                                    methodmsg.Description = $"**__Auto role has been setup__! If you'd like to change anything consider doing the command again.** \n \n{Constants.success} **Successfully added {phraseslist} as a phrase!** \n \n(**Note: Make sure to check `k!autorole-info` for more information.**)";
                                }
                            }
                            await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                        }
                    }
                    catch
                    {
                        methodmsg.WithColor(Color.Red);
                        methodmsg.Description += $"\n \n{Constants.fail} Invalid time, try to follow the above example please and try again.";
                        await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    }

                }
                //MESSAGE TIMED METHOD
                else if (methodchosen.Content.ToLower() == "messagetimed")
                {
                    methodmsg.WithColor(Color.Green);
                    methodmsg.Description = $"\n \n{Constants.success} **After how much time would you like to give new members this role?** \nExample: 1h 15m 30s";
                    await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    var timemsg = await NextMessageAsync(true, true, new TimeSpan(0, 2, 0));
                    if (timemsg.Content.ToLower() == "kbcancel")
                    {
                        await ReplyAsync($"{Constants.success} Auto Role Setup has been canceled.");
                        return;
                    }
                    try
                    {
                        var time = TimeSpan.ParseExact(timemsg.Content, Formats, CultureInfo.CurrentCulture);
                        guild.RoleOnJoinTime = time;
                        _globalGuildAccounts.SaveAccounts(guild.Id);
                        methodmsg.Description = $"**__Auto role has been setup__! If you'd like to change anything consider doing the command again.** \n \n{Constants.success} **Success! I will now automatically give this role to users (non-bots) who send any __MESSAGE__ and after __{guild.RoleOnJoinTime}__** \n \n(**Note: Make sure to check `k!autorole-info` for more information.**)";
                    }
                    catch
                    {
                        methodmsg.WithColor(Color.Red);
                        methodmsg.Description += $"\n \n{Constants.fail} Invalid time, try to follow the above example please and try again.";
                        await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    }
                }
                //INVALID METHOD
                else if (MethodRight == false)
                {
                    methodmsg.WithColor(Color.Red);
                    methodmsg.Description = $"{Constants.fail} Please enter a valid method and make sure to type it as mentioned. This is non-repliable so please do the command again. \n(Methods Available: __Instant__ , __Timed__ , __Phrased__ , __Message__ , __PhrasedTimed__ , __MessageTimed__)";
                    await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                    return;
                }
                await setup.ModifyAsync(m => m.Embed = methodmsg.Build());
                guild.RoleOnJoinToggle = true;
                _globalGuildAccounts.SaveAccounts(guild.Id);
                #endregion
            }
        }

        [Command("autorole-info")]
        [Alias("autorole-information", "info-autorole", "ar-info", "ar-information", "autorole")]
        [Summary("Get all the information you need on the server's auto role system.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        [Cooldown(10)]
        public async Task AutoRoleInfo()
        {
            var guild = _globalGuildAccounts.GetById(Context.Guild.Id);
            if (guild.RoleOnJoin != 0 || guild.RoleOnJoinMethod != null)
            {
                var emb = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTitle("Server Auto Role Information:");

                // Toggle Info
                if (guild.RoleOnJoinToggle == true)
                    emb.Description = $"{Constants.success} The Auto Role system is toggled on. \nIf you want to turn this off use `k!autorole-toggle off`.";
                else
                    emb.Description = $"{Constants.fail} The Auto Role system is toggled off. \nMake sure to turn it on using `k!autorole-toggle on` if you want this to work.";
                // Role Info
                if (Context.Guild.GetRole(guild.RoleOnJoin) != null)
                    emb.AddField("Role", $"{Context.Guild.GetRole(guild.RoleOnJoin).Name} - {guild.RoleOnJoin}");
                else
                    emb.AddField("Role", $"[Role was Deleted, Set up a new one in `k!autorole-setup`]");

                // Method Info
                if (guild.RoleOnJoinMethod == null)
                    emb.AddField("Method", "[None Set, Set it up in `k!autorole-setup`]");
                else
                    emb.AddField("Method", guild.RoleOnJoinMethod);

                // Duration Info
                if (guild.RoleOnJoinMethod == "timed")
                {
                    emb.AddField("Given After:", $"{guild.RoleOnJoinTime}");
                }
                else if (guild.RoleOnJoinMethod == "phrasedtimed")
                {
                    emb.AddField("Given After:", $"{guild.RoleOnJoinTime} - After the user sends the phrase(s)");
                }
                else if (guild.RoleOnJoinMethod == "messagetimed")
                {
                    emb.AddField("Given After:", $"{guild.RoleOnJoinTime} - After the user sends a message");
                }

                // Phrases (If method is chosen)
                if (guild.RoleOnJoinMethod == "phrased" || guild.RoleOnJoinMethod == "phrasedtimed")
                {
                    var phraseslist = "";
                    foreach (var phrase in guild.RoleOnJoinPhrase)
                    {
                        phraseslist += $"`{phrase}`, ";
                    }
                    if (guild.RoleOnJoinPhrase.Count == 0)
                        phraseslist = "[None Set, please do `k!autorole-setup` again to set it]";
                    emb.AddField("Phrase(s):", $"{phraseslist}");
                }
                // Method Description & Permissions needed
                if (guild.RoleOnJoinMethod != null)
                {
                    string description = "";
                    string permissions = "";
                    if (guild.RoleOnJoinMethod == "instant")
                    {
                        description = "KillerBot will automatically give new members (bots ignored) the role you set instantly.";
                        permissions = "KillerBot should: Be able to give users the role you have set. \n(**Manage Roles** permission and a role higher than the one you need to give)";
                    }
                    else if (guild.RoleOnJoinMethod == "timed")
                    {
                        description = "KillerBot will automatically give new members (bots ignored) the role you set after the specified time you set.";
                        permissions = "KillerBot should: Be able to give users the role you have set. \n(**Manage Roles** permission and a role higher than the one you need to give)";
                    }
                    else if (guild.RoleOnJoinMethod == "phrased")
                    {
                        description = "KillerBot will automatically give members (bots ignored) the role you set instantly after they send (a) specific phrase(s) that you set.";
                        permissions = "KillerBot should: Have access to the channel the user sends the message in, and be able to give users the role you have set. \n(Channel **Read Messages** permission, **Manage Roles** permission, and a role higher than the one you need to give)";
                    }
                    else if (guild.RoleOnJoinMethod == "message")
                    {
                        description = "KillerBot will automatically give members (bots ignored) the role you set instantly after they send any message.";
                        permissions = "KillerBot should: Have access to the channel the user sends the message in, and be able to give users the role you have set. \n(Channel **Read Messages** permission, **Manage Roles** permission, and a role higher than the one you need to give)";
                    }
                    else if (guild.RoleOnJoinMethod == "phrasedtimed")
                    {
                        description = "KillerBot will automatically give members (bots ignored) the role you set after a specified time from the user sending the phrase(s) you set.";
                        permissions = "KillerBot should: Have access to the channel the user sends the message in, and be able to give users the role you have set. \n(Channel **Read Messages** permission, **Manage Roles** permission, and a role higher than the one you need to give)";
                    }
                    else if (guild.RoleOnJoinMethod == "mesagetimed")
                    {
                        description = "KillerBot will automatically give members (bots ignored) the role you set after a specified time from the user sending any message";
                        permissions = "KillerBot should: Have access to the channel the user sends the message in, and be able to give users the role you have set. \n(Channel **Read Messages** permission, **Manage Roles** permission, and a role higher than the one you need to give)";
                    }

                    emb.AddField("Method Description:", $"{description}");
                    emb.AddField("Required Permissions:", $"{permissions}");
                }
                await ReplyAsync("", false, emb.Build());
            }
            else
            {
                var emb = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithDescription($"{Constants.fail} This server has no auto role set up, please read below for more information on how to set it up.")
                    .WithTitle("Server Auto Role Information:");
                emb.AddField("How To Set It Up:", "1. **Set up the role and method using the command `k!autorole-setup`** \n \n2. **Toggle on the system if you want it to start working using the `k!autorole-toggle` command in case you toggled it off.** \n \n3. **Make sure KillerBot has enough permissions to do the job. You can do this command again after setting up everything and it will let you know about the permissions it needs.**");
                await ReplyAsync("", false, emb.Build());
            }
        }

        public async Task Method(SocketGuildUser user, string method)
        {
            var guild = _globalGuildAccounts.GetById(user.Guild.Id);
            var autorole = user.Guild.GetRole(guild.RoleOnJoin);
            if (autorole != null)
            {
                if (method == "instant")
                {
                    await GiveRoleInstantly(user);
                }
                else if (method == "timed")
                {
                    await GiveRoleOnTime(user);
                }
            }
        }
        public async Task Method2(SocketGuildUser user, string method, SocketMessage msg)
        {
            var guild = _globalGuildAccounts.GetById(user.Guild.Id);
            var autorole = user.Guild.GetRole(guild.RoleOnJoin);
            if (autorole != null)
            {
                if (method == "phrased")
                {
                    await GiveRoleOnPhrase(user, msg);
                }
                else if (method == "message")
                {
                    await GiveRoleOnMessage(user, msg);
                }
                else if (method == "phrasedtimed")
                {
                    await GiveRoleOnTimePhrased(user, msg);
                }
                else if (method == "messagetimed")
                {
                    await GiveRoleOnTimeMessage(user, msg);
                }
            }
        }
        public async Task GiveRoleInstantly(SocketGuildUser user)
        {
            var guild = _globalGuildAccounts.GetById(user.Guild.Id);
            var onjoinrole = user.Guild.GetRole(guild.RoleOnJoin);
            try
            {
                await user.AddRoleAsync(onjoinrole, options: new RequestOptions { AuditLogReason = "Auto Role [Instant]" });
            }
            catch
            {

            }

        }
        public async Task GiveRoleOnMessage(SocketGuildUser user, SocketMessage msg)
        {
            var guild = _globalGuildAccounts.GetById(user.Guild.Id);
            var onjoinrole = user.Guild.GetRole(guild.RoleOnJoin);
            if (msg.Content != null)
            {
                try
                {
                    await user.AddRoleAsync(onjoinrole, options: new RequestOptions { AuditLogReason = "Auto Role [On Message]" });
                }
                catch
                {

                }
            }
        }
        public async Task GiveRoleOnPhrase(SocketGuildUser user, SocketMessage msg)
        {
            var guild = _globalGuildAccounts.GetById(user.Guild.Id);
            var onjoinrole = user.Guild.GetRole(guild.RoleOnJoin);
            foreach (var phrase in guild.RoleOnJoinPhrase)
            {
                if (msg.Content == phrase)
                {
                    try
                    {
                        await user.AddRoleAsync(onjoinrole, options: new RequestOptions { AuditLogReason = "Auto Role [By Phrase]" });
                    }
                    catch
                    {

                    }
                }
            }
        }
        public async Task GiveRoleOnTime(SocketGuildUser user)
        {
            var guild = _globalGuildAccounts.GetById(user.Guild.Id);
            var onjoinrole = user.Guild.GetRole(guild.RoleOnJoin);
            if (guild.RoleOnJoinTime != (new TimeSpan(0,0,0)))
                await Task.Delay(guild.RoleOnJoinTime);
            try
            {
                await user.AddRoleAsync(onjoinrole, options: new RequestOptions { AuditLogReason = "Auto Role [Timed]" });
            }
            catch
            {

            }
        }
        public async Task GiveRoleOnTimePhrased(SocketGuildUser user, SocketMessage msg)
        {
            var guild = _globalGuildAccounts.GetById(user.Guild.Id);
            var onjoinrole = user.Guild.GetRole(guild.RoleOnJoin);
            foreach (var phrase in guild.RoleOnJoinPhrase)
            {
                if (msg.Content == phrase)
                {
                    if (guild.RoleOnJoinTime != (new TimeSpan(0, 0, 0)))
                        await Task.Delay(guild.RoleOnJoinTime);
                    try
                    {
                        await user.AddRoleAsync(onjoinrole, options: new RequestOptions { AuditLogReason = "Auto Role [By Phrase + Timed]" });
                    }
                    catch
                    {

                    }
                }
            }
        }
        public async Task GiveRoleOnTimeMessage(SocketGuildUser user, SocketMessage msg)
        {
            var guild = _globalGuildAccounts.GetById(user.Guild.Id);
            var onjoinrole = user.Guild.GetRole(guild.RoleOnJoin);
            if (msg.Content != null)
            {
                if (guild.RoleOnJoinTime != (new TimeSpan(0, 0, 0)))
                    await Task.Delay(guild.RoleOnJoinTime);
                try
                {
                    await user.AddRoleAsync(onjoinrole, options: new RequestOptions { AuditLogReason = "Auto Role [On Message + Timed]" });
                }
                catch
                {

                }
            }
        }
    }
}