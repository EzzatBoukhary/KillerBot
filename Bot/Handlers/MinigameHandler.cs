using System;
using System.IO;
using Newtonsoft.Json;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using Bot.Modules.Fun;
using System.Threading.Tasks;
using System.Collections.Generic;
using Bot.Features.GlobalAccounts;
using System.Linq;
using Discord;
using Bot.Helpers;
using System.Text;

namespace Bot.Handlers
{
    public class MinigameHandler
    {
        private static GlobalUserAccounts _globalUserAccounts;
        private Dictionary<ulong, RussianRoulette> russianRoulettes;

        public enum Games
        {
            RussianRoulette,
        }


        public MinigameHandler(GlobalUserAccounts globalUserAccounts)
        {
            _globalUserAccounts = globalUserAccounts;
            russianRoulettes = new Dictionary<ulong, RussianRoulette>();
        }

        public bool CheckIfHosting(ulong uid, Games game)
        {
            if (game == Games.RussianRoulette)
                return russianRoulettes.ContainsKey(uid);
            throw new Exception("Not implemented");
        }


        public async Task CreateGame(SocketCommandContext context, Games game, int players, int bet)
        {
            if (game == Games.RussianRoulette)
            {
                if (players < 2 || players > 6)
                {
                    string msg = "Sorry, 2-6 players are required for Russian Roulette.";
                    EmbedBuilder Embed = new EmbedBuilder();
                    Embed.WithTitle(":gun: Russian Roulette");
                    Embed.WithColor(Color.Red);
                    Embed.WithDescription(msg);
                    await context.Channel.SendMessageAsync("", false, Embed.Build());
                    return;
                }

                SocketUser user = context.User;
                ulong id = user.Id;
                // if user has not started game
                if (CheckIfHosting(id, game) == true)
                {
                    // user is already hosting a game
                    await context.Channel.SendMessageAsync("", false, Embed($"Sorry, you ({user.Mention}) are currently hosting a game.", "", false, russianRoulettes[id]));
                    return;
                }

                // games the user is already in (could be empty)
                IEnumerable<RussianRoulette> games = russianRoulettes.Values.ToList().Where(g => g.ContainsPlayer(id));
                if (games.Count() > 0)
                {
                    // user is in another game
                    await context.Channel.SendMessageAsync("", false, Embed($"Sorry, you ({user.Mention}) are currently in a game.", "", false, games.First()));
                    return;
                }
                // Check if user has enough money to host game
                if (!HasEnoughMoney(user, bet))
                {
                    throw new ArgumentException($"Sorry, you do not have enough coins in your wallet to put that bet");
                }
                else if (bet < 100 || bet > 100000)
                {
                    throw new ArgumentException("Sorry, but the bet amount should be 100+ coins and less than or equal to 1,000,000 coins");
                }
                russianRoulettes[id] = new RussianRoulette(_globalUserAccounts, GetDeleteGame(Games.RussianRoulette), players, bet);
                await russianRoulettes[id].StartGame(context);
                
            }
        }

        public bool HasEnoughMoney(SocketUser user, int amount)
        {
            return _globalUserAccounts.GetById(user.Id).Coins >= amount;
        }

        public async Task JoinGame(SocketCommandContext context, Games game, ulong hostId)
        {
            ulong uid = context.User.Id;
            if (game == Games.RussianRoulette)
            {
                // check if player is already hosting a game
                if (CheckIfHosting(uid, Games.RussianRoulette) == true)
                {
                    EmbedBuilder Embed = new EmbedBuilder();
                    Embed.WithTitle(":gun: Russian Roulette");
                    Embed.WithColor(Color.Red);
                    Embed.WithDescription("Sorry, you are already hosting a game.");
                    await context.Channel.SendMessageAsync("", false, Embed.Build());
                    return;
                }

                // if game does not exist
                if (!russianRoulettes.ContainsKey(hostId))
                {
                    EmbedBuilder Embed = new EmbedBuilder();
                    Embed.WithTitle(":gun: Russian Roulette");
                    Embed.WithColor(Color.Red);
                    Embed.WithDescription("Sorry, could not find a game for this user.");
                    await context.Channel.SendMessageAsync("", false, Embed.Build());
                    return;

                }
                // games the user is already in (could be empty)
                IEnumerable<RussianRoulette> games = russianRoulettes.Values.ToList().Where(g => g.ContainsPlayer(uid));
                if (games.Count() > 0)
                {
                    // user is in another game
                    await context.Channel.SendMessageAsync("", false, Embed($"Sorry, you are currently in a game.", "", false, games.First()));
                    return;
                }

                // Check if user has enough money to join game
                if (!HasEnoughMoney(context.User, russianRoulettes[hostId].Bet))
                {
                    throw new ArgumentException($"Sorry, you do not have enough coins in your wallet to join this russian roulette game **(Bet: {russianRoulettes[hostId].Bet} coins)**");
                }

                await russianRoulettes[hostId].TryToJoin(context);
            }
        }
        public async Task LeaveGame(SocketCommandContext context, Games game, ulong hostId)
        {
            ulong uid = context.User.Id;
            if (game == Games.RussianRoulette)
            {
                // check if player is already hosting a game
                if (CheckIfHosting(uid, Games.RussianRoulette) == true)
                {
                    EmbedBuilder Embed = new EmbedBuilder();
                    Embed.WithTitle(":gun: Russian Roulette");
                    Embed.WithColor(Color.Red);
                    Embed.WithDescription("Sorry, but you are the hoster of the game. However, if you want to delete the game consider doing `k!rr delete` instead.");
                    await context.Channel.SendMessageAsync("", false, Embed.Build());
                    return;
                }

                // games the user is already in (could be empty)
                IEnumerable<RussianRoulette> games = russianRoulettes.Values.ToList().Where(g => g.ContainsPlayer(uid));
                if (games.Count() == 0)
                {
                    // user is in another game
                    await context.Channel.SendMessageAsync("", false, Embed($"Sorry, but you aren't in a russian-roulette game at the moment.", "", false, games.First()));
                    return;
                }
                RussianRoulette rGame;
                rGame = russianRoulettes[hostId];
                if (rGame.PlayerCount < rGame.PlayerSlots)
                {
                    russianRoulettes[hostId].Players.Remove(context.User);
                    _globalUserAccounts.GetById(uid).Coins += rGame.Bet;
                    _globalUserAccounts.SaveAccounts(uid);
                    await context.Channel.SendMessageAsync("", false, Embed($"You were removed from that game.", "", false, games.First()));
                }
                else
                {
                    russianRoulettes[hostId].AlivePlayers.Remove(context.User);
                    _globalUserAccounts.GetById(uid).Coins += rGame.Bet;
                    _globalUserAccounts.SaveAccounts(uid);
                    await context.Channel.SendMessageAsync("", false, Embed($"You were removed from the alive players of the game, but you will have to wait for it to end in order to join another one.", "", false, games.First()));
                }

            }
        }

            private Embed Embed(string description, string footer, bool showPlayers, EconomyGame game)
        {
            var embed = new EmbedBuilder()
                .WithTitle(game.Name)
                .WithColor(DiscordHelpers.ClearColor)
                .WithDescription(description)
                .WithFooter(footer);
            if (showPlayers)
            {
                StringBuilder PlayerDesc = new StringBuilder();

                int i = 0;
                foreach (SocketUser player in game.Players)
                {
                    PlayerDesc.AppendLine($"Player {i++ + 1}: {player.Mention}");
                }
                return embed.AddField("Players", PlayerDesc).Build();

            }
            return embed.Build();
        }

        public async Task PlayGame(SocketCommandContext context, Games game)
        {
            SocketUser user = context.User;
            if (game == Games.RussianRoulette)
            {
                RussianRoulette rGame;
                if (CheckIfHosting(user.Id, Games.RussianRoulette))
                {
                    rGame = russianRoulettes[user.Id];
                }
                else
                {
                    // games the user is already in (could be empty)
                    IEnumerable<RussianRoulette> games = russianRoulettes.Values.ToList().Where(g => g.ContainsPlayer(user.Id));
                    if (games.Count() > 0)
                    {
                        rGame = games.First();
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"You are not currently in a game.", false);
                        return;
                    }
                }

                await rGame.PullTrigger(context);

            }
        }

        // Reset a game
        public async Task DeleteGame(SocketCommandContext context, Games game)
        {
            SocketUser user = context.User;

            // Check if user is hosting a game
            if (game == Games.RussianRoulette && russianRoulettes.ContainsKey(user.Id))
            {
                RussianRoulette rGame;
                rGame = russianRoulettes[user.Id];
                foreach (var player in rGame.Players)
                {
                    _globalUserAccounts.GetById(player.Id).Coins += rGame.Bet;
                }
                russianRoulettes.Remove(user.Id);
                await context.Channel.SendMessageAsync($"**{user}** has ended their Russian Roulette game and the bet (**{rGame.Bet} coins**) was given back to the players.", false);
            }
            else
            {
                await context.Channel.SendMessageAsync($"You are not currently hosting a game.", false);
            }
        }

        private Action<ulong> GetDeleteGame(Games game)
        {
            if (game == Games.RussianRoulette)
            {
                var games = russianRoulettes;
                return (ulong id) => games.Remove(id);
            }

            throw new Exception("Not implemented");
        }


    }

}
