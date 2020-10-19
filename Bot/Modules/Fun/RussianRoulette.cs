using Discord;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using Bot.Handlers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Bot.Helpers;
using Bot.Features.GlobalAccounts;
using System;

namespace Bot.Modules.Fun
{
    public class RussianRoulette : EconomyGame
    {
        private int round, currentTurn;
        public int PlayerSlots;
        private bool isGameGoing;
        public int Bet { get; }

        private SocketUser host;
        public readonly List<SocketUser> AlivePlayers = new List<SocketUser>();
        private int RandomChamber() => DiscordHelpers.GetRandomNumber(0, 6);
        private readonly GlobalUserAccounts _globalUserAccounts;
        private readonly Action<ulong> endGame;

         public RussianRoulette(GlobalUserAccounts globalUserAccounts, Action<ulong> endGame, int slots, int bet)
         {
             _globalUserAccounts = globalUserAccounts;
            this.endGame = endGame;
            PlayerSlots = slots;
            this.Bet = bet;
         }

        public override string Name => ":gun: Russian Roulette";
        

        public bool ContainsPlayer(ulong id)
        {
            return Players.Where(x => x.Id == id).Count() > 0;
        }

        private Embed Embed(string description, string footer, bool showPlayers)
        {
            var embed = new EmbedBuilder()
                .WithTitle($":gun: Russian Roulette")
                .WithColor(DiscordHelpers.ClearColor)
                .WithDescription(description)
                .WithFooter(footer);
            if (showPlayers)
            {
                StringBuilder PlayerDesc = new StringBuilder();
                for (int i = 0; i < Players.Count; i++)
                    PlayerDesc.AppendLine($"Player {i + 1}: {Players.ElementAt(i).Mention}");
                embed.AddField("Players", PlayerDesc);
            }
            return embed.Build();
        }

        private Embed gameEmbed(string description, string footer) => DiscordHelpers.Embed($":gun: Russian Roulette - Round {round}", description, DiscordHelpers.ClearColor, footer, "");

        public async Task TryToStartGame(SocketCommandContext context, string input)
        {
            //if (isGameGoing)
            //{
            //    await context.Channel.SendMessageAsync("", false, Embed($"Sorry, {host.Mention} is currently hosting a game.", "", false));
            //    return;
            //}
            if (input == "")
            {
                await context.Channel.SendMessageAsync("", false, Embed("Please enter an amount of players.\n\n`!rr #` - # = Players\n\nExample: \n`k!rr 5` will start a game with 5 players.", "", false));
                return;
            }
            input = input.Replace("rr ", "");
            int.TryParse(input, out PlayerSlots);
            if (PlayerSlots > 6)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, 6 is the max amount of players for Russian Roulette.", "", false));
                return;
            }
            else if (PlayerSlots < 2)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, 2 is the minimum amount of players for Russian Roulette.", "", false));
                return;
            }
            await StartGame(context).ConfigureAwait(false);
        }

        public async Task StartGame(SocketCommandContext context)
        {
            host = (SocketGuildUser)context.User;
            await context.Channel.SendMessageAsync("", false, Embed($"{host.Mention} has started a game of Russian Roulette with {PlayerSlots} players and a bet of **{Bet} coins**!\n\nType `k!rr join @{host.Username}#{host.Discriminator}` to enter the game!", "", false));
            Players.Add(host);
            AlivePlayers.Add(host);
            _globalUserAccounts.GetById(host.Id).Coins -= Bet;
            isGameGoing = true;
        }

        public async Task TryToJoin(SocketCommandContext context)
        {
            if (Players.Count == PlayerSlots || Players.Contains(context.User)) return;

            SocketUser newPlayer = context.User;
            Players.Add(newPlayer);
            AlivePlayers.Add(newPlayer);
            _globalUserAccounts.GetById(newPlayer.Id).Coins -= Bet;
            if (Players.Count != PlayerSlots)
                await context.Channel.SendMessageAsync("", false, Embed($"{PlayerSlots - Players.Count} more player(s) needed!\n\nType `k!rr join @{Players.ElementAt(0).Username}#{Players.ElementAt(0).Discriminator}` to enter the game! (**Bet: {Bet} coins**)", "", true));
            else
                await context.Channel.SendMessageAsync("", false, gameEmbed($"Initial round.\n\nWaiting for {AlivePlayers.ElementAt(0).Mention} to pull the trigger. (`k!rr pt`)", ""));
        }

        public async Task PullTrigger(SocketCommandContext context)
        {
            if (Players.Count() < PlayerSlots)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"{PlayerSlots - Players.Count} more player(s) needed!\n\nType `k!rr join @{Players.ElementAt(0).Username}#{Players.ElementAt(0).Discriminator}` to enter the game! (**Bet: {Bet} coins**)", "", true));
                return;
            }
            SocketGuildUser player = (SocketGuildUser)context.User;
            if (AlivePlayers.ElementAt(currentTurn) != player) return;
            await DoRound(player, context).ConfigureAwait(false);
        }
        bool gameended = false;
        private async Task DoRound(SocketGuildUser player, SocketCommandContext context)
        {

            round++;
            int badChamber = RandomChamber();
            int currentChamber = RandomChamber();
            if (currentChamber == badChamber)
            {
                var playeracc = _globalUserAccounts.GetById(player.Id);
                for (int i = 0; i < playeracc.Bought_Items.Count; i++)
                {
                    if (playeracc.Bought_Items[i].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                    {
                        if (DateTime.UtcNow - playeracc.Bought_Items[i].Date > playeracc.Bought_Items[i].Duration)
                        {
                            playeracc.Bought_Items.Remove(playeracc.Bought_Items[i]);
                            _globalUserAccounts.SaveAccounts(playeracc.Id);
                        }

                    }
                    else if (playeracc.Bought_Items[i].Duration == new TimeSpan(long.MaxValue))
                    {
                        if (playeracc.Bought_Items[i].name == "Life Saver")
                        {
                            currentTurn = currentTurn == (AlivePlayers.Count - 1) ? currentTurn = 0 : currentTurn + 1;
                            await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} almost died BUT the shot missed and they survive! (thanks to **Life Saver**) \n\nWaiting for {AlivePlayers.ElementAt(currentTurn).Mention} to pull the trigger. (`k!rr pt`)", ""));
                            playeracc.Bought_Items.Remove(playeracc.Bought_Items[i]);
                            _globalUserAccounts.SaveAccounts(playeracc.Id);
                            return;
                        }
                    }
                }
                await DieAndCheckForWin(player, context).ConfigureAwait(false);
                if (gameended == false)
                {
                    //AlivePlayers.Remove(player);
                    for (int i = 0; i < playeracc.Bought_Items.Count; i++)
                    {
                        if (playeracc.Bought_Items[i].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                        {
                            if (DateTime.UtcNow - playeracc.Bought_Items[i].Date > playeracc.Bought_Items[i].Duration)
                            {
                                playeracc.Bought_Items.Remove(playeracc.Bought_Items[i]);
                                _globalUserAccounts.SaveAccounts(playeracc.Id);
                            }

                        }
                        else if (playeracc.Bought_Items[i].name == "No Loss Day")
                        {

                            await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost NOTHING due to **No Loss Day**!\n\nWaiting for {AlivePlayers.ElementAt(currentTurn).Mention} to pull the trigger. (`k!rr pt`)", ""));
                            playeracc.Coins += Bet;
                            _globalUserAccounts.SaveAccounts(playeracc.Id);
                            return;
                        }

                    }
                    currentTurn = currentTurn % AlivePlayers.Count;

                    await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost **{Bet} coins**!\n\nWaiting for {AlivePlayers.ElementAt(currentTurn).Mention} to pull the trigger. (`k!rr pt`)", ""));
                }
            }
            else
            {
                currentTurn = currentTurn == (AlivePlayers.Count - 1) ? currentTurn = 0 : currentTurn + 1;
                await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*click*\n\n{player.Mention} survived!\n\nWaiting for {AlivePlayers.ElementAt(currentTurn).Mention} to pull the trigger. (`k!rr pt`)", ""));
            }
        }

        private async Task DieAndCheckForWin(SocketGuildUser player, SocketCommandContext context)
        {
            AlivePlayers.Remove(player);
            var lost = _globalUserAccounts.GetById(player.Id);
            var winner = _globalUserAccounts.GetById(AlivePlayers.ElementAt(0).Id);
            if(AlivePlayers.Count == 1)
            {
                bool Double = false;
                bool NoLoss = false;

                // If the lost user has specific items bought from the shop
                for (int i = 0; i < lost.Bought_Items.Count; i++)
                {
                    if (lost.Bought_Items[i].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                    {
                        if (DateTime.UtcNow - lost.Bought_Items[i].Date > lost.Bought_Items[i].Duration)
                        {
                            lost.Bought_Items.Remove(lost.Bought_Items[i]);
                            _globalUserAccounts.SaveAccounts(lost.Id);
                        }
                        else if (lost.Bought_Items[i].name == "No Loss Day")
                        {
                            NoLoss = true;
                        }
                    }
                    continue;
                }
                // If the winner user has specific items bought from the shop
                for (int i = 0; i < winner.Bought_Items.Count; i++)
                {
                    if (winner.Bought_Items[i].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                    {
                        if (DateTime.UtcNow - winner.Bought_Items[i].Date > winner.Bought_Items[i].Duration)
                        {
                            winner.Bought_Items.Remove(winner.Bought_Items[i]);
                            _globalUserAccounts.SaveAccounts(winner.Id);
                        }
                        else if (winner.Bought_Items[i].name == "Double Day")
                        {
                            Double = true;
                        }
                    }
                    continue;
                }
                //Check what items the players have
                if (NoLoss == true && Double == false)
                {
                    lost.Coins += Bet;
                    await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost NOTHING due to **No Loss Day**!\n\n{AlivePlayers.ElementAt(0).Mention} won the game and got **{Bet} coins**!", ""));
                }
                else if (NoLoss == true && Double == true)
                {
                    lost.Coins += Bet;
                    winner.Coins += Bet * 3;
                    await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost NOTHING due to **No Loss Day**!\n\n{AlivePlayers.ElementAt(0).Mention} won the game and got **{Bet * 2} coins** due to **Double Day**!", ""));
                }
                else if (NoLoss == false && Double == true)
                {
                    winner.Coins += Bet * 3;
                    await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost **{Bet} Coins**!\n\n{AlivePlayers.ElementAt(0).Mention} won the game and got **{Bet * 2} coins** due to **Double Day**!", ""));
                }
                else
                {
                    winner.Coins += Bet * 2;
                    await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost {Bet} Coins!\n\n{AlivePlayers.ElementAt(0).Mention} won the game and got {Bet} coins!", ""));
                }
                _globalUserAccounts.SaveAccounts();
                endGame(host.Id);
                gameended = true;
            }
        }
    }
}
