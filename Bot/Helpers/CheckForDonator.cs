using Bot.Extensions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Helpers
{
    public class CheckForDonator : ModuleBase<MiunieCommandContext>
    {
        public static async Task<bool> CheckIfDonator(ulong id, ICommandContext context)
        {
            bool donator = false;

                var kbhq = await context.Client.GetGuildAsync(550064334714175512);
                if (kbhq == null)
                {
                return false;
                    //throw new ArgumentException("Couldn't find KillerBot's server, please report this error");
                }
                var donorrole = kbhq.GetRole(709847879199621190);
                if (donorrole == null)
                {
                return false;
                    //throw new ArgumentException("Couldn't find the donator role in KillerBot's server, please report this error");
                }
                var user = kbhq.GetUserAsync(id).Result;
                if (user == null)
                {
                return false;
                //throw new ArgumentException("You are not in the support server, please join the server using the link in k!server");
                }
                if (user.RoleIds.Contains(donorrole.Id))
                {
                    donator = true;
                }
            

            return await Task.FromResult(donator);
        }
    
    }
}
