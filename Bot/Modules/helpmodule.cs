/*using Bot.Extensions;
using Bot.Preconditions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
    public class helpmodule : ModuleBase<MiunieCommandContext>
    {
        private helpmodule()
        {

        }

        [Cooldown(8)]
        [Command("help"), Alias("h"),
         Remarks(
             "Sends the commands' categories list - otherwise shows help to the provided command or module")]
        public async Task Help()
        {
            var emb = new EmbedBuilder()
                .WithColor(Color.Green)
                .AddField("Moderation Commands", "k!help moderation", false)
                .AddField("Bot Commands", "k!help bot", false)
                .AddField("Fun Commands", "k!help fun", false)
                .AddField("Image Commands", "k!help image", false)
                .AddField("Economy Commands", "k!help economy", false)
                .AddField("Utility Commands", "k!help utility", false)
                .AddField("Other Commands", "k!help others", false);

            await Context.Channel.SendMessageAsync("KillerBot Help:", false, emb.Build());
        }
    }
} */
