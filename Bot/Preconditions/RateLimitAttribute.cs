﻿//taken from https://github.com/Joe4evr/Discord.Addons

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Bot.Preconditions
{
    /// <summary>
    ///     Sets how often a user is allowed to use this command
    ///     or any command in this module.
    /// </summary>
    /// <remarks>
    ///     This is backed by an in-memory collection
    ///     and will not persist with restarts.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class RatelimitAttribute : PreconditionAttribute
    {
        private readonly Logger _logger;
        public RatelimitAttribute(Logger logger)
        {
            _logger = logger;

        }
        private readonly bool _applyPerGuild;
        private readonly bool _applyPerUser;
        private readonly uint _invokeLimit;
        private readonly TimeSpan _invokeLimitPeriod;

        private readonly Dictionary<(ulong, ulong?), CommandTimeout> _invokeTracker =
            new Dictionary<(ulong, ulong?), CommandTimeout>();

        private readonly bool _noLimitForAdmins;
        private readonly bool _noLimitInDMs;

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="measure">The scale in which the <paramref name="period" /> parameter should be measured.</param>
        /// <param name="flags">Flags to set behavior of the ratelimit.</param>
        public RatelimitAttribute(
            uint times,
            double period,
            Measure measure,
            RatelimitFlags flags = RatelimitFlags.None)
        {
            _invokeLimit = times;
            _noLimitInDMs = (flags & RatelimitFlags.NoLimitInDMs) == RatelimitFlags.NoLimitInDMs;
            _noLimitForAdmins = (flags & RatelimitFlags.NoLimitForAdmins) == RatelimitFlags.NoLimitForAdmins;
            _applyPerGuild = (flags & RatelimitFlags.ApplyPerGuild) == RatelimitFlags.ApplyPerGuild;
            _applyPerUser = (flags & RatelimitFlags.ApplyPerUser) == RatelimitFlags.ApplyPerUser;

            //TODO: C# 8 candidate switch expression
            switch (measure)
            {
                case Measure.Days:
                    _invokeLimitPeriod = TimeSpan.FromDays(period);
                    break;
                case Measure.Hours:
                    _invokeLimitPeriod = TimeSpan.FromHours(period);
                    break;
                case Measure.Minutes:
                    _invokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;
            }
        }
        
        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="flags">Flags to set bahavior of the ratelimit.</param>
        public RatelimitAttribute(
            uint times,
            TimeSpan period,
            RatelimitFlags flags = RatelimitFlags.None
            )
        {
            _invokeLimit = times;
            _noLimitInDMs = (flags & RatelimitFlags.NoLimitInDMs) == RatelimitFlags.NoLimitInDMs;
            _noLimitForAdmins = (flags & RatelimitFlags.NoLimitForAdmins) == RatelimitFlags.NoLimitForAdmins;
            _applyPerGuild = (flags & RatelimitFlags.ApplyPerGuild) == RatelimitFlags.ApplyPerGuild;
            _applyPerUser = (flags & RatelimitFlags.ApplyPerUser) == RatelimitFlags.ApplyPerUser;

            _invokeLimitPeriod = period;
        }

        /// <inheritdoc />
        
        public override Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context,
            CommandInfo command,
            IServiceProvider services)
        {
            if (_noLimitInDMs && context.Channel is IPrivateChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());

            if (_noLimitForAdmins && context.User is IGuildUser gu && gu.GuildPermissions.Administrator)
                return Task.FromResult(PreconditionResult.FromSuccess());

            if (_applyPerUser && context.User != context.User)
                return Task.FromResult(PreconditionResult.FromSuccess());

            var now = DateTime.UtcNow;
            var key = _applyPerGuild ? (context.User.Id, context.Guild?.Id) : (context.User.Id, null);

            var timeout = _invokeTracker.TryGetValue(key, out var t)
                          && now - t.FirstInvoke < _invokeLimitPeriod
                ? t
                : new CommandTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked <= _invokeLimit)
            {
                _invokeTracker[key] = timeout;
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            //(LogSeverity.Critical, $"{DateTime.Now}", $"{context.User} has triggered the rate limit of a command. ({context.User.Id})");
            //Console.Write($"{context.User} has triggered the rate limit of a command. ({context.User.Id})");
           // Console.WriteLine($"{context.User} has triggered the rate limit of a command. ({context.User.Id})");
            
            return Task.FromResult(PreconditionResult.FromError("Cooldown! You have broke the rate limit of the command."));
            
            
        }

        private sealed class CommandTimeout
        {
            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }

            public uint TimesInvoked { get; set; }
            public DateTime FirstInvoke { get; }
        }
    }

    /// <summary> Sets the scale of the period parameter. </summary>
    public enum Measure
    {
        /// <summary> Period is measured in days. </summary>
        Days,

        /// <summary> Period is measured in hours. </summary>
        Hours,

        /// <summary> Period is measured in minutes. </summary>
        Minutes
    }

    /// <summary> Used to set behavior of the ratelimit </summary>
    [Flags]
    public enum RatelimitFlags
    {
        /// <summary> Set none of the flags. </summary>
        None = 0,

        /// <summary> Set whether or not there is no limit to the command in DMs. </summary>
        NoLimitInDMs = 1 << 0,

        /// <summary> Set whether or not there is no limit to the command for guild admins. </summary>
        NoLimitForAdmins = 1 << 1,

        /// <summary> Set whether or not to apply a limit per guild. </summary>
        ApplyPerGuild = 1 << 2,
        /// <summary> Set whether or not to apply a limit per user. </summary>
        ApplyPerUser = 1 << 3
    }
}