using System.Threading.Tasks;
/*using Bot.Extensions;
using Bot.Features.Trivia;
using Bot.Preconditions;
using Discord.Commands;

namespace Bot.Modules.Fun
{
    public class Trivia : ModuleBase<MiunieCommandContext>
    {
        private readonly TriviaGames _triviaGames;

        public Trivia(TriviaGames triviaGames)
        {
            _triviaGames = triviaGames;
        }

        [Command("Trivia", RunMode = RunMode.Async)]
        [Cooldown(5)]
        [Remarks("Requires Manage Messages permission in order to remove your reactions, and only the one did the command can play.")]
        [Summary("A fun game where the bot asks you questions and you answer. Giving you options to choose the category, question type and difficulty for the game, and then showing how you did after you stop.")]
        public async Task NewTrivia()
        {
            var msg = await Context.Channel.SendMessageAsync("", false, _triviaGames.TrivaStartingEmbed().Build());
            await _triviaGames.NewTrivia(msg, Context.User);
        }
        
    }
} */
