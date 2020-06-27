using Discord;
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
using System.Globalization;
using Bot.Preconditions;

namespace Bot.Modules
{
    /// <summary>
    /// Provides a simple voting system
    /// </summary>
    [Name("PollModule")]
    [RequireContext(ContextType.Guild)]
    public class PollModule : InteractiveBase
    {

        //private static readonly TimeSpan pollDuration = TimeSpan.FromMinutes(P);

        public PollModule()
        {
        }

        [Command("Poll")]
        [Alias("Vote", "start-poll")]
        [Priority(1)]
        [Cooldown(10)]
        [Remarks("Starts a new poll with the specified question and automatically adds reactions")]
        [Example("k!poll \"Was the event good?\" for 1h")]
        [RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task StartPollAsync([Summary("The question, \"for\", the duration")][Remainder] string args)
        {
            string[] splittedArgs = null;
            if (args.Contains(" for ")) splittedArgs = args.Split(new string[] { " for " }, StringSplitOptions.None);
            if (splittedArgs == null || splittedArgs.Length < 2)
            {
                await ReplyAsync("In order to start a poll you'll need to set the question and the duration you'd want this poll to run for. \nExample: k!poll Stream? for 1d \nMake sure to put **for** between the question and the duration.");
                return;
            }

            var timeString = splittedArgs[splittedArgs.Length - 1];
            if (timeString == "24h")
                timeString = "1d";

            splittedArgs[splittedArgs.Length - 1] = "";
            var question = string.Join(" for ", splittedArgs, 0, splittedArgs.Length - 1);

            var timeDateTime = TimeSpan.ParseExact(timeString, ReminderFormat.Formats, CultureInfo.CurrentCulture);
            question = question.Trim('\"');
            if (question.IsEmpty())
            {
                _ = await ReplyAsync("Please enter a question").ConfigureAwait(false);
                return;
            }
            var poll = new Poll
            {
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                Question = question,
                PollDuration = timeDateTime,
                Answers = new List<PollAnswer>
                {
                    new PollAnswer("Yes", new Emoji("👍")),
                    new PollAnswer("No", new Emoji("👎")),
                    new PollAnswer("Don't care", new Emoji("🤷"))
                }
            };
            _ = await InlineReactionReplyAsync(GeneratePoll(poll), false).ConfigureAwait(false);
        }
        /* [Command("Poll")]
        [Alias("Vote")]
        [Priority(1)]
        [Remarks("Starts a new poll with the specified question and automatically adds reactions")]
        [Example("k!poll \"Yeetos Mentos!\"")]
        [RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.ManageMessages)]
        public async Task StartPollAsync([Summary("The question")] string question)
        {
            var timeDateTime = TimeSpan.FromHours(1);
            question = question.Trim('\"');
            if (question.IsEmpty())
            {
                _ = await ReplyAsync("Please enter a question").ConfigureAwait(false);
                return;
            }
            var poll = new Poll
            {
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                Question = question,
                PollDuration = timeDateTime,
                Answers = new List<PollAnswer>
                {
                    new PollAnswer("Yes", new Emoji("👍")),
                    new PollAnswer("No", new Emoji("👎")),
                    new PollAnswer("Don't care", new Emoji("🤷"))
                }
            };
            _ = await InlineReactionReplyAsync(GeneratePoll(poll), false).ConfigureAwait(false);
        } */
          [Command("Poll")]
          [Alias("Vote")]
          [Priority(2)]
          [Cooldown(10)]
          [Remarks("Starts a new poll with the specified question and the list answers and automatically adds reactions")]
          [Example("k!poll 60 \"What's your favourite game?\" \"Minecraft\" \"COD\" \"CS:GO\"")]
          [RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.ManageMessages)]
          [RequireUserPermission(GuildPermission.ManageMessages)]
          public async Task StartPollAsync([Summary("Duration of the poll (in minutes)")] int duration, [Summary("The question of the poll")] string question, [Summary("The list of answers")] params string[] answers)
          {
              if (answers == null || answers.Length <= 0)
              {
                  await StartPollAsync(question).ConfigureAwait(false);
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
            TimeSpan time = new TimeSpan(0,duration,0);
              var poll = new Poll
              {
                  GuildId = Context.Guild.Id,
                  ChannelId = Context.Channel.Id,
                  Question = question,
                  PollDuration = time,
                  Answers = answers.Select((ans, i) => new PollAnswer(ans, new Emoji(DiscordHelpers.GetUnicodeRegionalLetter(i)))).ToList()
              };
              var a = await InlineReactionReplyAsync(GeneratePoll(poll), false).ConfigureAwait(false);
          } 
        private static ReactionCallbackData GeneratePoll(Poll poll)
        {
            string answers = string.Join(Environment.NewLine, poll.Answers.Select(x => $"{x.AnswerEmoji} {x.Answer}"));

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"New Poll: {poll.Question}")
                .WithColor(new Color(20, 20, 20))
                .WithDescription(
                    "- Pick an option by clicking on the corresponding Emoji" + Environment.NewLine
                    + "- Only your first pick counts!" + Environment.NewLine
                    + $"- You have {poll.PollDuration} to cast your vote"
                    )
                .AddField("Pick one", answers);
            var rcbd = new ReactionCallbackData("", embedBuilder.Build(), false, true, poll.PollDuration, async c => await PollEndedAsync(c, poll).ConfigureAwait(false));
            foreach (Emoji answerEmoji in poll.Answers.Select(x => x.AnswerEmoji))
            {
                _ = rcbd.WithCallback(answerEmoji, (c, r) => AddVoteCount(r, poll));
            }
            return rcbd;
        }

        private static Task AddVoteCount(SocketReaction reaction, Poll poll)
        {
            PollAnswer answer = poll.Answers.SingleOrDefault(e => e.AnswerEmoji.Equals(reaction.Emote));
            if (answer != null && reaction.User.IsSpecified)
            {
                _ = poll.ReactionUsers.AddOrUpdate(
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

        private static async Task PollEndedAsync(SocketCommandContext context, Poll poll)
        {
            if (poll == null)
            {
                return;
            }
            IEnumerable<string> answerCounts = poll.Answers.Select(answer => $"{answer.Answer}: { poll.ReactionUsers.FirstOrDefault(x => x.Key.Equals(answer)).Value?.Count.ToString() ?? "0"}");
            List<IUser> participants = poll.ReactionUsers.Select(x => x.Value).SelectMany(x => x).ToList();
            string participantsString = "-";
            if (participants != null && participants.Count > 0)
            {
                participantsString = string.Join(", ", participants?.Select(x => x.Mention));
            }
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Poll ended: {poll.Question}")
                .WithColor(new Color(20, 20, 20))
                .AddField("Results", string.Join(Environment.NewLine, answerCounts))
                .AddField("Voters", participantsString);

            _ = await context.Channel.SendMessageAsync("", embed: embedBuilder.Build()).ConfigureAwait(false);
        }
        
    }
}