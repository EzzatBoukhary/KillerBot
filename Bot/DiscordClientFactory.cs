using Bot.Configuration;
using Discord;
using Discord.WebSocket;

namespace Bot
{
    public static class DiscordClientFactory
    {
        public static DiscordSocketClient GetBySettings(ApplicationSettings settings)
        {
            if(settings is null) return new DiscordSocketClient();

            return new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = settings.Verbose ? LogSeverity.Verbose : LogSeverity.Info,
                MessageCacheSize = settings.CacheSize,
                AlwaysDownloadUsers = true
            });
        }
    }
}
