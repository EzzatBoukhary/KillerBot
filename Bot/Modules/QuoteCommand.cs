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

        public QuoteCommand(CommandService service, ListManager listManager)
        {
            _service = service;
            _listManager = listManager;
        }

        [Command("Quote"), Priority(0)]
        [Summary("Quotes a user's message from a given message's ID with an optional reply. (the message doesn't have to be in the channel the command is done in)")]
        [Remarks("Usage: k!quote {messageID} (reply)")]
        [RequireContext(ContextType.Guild)]
        public async Task QuoteAsync([Summary("Message ID")]ulong messageId = 0, [Summary("OPTIONAL: A reply to the quoted message")][Remainder] string reply = null)
        {
            if (messageId == 0)
            {
                await ReplyAsync("<:KBfail:580129304592252995> Please provide a message ID to quote.");
                return;
            }
            var m = await Context.Channel.GetMessageAsync(messageId);
            if (m is null)
                m = await FindMessageInUnknownChannel(messageId);
            if (m is null)
            {
                var embed = new EmbedBuilder()
                {
                    Color = (Color.DarkerGrey)
                };
                embed.WithDescription("A message with that ID doesn't exist in this server.");
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

            embed2.WithDescription($"{m.Content}")
                .AddField("Jump URL:", $"[Jump!]({m.GetJumpUrl()})")
                .WithAuthor($"{m.Author.Username}#{m.Author.Discriminator}", m.Author.GetAvatarUrl())
                .WithTimestamp(m.Timestamp)
                .WithFooter($"#{m.Channel.Name}", Context.Guild.IconUrl);
            if (shouldHaveImage)
            {
                embed2.WithImageUrl(m.Attachments.ElementAt(0).Url);
            }
            if (reply != null)
                embed2.AddField($"{Context.User.Username}'s Reply:", reply);
            await Context.Channel.SendMessageAsync("", false, embed2.Build());
        }

        [Command("Quote"), Priority(1)]
        [Summary("Quotes a user's message with an optional reply from a specific channel (supports cross server)")]
        [Remarks("Usage: k!quote {messageID} {channelID} (reply)")]
        public async Task QuoteAsync2([Summary("Message ID")]ulong messageId, [Summary("Channel ID")]ulong channel, [Summary("OPTIONAL: A reply to the quoted message")][Remainder] string reply = null)
        {
            var chn = Context.Client.GetChannel(channel) as SocketTextChannel;
            if (chn == null)
            {
                var embed = new EmbedBuilder()
                {
                    Color = (Color.DarkerGrey)
                };
                embed.WithDescription("Invalid channel. Are you sure you provided an ID of a text channel I can see?");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            var m = await chn.GetMessageAsync(messageId);
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
            embed2.WithDescription($"{m.Content}")
                .AddField("Jump URL:", $"[Jump!]({m.GetJumpUrl()})")
               .WithAuthor($"{m.Author.Username}#{m.Author.Discriminator}", m.Author.GetAvatarUrl())
                .WithTimestamp(m.Timestamp)
                .WithFooter($"#{m.Channel.Name}", chn.Guild.IconUrl);
            if (shouldHaveImage)
            {
                embed2.WithImageUrl(m.Attachments.ElementAt(0).Url);
            }
            if (reply != null)
                embed2.AddField($"{Context.User.Username}'s Reply:", reply);
            await Context.Channel.SendMessageAsync("", false, embed2.Build());
        }
        private async Task<IMessage> FindMessageInUnknownChannel(ulong messageId)
        {
            IMessage message = null;
            var guild = Context.Guild as SocketGuild;
            // We haven't found a message, now fetch all text
            // channels and attempt to find the message

            var channels = guild.TextChannels.ToList();

            foreach (var channel in channels)
            {
                try
                {
                    message = await channel.GetMessageAsync(messageId);

                    if (message != null)
                        break;
                }
                catch (Exception e)
                {
                    await ReplyAsync($"Failed accessing channel {channel.Name} when searching for message **{messageId}**");
                }
            }

            return message;
        }
    }
}
