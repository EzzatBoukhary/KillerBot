using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Features.GlobalAccounts;
using Bot.Features.RoleAssignment;
using Discord;

namespace Bot.Providers
{
    public class RoleByPhraseProvider
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        public RoleByPhraseProvider(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }

        public enum RoleByPhraseOperationResult { Success, AlreadyExists, Failed }

        public RoleByPhraseOperationResult AddPhrase(IGuild guild, string phrase)
        {
            var guildSettings = _globalGuildAccounts.GetFromDiscordGuild(guild);

            try
            {
                guildSettings.RoleByPhraseSettings.AddPhrase(phrase);
                _globalGuildAccounts.SaveAccounts();
                return RoleByPhraseOperationResult.Success;
            }
            catch (PhraseAlreadyAddedException)
            {
                return RoleByPhraseOperationResult.AlreadyExists;
            }
            catch (Exception)
            {
                return RoleByPhraseOperationResult.Failed;
            }
        }

        public RoleByPhraseOperationResult AddRole(IGuild guild, IRole role)
        {
            var guildSettings = _globalGuildAccounts.GetFromDiscordGuild(guild);

            if (guild.GetRole(role.Id) is null) return RoleByPhraseOperationResult.Failed;

            try
            {
                guildSettings.RoleByPhraseSettings.AddRole(role.Id);
                _globalGuildAccounts.SaveAccounts();
                return RoleByPhraseOperationResult.Success;
            }
            catch (RoleIdAlreadyAddedException)
            {
                return RoleByPhraseOperationResult.AlreadyExists;
            }
            catch (Exception)
            {
                return RoleByPhraseOperationResult.Failed;
            }
        }

        public RoleByPhraseOperationResult ForceRelation(IGuild guild, string phrase, IRole role)
        {
            var guildSettings = _globalGuildAccounts.GetFromDiscordGuild(guild);

            if (guild.GetRole(role.Id) is null) return RoleByPhraseOperationResult.Failed;

            try
            {
                guildSettings.RoleByPhraseSettings.ForceCreateRelation(phrase, role.Id);
                _globalGuildAccounts.SaveAccounts();
                return RoleByPhraseOperationResult.Success;
            }
            catch (RelationAlreadyExistsException)
            {
                return RoleByPhraseOperationResult.AlreadyExists;
            }
            catch (Exception)
            {
                return RoleByPhraseOperationResult.Failed;
            }
        }

        public enum RelationCreationResult { Success, InvalidIndex, AlreadyExists, Failed }

        public RelationCreationResult CreateRelation(IGuild guild, int phraseIndex, int roleIdIndex)
        {
            var guildSettings = _globalGuildAccounts.GetFromDiscordGuild(guild);

            try
            {
                guildSettings.RoleByPhraseSettings.CreateRelation(phraseIndex, roleIdIndex);
                _globalGuildAccounts.SaveAccounts();
                return RelationCreationResult.Success;
            }
            catch (ArgumentException)
            {
                return RelationCreationResult.InvalidIndex;
            }
            catch (RelationAlreadyExistsException)
            {
                return RelationCreationResult.AlreadyExists;
            }
            catch (Exception)
            {
                return RelationCreationResult.Failed;
            }
        }

        public void RemovePhrase(IGuild guild, int phraseIndex)
        {
            var guildSettings = _globalGuildAccounts.GetFromDiscordGuild(guild);

            guildSettings.RoleByPhraseSettings.RemovePhraseByIndex(phraseIndex);
            _globalGuildAccounts.SaveAccounts();
        }

        public void RemoveRole(IGuild guild, int roleIdIndex)
        {
            var guildSettings = _globalGuildAccounts.GetFromDiscordGuild(guild);

            guildSettings.RoleByPhraseSettings.RemoveRoleIdByIndex(roleIdIndex);
            _globalGuildAccounts.SaveAccounts();
        }

        public void RemoveRelation(IGuild guild, int phraseIndex, int roleIdIndex)
        {
            var guildSettings = _globalGuildAccounts.GetFromDiscordGuild(guild);

            guildSettings.RoleByPhraseSettings.RemoveRelation(phraseIndex, roleIdIndex);
            _globalGuildAccounts.SaveAccounts();
        }

        public async Task EvaluateMessage(IGuild guild, string message, IGuildUser sender)
        {
            var guildSettings = _globalGuildAccounts.GetFromDiscordGuild(guild);

            var triggeredPhrases = guildSettings.RoleByPhraseSettings.Phrases.Where(message.Contains).ToList();

            if (!triggeredPhrases.Any()) return;

            var roleIdsToGet = new List<ulong>();

            foreach (var phrase in triggeredPhrases)
            {
                var phraseIndex = guildSettings.RoleByPhraseSettings.Phrases.IndexOf(phrase);
                var roleIds = guildSettings.RoleByPhraseSettings.Relations
                    .Where(r => r.PhraseIndex == phraseIndex)
                    .Select(r => guildSettings.RoleByPhraseSettings.RolesIds.ElementAt(r.RoleIdIndex))
                    .ToList();

                foreach (var roleId in roleIds)
                {
                    if (roleIdsToGet.Contains(roleId)) continue;
                    roleIdsToGet.Add(roleId);
                }
            }

            foreach (var roleId in roleIdsToGet)
            {
                if (sender.RoleIds.Contains(roleId)) continue;
                var role = guild.GetRole(roleId);
                if(role is null) continue;
                await sender.AddRoleAsync(role);
            }
        }
    }
}