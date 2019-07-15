/*using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Bot.Modules
{
    public class ServerSetup : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;

        public ServerSetup (GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }

        [Command("offLog")]
        [RequireUserPermission(GuildPermission.Administrator)]
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
        public async Task SetServerActivivtyLog(ulong logChannel = 0)
        {
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);

            if (logChannel != 0)
            {
                try
                {
                    var channel = Context.Guild.GetTextChannel(logChannel);
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



         [Command("SetRoleOnJoin")]
        [Alias("RoleOnJoin")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SetRoleOnJoin([Remainder] SocketRole role)
        {

            string text;
            var guild = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild);
            if (role == null)
            {
                guild.RoleOnJoin = null;
                text = $"No one will get role on join from me!";
            }
            else
            {
                guild.RoleOnJoin = role;
                text = $"Everyone will now be getting {role} role on join!";
            }

            _globalGuildAccounts.SaveAccounts(Context.Guild.Id);
            guild.Modify(g => g.SetRoleOnJoin(role), _globalGuildAccounts);
            await ReplyAsync(text);

        }
        public async Task GiveRole(SocketGuildUser user)
        {
            var guild = _globalGuildAccounts.GetById(user.Guild.Id);
            if (guild.RoleOnJoin != null)
            {
                await user.AddRoleAsync(guild.RoleOnJoin);
            }
            else
            {
                await ReplyAsync("test");
            }
        } 
    }
} */