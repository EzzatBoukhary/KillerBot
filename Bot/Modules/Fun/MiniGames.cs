using Bot.Helpers;
using Bot.Modules.Helpers;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
    //public class LocalInteractiveBase : InteractiveBase<SocketCommandContext>
    //{
    //	public async Task<IUserMessage> ReplyMentionAsync(string message)
    //	{
    //		return await ReplyAsync($"{Context.User.Mention} - {message}");
    //	}

    //	public async Task<IUserMessage> ReplyMentionBlockAsync(string message)
    //	{
    //		return await ReplyAsync($"{Context.User.Mention}\n{message}");
    //	}

    //	public async Task EmojiOption(IUserMessage message, EmojiResponse response, TimeSpan timespan, params Emoji[] emojis)
    //	{
    //		foreach (Emoji emoji in emojis)
    //		{
    //			await message.AddReactionAsync(emoji);
    //		}

    //		await Task.Run(async () =>
    //		{
    //			Interactive.AddReactionCallback(message, response);
    //			await Task.Delay(timespan);
    //			Interactive.RemoveReactionCallback(message);
    //		});
    //	}

    //	public Task DeleteCommand()
    //	{
    //		return Context.Message.DeleteAsync();
    //	}

    //	public static string ShowCommands(string prefix, List<string> commands, List<string> descriptions)
    //	{
    //		string message = "";

    //		for (int i = 0; i < descriptions.Count; i++)
    //		{
    //			message += $"**{descriptions[i]}**\n";

    //			commands[i] = commands[i].Replace("[", "*[");
    //			commands[i] = commands[i].Replace("]", "]*");

    //			message += $"{prefix}{commands[i]}\n\n";
    //		}

    //		commands.Clear();
    //		descriptions.Clear();

    //		return message;
    //	}
    //}

    public class MiniGames : InteractiveBase<SocketCommandContext>
    {
        string GetString(string[] emojis, int[] progresses)
        {
            string response = "";

            for (int i = 0; i < emojis.Length; i++)
            {
                response += new string('ㅤ', progresses[i]) +
                    emojis[i] +
                    new string('ㅤ', 50 - progresses[i]) +
                    "<:RaceFinsh:643263794621186058>\n";
            }

            return response;
        }

        IEnumerable<int> GetBitIndices(int number)
        {
            for (int i = 0; i < 32; i++)
            {
                if ((number & (1 << i)) != 0)
                {
                    yield return i;
                }
            }
        }

        Dictionary<int, string> minesweeperValues = new Dictionary<int, string>()
    {
        {  -1 , ":bomb:" },
        {  0 , ":white_small_square:" },
        {  1 , ":one:" },
        {  2 , ":two:" },
        {  3 , ":three:" },
        {  4 , ":four:" },
        {  5 , ":five:" },
        {  6 , ":six:" },
        {  7 , ":seven:" },
        {  8 , ":eight:" },
    };
        private static readonly Discord.Color EMBED_COLOR = Color.DarkOrange;
        [Command("minesweeper")]
        [Summary("Minesweeper minigame")]
        [Remarks("You can customize the width, height, and bomb count by putting the corresponding numbers in the parameters. (k!minesweeper <width> <height> <bomb count)>")]
        [Example("k!minesweeper 8 8 20")]
        public async Task Minesweeper([Summary("width")]int width = 10, [Summary("height")]int height = 10, [Summary("bomb count")]int bombs = 35)
        {
            if (width < 1 || height < 1 || bombs < 0)
            {
                await Context.Channel.SendMessageAsync("Invalid grid size or bomb count");
            }
            else if (width > 10 || height > 10)
            {
                await Context.Channel.SendMessageAsync("Max Grid Size: 10 x 10");
            }
            else if (bombs >= height * width)
            {
                await Context.Channel.SendMessageAsync("Too many bombs!");
            }
            else
            {
                MinesweeperBoard game = new MinesweeperBoard(height, width, bombs);
                EmbedBuilder builder = new EmbedBuilder();
                builder.Title = ":bomb: Minesweeper";
                builder.Color = EMBED_COLOR;
                builder.Description = game.ToString();
                await SendEmbed(builder.Build());
            }
        }

        [Command("race", RunMode = RunMode.Async), Alias("rally")]
        [Summary("Create a virtual race/rally between two or more things/people.")]
        [Example("k!race Noob1 Noob2 turtle")]
        public async Task Race([Summary("The competitors. (Should be at least two and you need to put a space between each one.)")] [Remainder]string args)
        {
            string[] parts = args.Split(' ');

            if (parts.Length < 2)
            {
                await ReplyAsync("There must be at least two competitors! Add more competitors. You can also use emojis :wink:");
                return;
            }
            if (parts.Length > 5)
            {
                await ReplyAsync("The maximum amount of competitors is 5, and the minimum one is 2.");
                return;
            }

            Random random = new Random();

            string[] emojis = new string[Math.Min(5, parts.Length)];
            int[] progresses = new int[emojis.Length];
            int[] strengths = new int[emojis.Length];

            for (int i = 0; i < emojis.Length; i++)
            {
                emojis[i] = parts[i];
                strengths[i] = random.Next(5, 8);
            }

            IUserMessage message = await ReplyAsync("Ready...");

            await Task.Delay(500);

            await message.ModifyAsync(properties =>
            {
                properties.Content = "Set...";
            });

            await Task.Delay(500);

            await message.ModifyAsync(properties =>
            {
                properties.Content = "Go!";
            });

            await Task.Delay(500);

            int winner = 0;
            int winnerCount = 0;

            bool isFirst = true;

            while (winner == 0)
            {
                await message.ModifyAsync(properties =>
                {
                    if (!isFirst)
                    {
                        for (int i = 0; i < progresses.Length; i++)
                        {
                            progresses[i] += random.Next(1, strengths[i]);

                            if (progresses[i] >= 50)
                            {
                                progresses[i] = 50;
                                winner |= 1 << i;

                                winnerCount++;
                            }
                        }
                    }

                    isFirst = false;

                    properties.Content = GetString(emojis, progresses);
                });

                await Task.Delay(1000);
            }

            await message.ModifyAsync(properties =>
            {
                if (winnerCount == 1)
                {
                    properties.Content = ":first_place: The winner is **" + emojis[GetBitIndices(winner).First()] + "** !";
                }
                else
                {
                    string winners = "";

                    foreach (int index in GetBitIndices(winner))
                    {
                        if (winners.Length != 0)
                        {
                            winners += " and ";
                        }

                        winners += emojis[index];
                    }

                    properties.Content = "It's a tie between " + winners;
                }
            });
        }
        [Command("roll")]
        [Summary("Roll some dice")]
        [Example("k!roll 1d20")]
        public async Task RollDice([Summary("dice")] string diceStr = "")
        {
            try
            {
                Dice dice = Dice.FromString(diceStr);
                await Context.Channel.SendMessageAsync(string.Join(" ", dice.GenerateRolls()));
            }
            catch (ArgumentException e)
            {
                // This exception occurs when parsing the dice string,
                // and is meant to be displayed to the user
                // there is no need to log it
                await Context.Channel.SendMessageAsync(e.Message);
            }
        }

        [Command("catfact")]
        [Summary("Responds with a random cat fact")]
        public async Task RequestCatFact()
        {
            string fact = await ApiFetcher.RequestStringFromApi("https://catfact.ninja/fact", "fact");
            if (!string.IsNullOrEmpty(fact))
            {
                await Context.Channel.SendMessageAsync(fact);
            }
            else
            {
                await Context.Channel.SendMessageAsync("The catfact command is currently unavailable.");
            }
        }

        [Command("foxfact")]
        [Summary("Responds with a random fox fact")]
        public async Task RequestFoxFact()
        {
            string fact = await ApiFetcher.RequestStringFromApi("https://some-random-api.ml/facts/fox", "fact");
            if (!string.IsNullOrEmpty(fact))
            {
                await Context.Channel.SendMessageAsync(fact);
            }
            else
            {
                await Context.Channel.SendMessageAsync("The foxfact command is currently unavailable.");
            }
        }

        [Command("cat")]
        [Alias("meow", "randomcat")]
        [Summary("Responds with a random cat")]
        public async Task RequestCat()
        {
            string fileUrl = await ApiFetcher.RequestEmbeddableUrlFromApi("https://aws.random.cat/meow", "file");
            if (!string.IsNullOrEmpty(fileUrl))
            {
                await SendAnimalEmbed(":cat:", fileUrl);
            }
            else
            {
                await Context.Channel.SendMessageAsync("The cat command is currently unavailable.");
            }
        }

        [Command("dog")]
        [Alias("randomdog")]
        [Summary("Responds with a random dog")]
        public async Task RequestDog()
        {
            string fileUrl = await ApiFetcher.RequestEmbeddableUrlFromApi("https://random.dog/woof.json", "url");
            if (!string.IsNullOrEmpty(fileUrl))
            {
                await SendAnimalEmbed(":dog:", fileUrl);
            }
            else
            {
                await Context.Channel.SendMessageAsync("The dog command is currently unavailable.");
            }
        }

        [Command("fox")]
        [Alias("randomfox")]
        [Summary("Responds with a random fox")]
        public async Task RequestFox()
        {
            string fileUrl = await ApiFetcher.RequestEmbeddableUrlFromApi("https://wohlsoft.ru/images/foxybot/randomfox.php", "file");
            if (!string.IsNullOrEmpty(fileUrl))
            {
                await SendAnimalEmbed(":fox:", fileUrl);
            }
            else
            {
                await Context.Channel.SendMessageAsync("The fox command is currently unavailable.");
            }
        }

        [Command("birb")]
        [Alias("randombirb", "randombird", "bird")]
        [Summary("Responds with a random birb")]
        public async Task RequestBirb()
        {
            string fileUrl = await ApiFetcher.RequestEmbeddableUrlFromApi("https://random.birb.pw/tweet.json", "file");
            if (!string.IsNullOrEmpty(fileUrl))
            {
                fileUrl = "https://random.birb.pw/img/" + fileUrl;
                await SendAnimalEmbed(":bird:", fileUrl);
            }
            else
            {
                await Context.Channel.SendMessageAsync("The birb command is currently unavailable.");
            }
        }
        private async Task SendAnimalEmbed(string title, string fileUrl)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(EMBED_COLOR)
                .WithImageUrl(fileUrl);
            await SendEmbed(builder.Build());
        }

        private async Task SendEmbed(Embed embed)
        {
            await Context.Channel.SendMessageAsync("", false, embed);
        }
    }
}