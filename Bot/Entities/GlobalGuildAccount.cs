﻿using System;
using System.Collections.Generic;
using Bot.Features.GlobalAccounts;
using Bot.Features.RoleAssignment;
using Discord;
using Discord.WebSocket;
//using static Bot.Modules.ServerBots;

namespace Bot.Entities
{
    public class GlobalGuildAccount : IGlobalAccount
    {
        public GlobalGuildAccount(ulong id)
        {
            Id = id;
        }
        public ulong Id { get; }

        public ulong AnnouncementChannelId { get; set; }

        public IReadOnlyList<string> Prefixes { get; set; } = new List<string>();

        public IReadOnlyList<string> WelcomeMessages { get; set; } = new List<string> { };

        public IReadOnlyList<string> LeaveMessages { get; set; } = new List<string>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public List<WarnEntry> Warns { get; set; } = new List<WarnEntry>();

        //public Modules.ServerBots.GuildData BotData { get; private set; }

        public RoleByPhraseSettings RoleByPhraseSettings { get; set; } = new RoleByPhraseSettings();

        public int ServerActivityLog { get; set; }

        public bool KBPremium { get; set; }

        public bool Blacklisted { get; set; }

        public ulong LogChannelId { get; set; }

        public ulong RoleOnJoin { get; set; }

        public string RoleOnJoinMethod { get; set; }

        public List<string> RoleOnJoinPhrase { get; set; } = new List<string>();

        public TimeSpan RoleOnJoinTime { get; set; }

        public bool RoleOnJoinToggle { get; set; }

        /* Add more values to store */

        public GlobalGuildAccount Modify(Action<GuildAccountSettings> func, GlobalGuildAccounts globalGuildAccounts)
        {
            var settings = new GuildAccountSettings();
            func(settings);

            if (settings.AnnouncementChannelId.IsSpecified)
                AnnouncementChannelId = settings.AnnouncementChannelId.Value;
            if (settings.Prefixes.IsSpecified)
                Prefixes = settings.Prefixes.Value;
            if (settings.WelcomeMessages.IsSpecified)
                WelcomeMessages = settings.WelcomeMessages.Value;
            if (settings.LeaveMessages.IsSpecified)
                LeaveMessages = settings.LeaveMessages.Value;
            if (settings.Tags.IsSpecified)
                Tags = settings.Tags.Value;
            if (settings.KBPremium.IsSpecified)
                KBPremium = settings.KBPremium.Value;
            if (settings.RoleOnJoin.IsSpecified)
                RoleOnJoin = settings.RoleOnJoin.Value;
            if (settings.Blacklisted.IsSpecified)
                Blacklisted = settings.Blacklisted.Value;
            if (settings.Warns.IsSpecified)
                Warns = settings.Warns.Value;
            //if (settings.BotData.IsSpecified)
            //    BotData = settings.BotData.Value;
            if (settings.RoleByPhraseSettings.IsSpecified)
                RoleByPhraseSettings = settings.RoleByPhraseSettings.Value;
            globalGuildAccounts.SaveAccounts(Id);
            return this;
        }
        
        // override object.Equals
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals(obj as IGlobalAccount);
        }

        // implementation for IEquatable
        public bool Equals(IGlobalAccount other)
        {
            return Id == other.Id;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return unchecked((int)Id);
        }
    }
    public class GuildAccountSettings
    {
        public Optional<ulong> AnnouncementChannelId { get; private set; }
        public GuildAccountSettings SetAnnouncementChannelId(ulong id) { AnnouncementChannelId = id; return this; }
        

        public Optional<List<string>> Prefixes { get; private set; }
        public GuildAccountSettings SetPrefixes(List<string> prefixes) { Prefixes = prefixes; return this; }

        public Optional<List<string>> WelcomeMessages { get; private set; }
        public GuildAccountSettings SetWelcomeMessages(List<string> welcomeMessages) { WelcomeMessages = welcomeMessages; return this; }

        public Optional<List<string>> LeaveMessages { get; private set; }
        public GuildAccountSettings SetLeaveMessages(List<string> leaveMessages) { LeaveMessages = leaveMessages; return this; }

        public Optional<Dictionary<string, string>> Tags { get; private set; }
        public GuildAccountSettings SetTags(Dictionary<string, string> tags) { Tags = tags; return this; }

        public Optional<bool> KBPremium { get; private set; }
        public GuildAccountSettings SetKBPremium(bool Premium) { KBPremium = Premium; return this; }

        public Optional<ulong> RoleOnJoin { get; private set; }
        public GuildAccountSettings SetRoleOnJoin(ulong Role) { RoleOnJoin = Role; return this; }

        public Optional<bool> Blacklisted { get; private set; }
        public GuildAccountSettings SetBlacklisted(bool blacklisted) { Blacklisted = blacklisted; return this; }

        public Optional<List<WarnEntry>> Warns { get; private set; }
        public GuildAccountSettings SetWarns(List<WarnEntry> warns) { Warns = warns; return this; }
        //public Optional<GuildData> BotData { get; private set; }
       //public GuildAccountSettings SetBotData(GuildData botData) { BotData = botData; return this; }

        public Optional<RoleByPhraseSettings> RoleByPhraseSettings { get; private set; }
        public GuildAccountSettings SetBotData(RoleByPhraseSettings roleByPhraseSettings) { RoleByPhraseSettings = roleByPhraseSettings; return this; }
    }
    public struct WarnEntry
    {
        public DateTime Time;
        public string Moderator;
        public string Reason;
        public string Warned_username;
        public ulong Warned_userid;

        public WarnEntry(string moderator, string warned_username, ulong warned_userid, DateTime time, string reason)
        {
            Moderator = moderator;
            Warned_username = warned_username;
            Warned_userid = warned_userid;
            Time = time;
            Reason = reason;
        }
    }
}
