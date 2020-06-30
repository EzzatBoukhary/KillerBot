using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;
using Bot.Features.RoleAssignment;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Bot.Providers
{
    public class RoleByPhraseProvider : ModuleBase<MiunieCommandContext>
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

        public async Task EvaluateMessage(IGuild guild, ISocketMessageChannel channel ,string message, IGuildUser sender)
        {
            var guildSettings = _globalGuildAccounts.GetFromDiscordGuild(guild);
            var message2 = "";
            if (guild is null) return;
            var prefixes = _globalGuildAccounts.GetById(guild.Id).Prefixes;
            var defaultprefix = "k!";
            var allprefixes = new List<string>()
            {
                defaultprefix
            };
            foreach (var prefix in prefixes)
                allprefixes.Add(prefix);
            var tmpArgPos = 0;
            var triggeredPhrases = guildSettings.RoleByPhraseSettings.Phrases.Where(message.Contains).ToList();

            if (!triggeredPhrases.Any()) return;

            var roleIdsToGet = new List<ulong>();

            foreach (var phrase in triggeredPhrases)
            {
                var success = allprefixes.Any(pre =>
                {
                    if (!message.StartsWith(pre)) return false;
                    tmpArgPos = pre.Length;
                    message2 = message.Remove(0, tmpArgPos);
                    if (!message.EndsWith(phrase)) return false;
                    if (phrase != message2) return false;
                    return true;
                });
                if (success == false)
                    return;
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
                
                var role = guild.GetRole(roleId);
                if(role is null) continue;
                if (sender.RoleIds.Contains(roleId))
                {
                    await sender.RemoveRoleAsync(role);
                    await channel.SendMessageAsync($"<a:SuccessKB:639875484972351508> I have taken away from you **{role.Name}**!");
                    return;
                }
                try
                {
                    await sender.AddRoleAsync(role);
                    await channel.SendMessageAsync($"<a:SuccessKB:639875484972351508> I have given you **{role.Name}**!");
                }
                catch
                {
                    await channel.SendMessageAsync($"<:KBfail:580129304592252995> Something went wrong...please make sure I have permissions to give you that role.");
                }
            }
        }
    }
}