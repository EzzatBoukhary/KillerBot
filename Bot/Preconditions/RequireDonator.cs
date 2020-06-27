using Bot.Features.GlobalAccounts;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Preconditions
{
    public class RequireDonator : PreconditionAttribute
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        public RequireDonator(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            try
            {
                var kbhq = await context.Client.GetGuildAsync(550064334714175512);
                if (kbhq == null)
                {
                    return await Task.FromResult(PreconditionResult.FromError("Something is wrong from our side, please report this error because I couldn't find the bot's server."));
                }
                var donorrole = kbhq.GetRole(709847879199621190);
                if (donorrole == null)
                {
                    return await Task.FromResult(PreconditionResult.FromError("Something is wrong from our side, please report this error because I couldn't find the donor role on the bot's server."));
                }
                var user = kbhq.GetUserAsync(context.User.Id).Result;
                var guild = _globalGuildAccounts.GetById(context.Guild.Id);
                if (!user.RoleIds.Contains(donorrole.Id) && guild.KBPremium == false)
                {
                    return await Task.FromResult(PreconditionResult.FromError("This is a Donator only command!"));
                }
            }
            catch
            {
                return await Task.FromResult(PreconditionResult.FromError("Something went wrong."));
            }
            return await Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
