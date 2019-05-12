using Bot.Entities;
using Bot.Features.GlobalAccounts;
using Discord.Commands;
using Discord.WebSocket;

namespace Bot.Extensions
{
    public class MiunieCommandContext : SocketCommandContext
    {
        public GlobalUserAccount UserAccount { get; }
        private readonly GlobalUserAccounts _globalUserAccounts;
        
        public MiunieCommandContext(DiscordSocketClient client, SocketUserMessage msg, GlobalUserAccounts globalUserAccounts) : base(client, msg)
        {
            this._globalUserAccounts = globalUserAccounts;

            if (User is null) { return; }

            UserAccount = globalUserAccounts.GetFromDiscordUser(User);
        }

        public void RegisterCommandUsage()
        {
            var commandUsedInformation = new CommandInformation(Message.Content, Message.CreatedAt.DateTime);
            
            UserAccount.AddCommandToHistory(commandUsedInformation);

            _globalUserAccounts.SaveAccounts(UserAccount.Id);
        }
    }
}
