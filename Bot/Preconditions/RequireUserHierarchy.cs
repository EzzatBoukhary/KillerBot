using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Bot.Preconditions
{
    public class RequireUserHierarchy : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            var user = value is SocketGuildUser ? (SocketGuildUser)value : null;

            // var bot = (context.Guild as SocketGuild).GetUser(context.Client.CurrentUser.Id);
            var exec = (context.Guild as SocketGuild).GetUser(context.User.Id);
            if ((user != null) && (exec.Hierarchy == user.Hierarchy) && (exec != user))
                return Task.FromResult(PreconditionResult.FromError("You don't have enough permissions."));

            if ((user != null) && (exec.Hierarchy < user.Hierarchy))
                return Task.FromResult(PreconditionResult.FromError("You don't have enough permissions."));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}