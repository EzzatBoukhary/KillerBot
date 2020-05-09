using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;
using Bot.Providers;
using Discord;
using Discord.Commands;

namespace Bot.Modules.RoleAssignments
{
    [Group("RoleByPhrase"), Alias("rbp"), Summary("Settings for auto-assigning roles based on a sent Phrase")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class RoleByPhrase : ModuleBase<MiunieCommandContext>
    {
        private readonly RoleByPhraseProvider _roleByPhraseProvider;
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        public RoleByPhrase (GlobalGuildAccounts globalGuildAccounts, RoleByPhraseProvider roleByPhraseProvider)
        {
            _globalGuildAccounts = globalGuildAccounts;
            _roleByPhraseProvider = roleByPhraseProvider;
        }
        /* [Command("list")]
        [Summary("Sends the list of self-roles if there is any setup in the server.")]
        public async Task RbpList()
        {
            var rbp = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild).RoleByPhraseSettings;
            
            var relations = rbp.Relations.Any() ? string.Join("\n", rbp.Relations.Select(r => $"Phrase {r.PhraseIndex} => Role: {r.RoleIdIndex}")) : "There are no self-roles setup in this server. \n \nIf you have Admin perms and want to add some do `k!rbp status`.";
            var embed = new EmbedBuilder();
            embed.WithColor(Color.Blue);
            embed.WithFooter("To gain self-roles do: k!(phrase)");
            embed.WithTitle($"Self-Roles for {Context.Guild.Name}");
            embed.WithDescription(relations);

            embed.WithDescription("")
        } */
        [Command("status"), Alias("s"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Returns the current state of RoleByPhrase lists and relations.")]
        public async Task RbpStatus()
        {
            var rbp = _globalGuildAccounts.GetFromDiscordGuild(Context.Guild).RoleByPhraseSettings;

            var phrases = rbp.Phrases.Any() ? string.Join("\n", rbp.Phrases.Select(p => $"({rbp.Phrases.IndexOf(p)}) - {p}")) : "No phrases stored\nAdd one with `rbp addPhrase YOUR-PHRASE`";
            var roles = rbp.RolesIds.Any() ? string.Join("\n", rbp.RolesIds.Select(r => $"({rbp.RolesIds.IndexOf(r)}) - {Context.Guild.GetRole(r).Name}")) : "No roles stored\nAdd one with `rbp addRole @SomeRole`";
            var relations = rbp.Relations.Any() ? string.Join("\n", rbp.Relations.Select(r => $"Phrase {r.PhraseIndex} => Role {r.RoleIdIndex}")) : "No relations created\nAdd one with `rbp addRelation PHRASE-ID ROLE-ID`";

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Blue);
            embed.WithTitle($"Role Assignments for {Context.Guild.Name}");
            embed.AddField("Phrases", phrases);
            embed.AddField("Roles", roles);
            embed.AddField("Relations", relations);
            embed.WithFooter(Global.GetRandomDidYouKnow());
            embed.WithCurrentTimestamp();

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("addPhrase"), Alias("ap"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Adds a new phrase to the guild's settings. (Phrase is a Remainder, so no double quotes are needed)")]
        public async Task RbpAddPhrase([Remainder]string phrase)
        {
            var result = _roleByPhraseProvider.AddPhrase(Context.Guild, phrase);

            if (result == RoleByPhraseProvider.RoleByPhraseOperationResult.Success)
            {
                await RbpStatus();
            }
            else
            {
                await ReplyAsync("I work 24 hours a day so something went wrong...");
                Console.WriteLine(result.ToString());
            }
        }

        [Command("addRole"), Alias("arole"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Adds a new role to the guild's settings. (The role is a Remainder, so no double quotes are needed)")]
        public async Task RbpAddRole([Remainder]IRole role)
        {
            var result = _roleByPhraseProvider.AddRole(Context.Guild, role);

            if (result == RoleByPhraseProvider.RoleByPhraseOperationResult.Success)
            {
                await RbpStatus();
            }
            else
            {
                await ReplyAsync("Well, this was a total waste of time, something went wrong...");
                Console.WriteLine(result.ToString());
            }
        }

        [Command("addRelation"), Alias("arel"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Adds a new relation between a phrase and a role. Relation are automatically enabled and used after you add them.")]
        public async Task RbpAddRelation(int phraseIndex, int roleIndex)
        {
            var result = _roleByPhraseProvider.CreateRelation(Context.Guild, phraseIndex, roleIndex);

            if (result == RoleByPhraseProvider.RelationCreationResult.Success)
            {
                await RbpStatus();
            }
            else
            {
                await ReplyAsync("That's just what I need, great! Terrific!, something went wrong...");
                Console.WriteLine(result.ToString());
            }
        }

        [Command("removeRelation"), Alias("rrel"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Removes a relation between a phrase and a role.")]
        public async Task RbpRemoveRelation(int phraseIndex, int roleIndex)
        {
            _roleByPhraseProvider.RemoveRelation(Context.Guild, phraseIndex, roleIndex);
            await RbpStatus();
        }

        [Command("removePhrase"), Alias("rp"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Removes a phrase and its relations.")]
        public async Task RbpRemovePhrase(int phraseIndex)
        {
            _roleByPhraseProvider.RemovePhrase(Context.Guild, phraseIndex);
            await RbpStatus();
        }

        [Command("removeRole"), Alias("rrole"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Removes a role and its relations.")]
        public async Task RbpRemoveRole(int roleIndex)
        {
            _roleByPhraseProvider.RemoveRole(Context.Guild, roleIndex);
            await RbpStatus();
        }
    }
}