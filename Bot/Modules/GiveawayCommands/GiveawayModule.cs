/*using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Bot.Helpers;
using Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Modules
{
    /// <summary>
    /// Provides a simple voting system
    /// </summary>
    [Name("Giveaway")]
    [RequireContext(ContextType.Guild)]
    public class GiveawayModule : InteractiveBase
    {

        private static readonly TimeSpan pollDuration = TimeSpan.FromMinutes(1);

        public GiveawayModule()
        {
        }

        [Command("giveaway")]
        [Alias("start-giveaway")]
        [Priority(1)]
        [Remarks("Starts a new giveaway with the specified reward and automatically adds reactions")]
        [Example("k!giveaway \"An epic reward\"")]
        [RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.ManageMessages)]
        public async Task StartGiveawayAsync([Summary("The reward")][Remainder] string reward)
        {
            reward = reward.Trim('\"');
            if (reward.IsEmpty())
            {
                _ = await ReplyAsync("Please enter a reward").ConfigureAwait(false);
                return;
            }
            var giveaway = new Giveaways
            {
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                Question = reward,
                Answers = new List<GiveawayAnswer>
                {
                    new GiveawayAnswer("Join the giveaway by reacting with \"🎉\"", new Emoji("🎉")),
                }
            };
            _ = await InlineReactionReplyAsync(GenerateGiveaway(giveaway), false).ConfigureAwait(false);
        }

        [Command("giveaway")]
        [Alias("Vote")]
        [Priority(2)]
        [Remarks("Starts a new poll with the specified question and the list answers and automatically adds reactions")]
        [Example("k!poll \"Cool new feature?\" \"supercool\" \"over 9000\" \"bruh...\"")]
        [RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.ManageMessages)]
        public async Task StartGiveawayAsync([Summary("The question")] string question, [Summary("The list of answers")] params string[] answers)
        {
            if (answers == null || answers.Length <= 0)
            {
                await StartGiveawayAsync(question).ConfigureAwait(false);
                return;
            }
            if (answers.Length < 2)
            {
                _ = await ReplyAsync("Please provide at least 2 answers").ConfigureAwait(false);
                return;
            }
            if (answers.Length > 7)
            {
                _ = await ReplyAsync("Please provide a maximum of 7 answers").ConfigureAwait(false);
                return;
            }
            question = question.Trim('\"');
            if (question.IsEmptyOrWhiteSpace())
            {
                _ = await ReplyAsync("Please enter a question").ConfigureAwait(false);
                return;
            }

            var giveaway = new Giveaways
            {
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                Question = question,
                Answers = answers.Select((ans, i) => new GiveawayAnswer(ans, new Emoji(DiscordHelpers.GetUnicodeRegionalLetter(i)))).ToList()
            };
            _ = await InlineReactionReplyAsync(GenerateGiveaway(giveaway), false).ConfigureAwait(false);
        }

        private static ReactionCallbackData GenerateGiveaway(Giveaways giveaway)
        {
            string answers = string.Join(Environment.NewLine, giveaway.Answers.Select(x => $"{x.AnswerEmoji} {x.Answer}"));

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"🎉 Giveaway 🎉")
                .WithColor(Color.Blue)
                .WithDescription(
                    $"- {giveaway.Question}" + Environment.NewLine
                    + $"- You have {pollDuration.Humanize()} to join the giveaway"
                    )
                .AddField("Join", answers);

            var rcbd = new ReactionCallbackData("", embedBuilder.Build(), false, true, pollDuration, async c => await PollEndedAsync(c, giveaway).ConfigureAwait(false));
            foreach (Emoji answerEmoji in giveaway.Answers.Select(x => x.AnswerEmoji))
            {
                _ = rcbd.WithCallback(answerEmoji, (c, r) => AddEntryCount(r, giveaway));
            }
            return rcbd;
        }

        private static Task AddEntryCount(SocketReaction reaction, Giveaways giveaway)
        {
            GiveawayAnswer answer = giveaway.Answers.SingleOrDefault(e => e.AnswerEmoji.Equals(reaction.Emote));
            if (answer != null && reaction.User.IsSpecified)
            {
                _ = giveaway.ReactionUsers.AddOrUpdate(
                        answer,
                        new List<IUser> { reaction.User.Value },
                        (_, list) =>
                          {
                              list.Add(reaction.User.Value);
                              return list;
                          }
                );
            }
            return Task.CompletedTask;
        }

        private static async Task PollEndedAsync(SocketCommandContext context, Giveaways giveaway)
        {
            if (giveaway == null)
            {
                return;
            }
            IEnumerable<string> answerCounts = giveaway.Answers.Select(answer => $"{answer.Answer}: { giveaway.ReactionUsers.FirstOrDefault(x => x.Key.Equals(answer)).Value?.Count.ToString() ?? "0"}");
            List<IUser> participants = giveaway.ReactionUsers.Select(x => x.Value).SelectMany(x => x).ToList();
            string participantsString = "-";
            if (participants != null && participants.Count > 0)
            {
                participantsString = string.Join(", ", participants?.Select(x => x.Mention));
            }
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Poll ended: {giveaway.Question}")
                .WithColor(new Color(20, 20, 20))
                .AddField("Results", string.Join(Environment.NewLine, answerCounts))
                .AddField("Voters", participantsString);

            _ = await context.Channel.SendMessageAsync("", embed: embedBuilder.Build()).ConfigureAwait(false);
        }
        
    }
} */