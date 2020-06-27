using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Modules.Fun
{
    public abstract class EconomyGame
    {
        public readonly List<SocketUser> Players = new List<SocketUser>();
        public int PlayerCount => Players.Count();
        public abstract string Name { get; }

    }
}
