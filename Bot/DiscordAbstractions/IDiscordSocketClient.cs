using Discord;

namespace Bot.DiscordAbstractions
{
    public interface IDiscordSocketClient
    {
        ISelfUser GetCurrentUser();
    }
}