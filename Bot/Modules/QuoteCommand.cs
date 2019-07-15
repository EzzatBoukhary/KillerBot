using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Discord.Commands;
using Bot.Features.Lists;
using Bot.Extensions;
using Bot.Helpers;

namespace Bot.Modules
{
    public class QuoteCommand : ModuleBase<MiunieCommandContext>
    {
        private CommandService _service;
        private readonly ListManager _listManager;
        private int _fieldRange = 10;

        public QuoteCommand(CommandService service, ListManager listManager)
        {
            _service = service;
            _listManager = listManager;
        }

        [Command("Quote"), Priority(0)]
        [Summary("Quotes a user from a given message's ID.")]
        [Remarks("Usage: k!quote {messageID}")]
        public async Task QuoteAsync(ulong messageId)
        {
            var m = await Context.Channel.GetMessageAsync(messageId);
            if (m is null)
            {
                var embed = new EmbedBuilder()
                {
                    Color = (Color.DarkerGrey)
                };
                embed.WithDescription("A message with that ID doesn't exist in this channel.");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            var shouldHaveImage = m.Attachments.Count > 0;

            var embed2 = new EmbedBuilder();
            if (Context.Guild.GetUser(m.Author.Id) != null)
            {
                IGuildUser author = Context.Guild.Users.Where((user) => user.Id == m.Author.Id).First();
                IRole highestRole = DiscordHelpers.GetUsersHigherstRole(author);
                if (highestRole != null)
                    embed2.Color = highestRole.Color;
            }
            else
            {

            }

            embed2.WithDescription($"{m.Content}\n\n[Jump!]({m.GetJumpUrl()})")
                .WithAuthor($"{m.Author.Username}#{m.Author.Discriminator}", m.Author.GetAvatarUrl())
                .WithTimestamp(m.Timestamp)
                .WithFooter($"#{m.Channel.Name}", Context.Guild.IconUrl);
            if (shouldHaveImage)
            {
                embed2.WithImageUrl(m.Attachments.ElementAt(0).Url);
            }

            await Context.Channel.SendMessageAsync("", false, embed2.Build());
        }

        [Command("Quote"), Priority(1)]
        [Summary("Quotes a user in a different chanel from a given message's ID.")]
        [Remarks("Usage: |prefix |quote |channel {messageID}  or message ID then channel, same thing")]
        public async Task QuoteAsync(SocketTextChannel channel, ulong messageId)
        {
            var m = await channel.GetMessageAsync(messageId);
            if (m is null)
            {
                var embed = new EmbedBuilder()
                {
                    Color = (Color.DarkerGrey)
                };
                embed.WithDescription("A message with that ID doesn't exist in the given channel.");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            
            var shouldHaveImage = m.Attachments.Count > 0;

            var embed2 = new EmbedBuilder();
            if (Context.Guild.GetUser(m.Author.Id) != null)
            {
                IGuildUser author = Context.Guild.Users.Where((user) => user.Id == m.Author.Id).First();
                IRole highestRole = DiscordHelpers.GetUsersHigherstRole(author);

                if (highestRole != null)
                    embed2.Color = highestRole.Color;
            }
            else
            {

            }
            embed2.WithDescription($"{m.Content}\n\n[Jump!]({m.GetJumpUrl()})")
             .WithAuthor($"{m.Author.Username}#{m.Author.Discriminator}", m.Author.GetAvatarUrl())
                .WithTimestamp(m.Timestamp)
                .WithFooter($"#{m.Channel.Name}", Context.Guild.IconUrl);
            if (shouldHaveImage)
            {
                embed2.WithImageUrl(m.Attachments.ElementAt(0).Url);
            }

            await Context.Channel.SendMessageAsync("", false, embed2.Build());
        }

        [Command("Quote"), Priority(1)]
        [Summary("Quotes a user in a different chanel from a given message's ID.")]
        [Remarks("Usage: |prefix |quote |channel {messageID} or message ID then channel, same thing")]
        public async Task QuoteAsync2(ulong messageId, SocketTextChannel channel)
        {
            var m = await channel.GetMessageAsync(messageId);
            if (m is null)
            {
                var embed = new EmbedBuilder()
                {
                    Color = (Color.DarkerGrey)
                };
                embed.WithDescription("A message with that ID doesn't exist in the given channel.");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            var shouldHaveImage = m.Attachments.Count > 0;
            var gld = Context.Guild as SocketGuild;
            var embed2 = new EmbedBuilder();
            if (Context.Guild.GetUser(m.Author.Id) != null)
            {
                IGuildUser author = Context.Guild.Users.Where((user) => user.Id == m.Author.Id).First();
                IRole highestRole = DiscordHelpers.GetUsersHigherstRole(author);

                if (highestRole != null)
                    embed2.Color = highestRole.Color;
            }
            else
            {

            }
            embed2.WithDescription($"{m.Content}\n\n[Jump!]({m.GetJumpUrl()})")
               .WithAuthor($"{m.Author.Username}#{m.Author.Discriminator}", m.Author.GetAvatarUrl())
                .WithTimestamp(m.Timestamp)
                .WithFooter($"#{m.Channel.Name}", gld.IconUrl);
            if (shouldHaveImage)
            {
                embed2.WithImageUrl(m.Attachments.ElementAt(0).Url);
            }

            await Context.Channel.SendMessageAsync("", false, embed2.Build());
        }
    }
}
