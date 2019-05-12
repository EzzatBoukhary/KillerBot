using System.Threading.Tasks;
using Bot.Features.GlobalAccounts;
using Discord.WebSocket;

namespace Bot.Features
{
    public class Announcements
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;

        public Announcements(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }

       
            /*var dmChannel = await user.GetOrCreateDMChannelAsync();
            var possibleMessages = _globalGuildAccounts.GetById(user.Guild.Id).WelcomeMessages;
            var messageString = possibleMessages[Global.Rng.Next(possibleMessages.Count)];
            messageString = messageString.ReplacePlacehoderStrings(user);
            if (string.IsNullOrEmpty(messageString)) return;
            await Channel.SendMessageAsync(messageString); */
            public async Task UserJoined(SocketGuildUser user, DiscordSocketClient client)
            {
                var guildAcc = _globalGuildAccounts.GetById(user.Guild.Id);
                if (guildAcc.AnnouncementChannelId == 0) return;
                if (!(client.GetChannel(guildAcc.AnnouncementChannelId) is SocketTextChannel channel)) return;
                var possibleMessages = guildAcc.WelcomeMessages;
                var messageString = possibleMessages[Global.Rng.Next(possibleMessages.Count)];
                messageString = messageString.ReplacePlacehoderStrings(user);
                if (string.IsNullOrEmpty(messageString)) return;
                await channel.SendMessageAsync(messageString);
            }
        

        public async Task UserLeft(SocketGuildUser user, DiscordSocketClient client)
        {
            var guildAcc = _globalGuildAccounts.GetById(user.Guild.Id);
            if (guildAcc.AnnouncementChannelId == 0) return;
            if (!(client.GetChannel(guildAcc.AnnouncementChannelId) is SocketTextChannel channel)) return;
            var possibleMessages = guildAcc.LeaveMessages;
            var messageString = possibleMessages[Global.Rng.Next(possibleMessages.Count)];
            messageString = messageString.ReplacePlacehoderStrings(user);
            if (string.IsNullOrEmpty(messageString)) return;
            await channel.SendMessageAsync(messageString);
        }
    }
}
