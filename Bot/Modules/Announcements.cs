 using System.Linq;
using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;
using Bot.Helpers;
using Discord;
using Discord.Commands;

namespace Bot.Modules
{
    [Group("Announcements"), Alias("Announcement"), Summary("Settings for announcements")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Announcements : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;

        public Announcements(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }

        [Command("SetChannel"), Alias("Set", "toggle on", "on"), RequireUserPermission(GuildPermission.Administrator)]
        [Example("k!setchannel #joins-leaves")]
        [Remarks("Sets the channel where to post announcements")]
        public async Task SetAnnouncementChannel([Summary("The #channel you want the announcements to be sent in.")]ITextChannel channel)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            guildAcc.Modify(g => g.SetAnnouncementChannelId(channel.Id), _globalGuildAccounts);
            await ReplyAsync("The Announcement-Channel has been set to " + channel.Mention);
        }

        [Command("UnsetChannel"), Alias("Unset", "Off", "toggle off"), RequireUserPermission(GuildPermission.Administrator)]
        [Example("k!unsetchannel")]
        [Remarks("Turns posting announcements to a channel off")]
        public async Task UnsetAnnouncementChannel()
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            guildAcc.Modify(g => g.SetAnnouncementChannelId(0), _globalGuildAccounts);
            await ReplyAsync("Now there is no Announcement-Channel anymore! No more Announcements from now on... RIP!");
        }
    }
    [Group("Welcome")]
    [Summary("DM a joining user a random message out of the ones defined.")]
    public class WelcomeMessages : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;

        public WelcomeMessages(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }
        [Command("add"), RequireUserPermission(GuildPermission.Administrator)]
        [Example("`k!welcome add <usermention>, welcome to **<guildname>**!`")]
        [Remarks("`Try using ```@<botname>#<botdiscriminator> help``` for all the commands of <botmention>!`\n" +
                 "Possible placeholders are: `<usermention>`, `<username>`, `<userdiscriminator>`, `<guildname>`, " +
                 "`<botname>`, `<botdiscriminator>`, `<botmention>` ")]
        public async Task AddWelcomeMessage([Remainder] string message)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var response = $"Failed to add this Welcome Message...";
            if (!guildAcc.WelcomeMessages.Contains(message))
            {
                var messages = guildAcc.WelcomeMessages.ToList();
                messages.Add(message);
                guildAcc.Modify(g => g.SetWelcomeMessages(messages), _globalGuildAccounts);
                response =  $"Successfully added ```\n{message}\n``` as Welcome Message!";
            }

            await ReplyAsync(response);
        }

        [Command("remove"), Alias("delete"), Remarks("Removes a Welcome Message from the ones availabe")]
        [Example("k!welcome remove 2")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveWelcomeMessage([Summary("The index of the message you want to delete (index can be found in `k!welcome list`")]int messageIndex)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var messages = guildAcc.WelcomeMessages.ToList();
            var response = $"Failed to remove this Welcome Message... Use the number shown in `welcome list` next to the `#` sign!";
            if (messages.Count > messageIndex - 1)
            {
                messages.RemoveAt(messageIndex - 1);
                guildAcc.Modify(g => g.SetWelcomeMessages(messages), _globalGuildAccounts);
                response =  $"Successfully removed message #{messageIndex} as possible Welcome Message!";
            }

            await ReplyAsync(response);
        }

        [Command("list"), Remarks("Shows all currently set Welcome Messages")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListWelcomeMessages()
        {
            var welcomeMessages = _globalGuildAccounts.GetById(Context.Guild.Id).WelcomeMessages;
            var embB = new EmbedBuilder().WithTitle("No Welcome Messages set yet... add some if you want to greet incoming people! =)");
            if (welcomeMessages.Count > 0) embB.WithTitle("Possible Welcome Messages:");

            for (var i = 0; i < welcomeMessages.Count; i++)
            {
                embB.AddField($"Message #{i + 1}:", welcomeMessages[i]);
            }
            await ReplyAsync("", false, embB.Build());
        }
    }

    [Group("Leave")]
    [Summary("Announce a leaving user in the set announcement channel" +
             "with a random message out of the ones defined.")
    ]
    public class LeaveMessages : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;

        public LeaveMessages(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }

        [Command("add"), RequireUserPermission(GuildPermission.Administrator)]
        [Example("`k!leave add Oh noo! <usermention>, left <guildname>...`")]
        [Remarks("Possible placeholders are: `<usermention>`, `<username>`, `<userdiscriminator>`, `<guildname>`, " +
                 "`<botname>`, `<botdiscriminator>`, `<botmention>`")]
        public async Task AddLeaveMessage([Remainder] string message)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var response = $"Failed to add this Leave Message...";
            if (!guildAcc.LeaveMessages.Contains(message))
            {
                var messages = guildAcc.LeaveMessages.ToList();
                messages.Add(message);
                guildAcc.Modify(g => g.SetLeaveMessages(messages), _globalGuildAccounts);
                response =  $"Successfully added `{message}` as Leave Message!";
            }

            await ReplyAsync(response);
        }

        [Command("remove"),Alias("delete"), Remarks("Removes a Leave Message from the ones available")]
        [Example("k!leave remove 1")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveLeaveMessage([Summary("The index of the message you want to delete (index can be found in `k!leave list`")]int messageIndex)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var messages = guildAcc.LeaveMessages.ToList();
            var response = $"Failed to remove this Leave Message... Use the number shown in `leave list` next to the `#` sign!";
            if (messages.Count > messageIndex - 1)
            {
                messages.RemoveAt(messageIndex - 1);
                guildAcc.Modify(g => g.SetLeaveMessages(messages), _globalGuildAccounts);
                response =  $"Successfully removed message #{messageIndex} as possible Leave Message!";
            }

            await ReplyAsync(response);
        }

        [Command("list"), Remarks("Shows all currently set Leave Messages")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListLeaveMessages()
        {
            var leaveMessages = _globalGuildAccounts.GetById(Context.Guild.Id).LeaveMessages;
            var embB = new EmbedBuilder().WithTitle("No Leave Messages set yet... add some if you want a message to be shown if someone leaves.");
            if (leaveMessages.Count > 0) embB.WithTitle("Possible Leave Messages:");

            for (var i = 0; i < leaveMessages.Count; i++)
            {
                embB.AddField($"Message #{i + 1}:", leaveMessages[i]);
            }
            await ReplyAsync("", false, embB.Build());
        }
    }
} 
