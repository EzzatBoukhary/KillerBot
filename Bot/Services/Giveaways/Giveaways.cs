using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Bot.Services
{
    public class Giveaways
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public string Question { get; set; }
        public List<GiveawayAnswer> Answers { get; set; }
        public ConcurrentDictionary<GiveawayAnswer, List<IUser>> ReactionUsers { get; }

        public Giveaways()
        {
            Answers = new List<GiveawayAnswer>();
            ReactionUsers = new ConcurrentDictionary<GiveawayAnswer, List<IUser>>();
        }
    }

    public class GiveawayAnswer : IEquatable<GiveawayAnswer>
    {
        public string Answer { get; set; }
        public Emoji AnswerEmoji { get; set; }

        public GiveawayAnswer(string answer, Emoji answerEmoji)
        {
            Answer = answer;
            AnswerEmoji = answerEmoji;
        }

        public bool Equals(GiveawayAnswer other) =>
            other != null &&
            Answer == other.Answer &&
            AnswerEmoji.Equals(other.AnswerEmoji);

        public override bool Equals(object other) => other is GiveawayAnswer ans && Equals(ans);

        public override int GetHashCode() => (Answer, AnswerEmoji).GetHashCode();
    }
}