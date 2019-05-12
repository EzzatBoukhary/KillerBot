using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace Bot.Extensions
{
    public static class ClientExtensions
    {
        public static IGuild GetPrimaryGuild(this DiscordSocketClient client) => client.GetGuild(550064334714175512);
    }
}
