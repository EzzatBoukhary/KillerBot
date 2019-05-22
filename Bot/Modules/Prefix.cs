using System.Linq;
using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;
using Discord;
using Discord.Commands;

namespace Bot.Modules
{
    [Group("Prefix"), Alias("Prefixes"), Summary("Setting for the Bots prefix on this server")]
    [RequireContext(ContextType.Guild)]
    public class Prefix : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        public Prefix(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }

        [Command("add"), Alias("set"), RequireUserPermission(GuildPermission.Administrator)]
        [Remarks("Adds a prefix to the list of prefixes")]
        public async Task AddPrefix([Remainder] string prefix)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var response = $"Failed to add the Prefix... Was `{prefix}` already a prefix?";
            if (!guildAcc.Prefixes.Contains(prefix))
            {
                var prefixes = guildAcc.Prefixes.ToList();
                guildAcc.Modify(g => g.SetPrefixes(prefixes.Append(prefix).ToList()), _globalGuildAccounts);
                response =  $"Successfully added `{prefix}` as prefix! <a:KBtick:580851374070431774>";
            }

            await ReplyAsync(response);
        }

        [Command("remove"), Remarks("Removes a prefix from the list of prefixes")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemovePrefix([Remainder] string prefix)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var response = $"<:KBfail:580129304592252995> Failed to remove the Prefix... Was `{prefix}` really a prefix?";
            if (guildAcc.Prefixes.Contains(prefix))
            {
                var prefixes = guildAcc.Prefixes.ToList();
                prefixes.Remove(prefix);
                guildAcc.Modify(g => g.SetPrefixes(prefixes), _globalGuildAccounts);
                response =  $"Successfully removed `{prefix}` as possible prefix!";
            }

            await ReplyAsync(response);
        }

        [Command("list"), Remarks("Show all possible prefixes for this server")]
        public async Task ListPrefixes()
        {
            var prefixes = _globalGuildAccounts.GetById(Context.Guild.Id).Prefixes;
            var response = "No Prefix set yet... just mention me to use commands!";
            if (prefixes.Count != 0) response = "Usable Prefixes are:\n`" + string.Join("`, `", prefixes) + "`\nOr just mention me!";
            await ReplyAsync(response);
        }
    }
}
