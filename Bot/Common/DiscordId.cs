﻿using System;

namespace Bot.Common
{
    /// <summary>
    /// Provides a unique id as a combination of GuildId, ChannelId and UserId. Id parts that are null and will be ignored
    /// </summary>
    public class DiscordId : IEquatable<DiscordId>
    {
        public ulong? GuildId { get; }
        public ulong? ChannelId { get; }
        public ulong? UserId { get; }

        public DiscordId()
        {
        }

        public DiscordId(ulong? guildId, ulong? channelId, ulong? userId)
        {
            if (guildId == null && channelId == null && userId == null)
            {
                throw new ArgumentNullException($"{nameof(guildId)}, {nameof(channelId)}, {nameof(userId)}", "Provide at least one id");
            }
            GuildId = guildId;
            ChannelId = channelId;
            UserId = userId;
        }

        public override bool Equals(object obj)
            => (obj is DiscordId) && Equals(obj as DiscordId);

        public bool Equals(DiscordId other)
        {
            if (other == null)
            {
                return false;
            }
            //if both IDs have a value then the values must be equal or either one has no value (is null) then this id part can be ignored
            bool guild = (GuildId.HasValue
                          && other.GuildId.HasValue
                          && (GuildId == other.GuildId)) || !GuildId.HasValue || !other.GuildId.HasValue;
            bool channel = (ChannelId.HasValue
                            && other.ChannelId.HasValue
                            && (ChannelId == other.ChannelId)) || !ChannelId.HasValue || !other.ChannelId.HasValue;
            bool user = (UserId.HasValue
                         && other.UserId.HasValue
                         && (UserId == other.UserId)) || !UserId.HasValue || !other.UserId.HasValue;
            return (guild && channel && user);
        }

        public override int GetHashCode()
            => (GuildId, ChannelId, UserId).GetHashCode();
    }
}