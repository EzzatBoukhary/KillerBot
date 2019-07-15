﻿using System;
 using Bot.Features.Economy;
using Bot.Features.GlobalAccounts;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using Bot.Extensions;
using static Bot.Global;
using Bot.Preconditions;
using Bot.Entities;
using System.Collections.Generic;
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace Bot.Modules
{
    public class Economy : ModuleBase<MiunieCommandContext>
    {
        private readonly IDailyMiunies _dailyMiunies;
        private readonly IMiuniesTransfer _miuniesTransfer;
        private readonly GlobalUserAccounts _globalUserAccounts;

        public Economy(IDailyMiunies dailyMiunies, IMiuniesTransfer miuniesTransfer, GlobalUserAccounts globalUserAccounts)
        {
            _dailyMiunies = dailyMiunies;

            _miuniesTransfer = miuniesTransfer;
            _globalUserAccounts = globalUserAccounts;

        }

        [Command("Daily"), Remarks("Gives you some coins, but can only be used once a day")]
        [Alias("GetDaily", "ClaimDaily")]
        public async Task GetDaily()
        {
            try
            {
                var accounts = _dailyMiunies.GetDaily(Context.User.Id);
                await ReplyAsync($"{Context.User.Mention}, you just claimed your {Constants.DailyMuiniesGain} daily coins!");
            }
            catch (InvalidOperationException e)
            {
                var timeSpanString = string.Format("{0:%h} hours {0:%m} minutes {0:%s} seconds", new TimeSpan(24, 0, 0).Subtract((TimeSpan)e.Data["sinceLastDaily"]));
                await ReplyAsync($"You already got your daily, {Context.User.Mention}.\nCome back in {timeSpanString}.");
            }
        }

        [Command("money"), Remarks("Shows how much money you have")]
        [Alias("Cash", "balance", "coins", "bal")]
        public async Task CheckMiunies()
        {
            var account = _globalUserAccounts.GetById(Context.User.Id);
            //DESCRIPTION REACTION MSG
            string GetCoinsCountReaction = "";
            if (account.NetWorth > 100000)
            {
                GetCoinsCountReaction = $"Wow, {Context.User.Username}! You're either cheating or you're really dedicated.";
            }
            else if (account.NetWorth > 50000)
            {
                GetCoinsCountReaction = $"You must be here often to get that amount, {Context.User.Username}. Thinkin of givin some out?";
            }
            else if (account.NetWorth > 20000)
            {
                GetCoinsCountReaction = $"That's enough to buy a house... \n\nFor me, not for you, shut up, {Context.User.Username}!";
            }
            else if (account.NetWorth > 10000)
            {
                GetCoinsCountReaction = $"{Context.User.Username} is kinda getting rich. Do we rob them or what?";
            }
            else if (account.NetWorth > 5000)
            {
                GetCoinsCountReaction = $"Is it just me or is {Context.User.Username} taking this economy a little too seriously?";
            }
            else if (account.NetWorth > 2500)
            {
                GetCoinsCountReaction = $"Great, {Context.User.Username}! Now you give me those coins.";
            }
            else if (account.NetWorth > 1100)
            {
                GetCoinsCountReaction = $"Looks like {Context.User.Username} is showing their wealth on the internet again.";
            }
            else if (account.NetWorth > 800)
            {
                GetCoinsCountReaction = $"Alright, {Context.User.Username}. Put the coins away and nobody gets hurt.";
            }
            else if (account.NetWorth > 550)
            {
                GetCoinsCountReaction = $"I like how {Context.User.Username} thinks that's impressive.";
            }
            else if (account.NetWorth > 200)
            {
                GetCoinsCountReaction = $"Ouch, {Context.User.Username}! If I knew that is all you've got, I would just DM you the amount. Embarrassing!";
            }
            else if (account.NetWorth == 0)
            {
                GetCoinsCountReaction = $"Yea, {Context.User.Username} is broke. What a surprise.";
            }
            else
                GetCoinsCountReaction = $"Yea, {Context.User.Username} you still have a lot to go.";
            //END
            var emb = new EmbedBuilder();
            emb.WithAuthor($"{Context.User}", Context.User.GetAvatarUrl());
            emb.WithColor(Color.Blue);
            emb.WithDescription($"\n \n{GetCoinsCountReaction} \n");
            emb.WithFooter($"Requested by: {Context.User}", Context.User.GetAvatarUrl());
            emb.AddField("Wallet", $"{account.Coins} coins", true);
            emb.AddField("Bank", $"{account.BankCoins} coins", true);
            emb.AddField("Net Worth", $"{account.NetWorth} coins", true);
            await ReplyAsync("", false, emb.Build());
        }

        [Command("money"), Remarks("Shows how much money the mentioned user has")]
        [Alias("Cash", "balance", "coins", "bal")]
        public async Task CheckMiuniesOther([Summary("user to check")]IGuildUser target)
        {
            var account = _globalUserAccounts.GetById(target.Id);
            //DESCRIPTION REACTION MSG
            string GetCoinsCountReaction = "";
            if (account.NetWorth > 100000)
            {
                GetCoinsCountReaction = $"Wow, {target.Username}! You're either cheating or you're really dedicated.";
            }
            else if (account.NetWorth > 50000)
            {
                GetCoinsCountReaction = $"You must be here often to get that amount, {target.Username}. Thinkin of givin some out?";
            }
            else if (account.NetWorth > 20000)
            {
                GetCoinsCountReaction = $"That's enough to buy a house... \n\nFor me, not for you, shut up, {target.Username}!";
            }
            else if (account.NetWorth > 10000)
            {
                GetCoinsCountReaction = $"{target.Username} is kinda getting rich. Do we rob them or what?";
            }
            else if (account.NetWorth > 5000)
            {
                GetCoinsCountReaction = $"Is it just me or is {target.Username} taking this economy a little too seriously?";
            }
            else if (account.NetWorth > 2500)
            {
                GetCoinsCountReaction = $"Great, {target.Username}! Now you give me those coins.";
            }
            else if (account.NetWorth > 1100)
            {
                GetCoinsCountReaction = $"Looks like {target.Username} is showing their wealth on the internet again.";
            }
            else if (account.NetWorth > 800)
            {
                GetCoinsCountReaction = $"Alright, {target.Username}. Put the coins away and nobody gets hurt.";
            }
            else if (account.NetWorth > 550)
            {
                GetCoinsCountReaction = $"I like how {target.Username} thinks that's impressive.";
            }
            else if (account.NetWorth > 200)
            {
                GetCoinsCountReaction = $"Ouch, {target.Username}! If I knew that is all you've got, I would just DM you the amount. Embarrassing!";
            }
            else if (account.NetWorth == 0)
            {
                GetCoinsCountReaction = $"Yea, {target.Username} is broke. What a surprise.";
            }
            else if (account.NetWorth < 0)
            {
                GetCoinsCountReaction = $"Well, looks like {target.Username} is in debt now.";
            }
            else
                GetCoinsCountReaction = $"Yea, {target.Username} you still have a lot to go.";
            //END
            var emb = new EmbedBuilder();
            emb.WithAuthor($"{target}", target.GetAvatarUrl());
            emb.WithColor(Color.Blue);
            emb.WithDescription($"\n \n{GetCoinsCountReaction} \n");
            emb.WithFooter($"Requested by: {Context.User}", Context.User.GetAvatarUrl());
            emb.AddField("Wallet", account.Coins, true);
            emb.AddField("Bank", account.BankCoins, true);
            emb.AddField("Net Worth", account.NetWorth, true);
            await ReplyAsync("", false, emb.Build());
        }

        [Command("Leaderboard"), Remarks("Shows a user list of the sorted by money. Pageable to see lower ranked users.")]
        [Alias("Top", "Top10", "Richest", "lb")]
        [Cooldown(5)]
        public async Task ShowRichesPeople(int page = 1)
        {
            if (page < 1)
            {
                await ReplyAsync("There's only 1 page at the moment...");
                return;
            }

            var guildUserIds = Context.Guild.Users.Select(user => user.Id);
            // Get only accounts of this server
            var accounts = _globalUserAccounts.GetFilteredAccounts(acc => guildUserIds.Contains(acc.Id));

            const int usersPerPage = 9;
            // Calculate the highest accepted page number => amount of pages we need to be able to fit all users in them
            // (amount of users) / (how many to show per page + 1) results in +1 page more every time we exceed our usersPerPage  
            var lastPageNumber = 1 + (accounts.Count / (usersPerPage + 1));
            if (page > lastPageNumber)
            {
                await ReplyAsync($"There are not that many pages...\nPage {lastPageNumber} is the last one...");
                return;
            }
            // Sort the accounts descending by Coins
            var ordered = accounts.OrderByDescending(acc => acc.NetWorth).ToList();

            var embB = new EmbedBuilder()
                .WithTitle("These are the richest people:")
                .WithFooter($"Page {page}/{lastPageNumber}");

            // Add fields to the embed with information of users according to the provided page we should show
            // Two conditions because:  1. Only get as many as we want 
            //                          2. The last page might not be completely filled so we have to interrupt early
            page--;
            for (var i = 1; i <= usersPerPage && i + usersPerPage * page <= ordered.Count; i++)
            {
                // -1 because we take the users non zero based input
                var account = ordered[i - 1 + usersPerPage * page];
                var user = Context.Client.GetUser(account.Id);

                //try to give it a medal in cases 1 - 3, if it is not possible just send it with out change
                var contentName = string.Empty;
                if (page == 0)
                {
                    switch (i)
                    {
                        case 1:
                            contentName = $"🥇 #{i + usersPerPage * page} {user.Username}";
                            break;
                        case 2:
                            contentName = $"🥈 #{i + usersPerPage * page} {user.Username}";
                            break;
                        case 3:
                            contentName = $"🥉 #{i + usersPerPage * page} {user.Username}";
                            break;
                        default:
                            contentName = $"#{i + usersPerPage * page} {user.Username}";
                            break;
                    }
                }
                else
                {
                    contentName = $"#{i + usersPerPage * page} {user.Username}";
                }
                embB.AddField(contentName, $"{account.NetWorth} Coins", true);
            }

            await ReplyAsync("", false, embB.Build());
        }

        [Command("Transfer")]
        [Remarks("Transfers specified amount of your coins to the mentioned person.")]
        [Alias("Give", "Gift")]
        public async Task TransferMinuies(
            [Summary("User to transfer money to")]IGuildUser target,
            [Summary("Amount of coins to transfer")]long amount)
        {
            if (amount < 0)
                throw new ArgumentException("<:KBfail:580129304592252995> Nice try giving the user a negative amount of coins. Sorry but no can do.");
            if (amount == 0)
                throw new ArgumentException("<:KBfail:580129304592252995> Oh wow, you really wanna transfer 0 coins to the user? Not cool. (amount should be more than 0)");
            if (target.IsBot == true)
                throw new ArgumentException("<:KBfail:580129304592252995> Cannot transfer coins to bots.");
            try
            {
                _miuniesTransfer.UserToUser(Context.User.Id, target.Id, amount);
                await ReplyAsync($"<a:KBtick:580851374070431774> **{Context.User.Username}** has given **{target.Username}** {amount} coin(s)!");
            }
            catch (InvalidOperationException e)
            {
                // TODO: Get Miunie phrase based on exception message.
                await ReplyAsync($"<:KBfail:580129304592252995> {e.Message}");
            }
        }

        [Command("newslot"), Summary("Creates a new slot machine if you feel the current one is unlucky"), Remarks("Usage: k!newslots [Optional: (amount of pieces)}]")]
        [Alias("newslots")]
        public async Task NewSlot([Summary("OPTIONAL: Amount of items in the slot machine")]int amount = 0)
        {
            Global.Slot = new Slot(amount);
            await ReplyAsync("<a:KBtick:580851374070431774> A new slot machine got generated! Good luck!");
        }

        [Command("slots"), Summary("Play the slots! Win or lose some coins!")]
        [Alias("slot")]
        public async Task SpinSlot(uint amount)
        {
            if (amount < 1)
            {
                await ReplyAsync("Oh, nice try. But we only spin for coins > 1");
                return;
            }
            var account = _globalUserAccounts.GetById(Context.User.Id);
            if (account.Coins < amount)
            {
                await ReplyAsync($"Sorry but it seems like you don't have enough coins... You only have {account.Coins}.");
                return;
            }

            account.Coins -= amount;
            _globalUserAccounts.SaveAccounts(Context.User.Id);

            var slotEmojis = Global.Slot.Spin();
            var payoutAndFlavour = Global.Slot.GetPayoutAndFlavourText(amount);

            if (payoutAndFlavour.Item1 > 0)
            {
                account.Coins += payoutAndFlavour.Item1;
                _globalUserAccounts.SaveAccounts();
            }

            await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
            await Task.Delay(1000);
            await ReplyAsync($"You used **{amount} coin(s)** {payoutAndFlavour.Item2}");
        }

        [Command("showslots"), Summary("Shows the configuration of the current slot machine")]
        [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.None)]
        [Alias("showslot")]
        public async Task ShowSlot()
        {
            await ReplyAsync(string.Join("\n", Global.Slot.GetCylinderEmojis(true)));
        }

        [Command("work")]
        [Summary("Work every hour and receive some coins!")]
        [Cooldown(3600)]
        public async Task Work()
        {
            var account = _globalUserAccounts.GetById(Context.User.Id);
            var emb = new EmbedBuilder();
            var result = Global.Rng.Next(Constants.WorkRewardMinMax.Item1, Constants.WorkRewardMinMax.Item2 + 1);
            account.Coins += result;
            _globalUserAccounts.SaveAccounts();
            emb.WithColor(Color.Green);
            emb.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
            emb.WithCurrentTimestamp();
            emb.WithDescription($"You work and recieve **{result} coins** for your hard work. <a:KBtick:580851374070431774> ");
            await ReplyAsync("", false, emb.Build());

        }

        [Command("deposit"), Alias("dep", "dp")]
        [Summary("Deposit your money to a safer place.")]
        public async Task dep(uint amount = 0)
        {
            var account = _globalUserAccounts.GetById(Context.User.Id);
            if (account.Coins < amount)
            {
                await ReplyAsync($"Sorry but it seems like you don't have enough coins... You only have {account.Coins} coins in your wallet.");
                return;
            }
            if (amount == 0)
            {
                await ReplyAsync("You can't deposit 0 coins... :neutral_face:");
            }
            else
            {
                await ReplyAsync($"<a:KBtick:580851374070431774> {amount} coins were deposited to your bank.");
                account.Coins -= amount;
                account.BankCoins += amount;
                _globalUserAccounts.SaveAccounts(Context.User.Id);
            }
        }

        //deposit all
        [Command("deposit"), Alias("dep", "dp")]
        [Summary("Deposit your money to a safer place.")]
        public async Task depALL(string all)
        {
            if (all == "all")
            {
                var account = _globalUserAccounts.GetById(Context.User.Id);
                if (account.Coins == 0)
                {
                    await ReplyAsync("You can't deposit 0 coins... :neutral_face:");
                }
                else
                {
                    await ReplyAsync($"<a:KBtick:580851374070431774> {account.Coins} coins were deposited to your bank.");
                    account.BankCoins += account.Coins;
                    account.Coins -= account.Coins;
                    _globalUserAccounts.SaveAccounts(Context.User.Id);
                }
            }
        }

        [Command("withdraw"), Alias("with", "wd")]
        [Summary("Withdraw your money from the bank and put some stuff into that empty wallet.")]
        public async Task with(uint amount = 0)
        {
            var account = _globalUserAccounts.GetById(Context.User.Id);
            if (account.BankCoins < amount)
            {
                await ReplyAsync($"Sorry but it seems like you don't have enough coins... You only have {account.BankCoins} coin(s) in your bank.");
                return;
            }
            if (amount == 0)
            {
                await ReplyAsync("You can't withdraw 0 coins... :neutral_face:");
            }
            else
            {
                await ReplyAsync($"<a:KBtick:580851374070431774> {amount} coins were withdrew from your bank.");
                account.BankCoins -= amount;
                account.Coins += amount;
                _globalUserAccounts.SaveAccounts(Context.User.Id);
            }
        }

        //withdraw all
        [Command("withdraw"), Alias("with", "wd")]
        [Summary("Withdraw your money from the bank and put some stuff into that empty wallet.")]
        public async Task withALL(string all)
        {
            var account = _globalUserAccounts.GetById(Context.User.Id);
            if (all == "all")
            {
                if (account.BankCoins == 0)
                {
                    await ReplyAsync("You can't withdraw 0 coins... :neutral_face:");
                }
                else
                {
                    await ReplyAsync($"<a:KBtick:580851374070431774> {account.BankCoins} coins were withdrew from your bank.");
                    account.Coins += account.BankCoins;
                    account.BankCoins -= account.BankCoins;
                    _globalUserAccounts.SaveAccounts(Context.User.Id);
                }
            }
        }

        /*[Command("roulette")]
        public async Task RouletteAsync(uint amount = 0)
        {
            if (amount < 50)
            {
                await ReplyAsync("Your bet should be at least **50 coins**");
            }
            var answers = new[]
            {
                ":gun: *click*, no bullet in there for you this round.\r\n",
                ":gun: :boom:, you died! :skull:\r\n", ":gun: *click*, no bullet in there for you this round.\r\n"
            };

            var answer = answers[Global.Rng.Next(3) % 2]; // 1 out of 3 possibility of death
            await ReplyAsync(answer);
        }*/

        [Command("rob")]
        [Alias("steal")]
        [Remarks("Really want that money that person has? go for it, but don't get caught!")]
        public async Task Rob([NoSelf] IGuildUser target)
        {

            string[] FailSuccess = new string[]
       {
      $"{Context.User} has robbed {target} " ,
      $"{Context.User} was caught trying to rob {target} "

       };
            var robbed = _globalUserAccounts.GetById(target.Id);
            var robber = _globalUserAccounts.GetById(Context.User.Id);
            var sinceLastRobbery = DateTime.UtcNow - robber.LastRobbery;

            if (sinceLastRobbery.TotalHours < 24)
            {
                var e = new InvalidOperationException(Constants.ExRobberyTooSoon);
                e.Data.Add("sinceLastRobbery", sinceLastRobbery);
                throw e;
            }
            if (target.IsBot == true)
                throw new ArgumentException("<:KBfail:580129304592252995> Cannot rob bots.");

            if (robbed.Coins < 1)
                await ReplyAsync("User has no money to rob :shrug:");

            //If robbed user has money
            else
            {
                Random rnd = new Random();
                int randomMessage = rnd.Next(1, 101);
                randomMessage = randomMessage <= 70 ? 1 : 0;
                string RobbingResult = FailSuccess[randomMessage];


                if (randomMessage == 1) //ROBBERY FAILS
                {
                    long amount = (long)((float)robber.Coins * (rnd.Next(15, 26) / 100.0)); //How much can the robber lose money.
                    EmbedBuilder fail = new EmbedBuilder();
                    fail.WithTitle("<:rob:593876945205329966> == ROBBING == <:rob:593876945205329966>");
                    fail.WithColor(new Color(255, 0, 0));
                    fail.WithCurrentTimestamp();


                    if (robber.Coins > 0) // If wallet has coins and not debt
                    {
                        robber.Coins -= amount;
                        fail.WithDescription(RobbingResult + $"and was fined {amount} coin(s)");
                    }


                    else if (robber.Coins == 0 && robber.BankCoins == 0) // If the robber is completely broke.
                    {
                        fail.WithDescription(RobbingResult + $"and due to being broke, the police gave them just a warning.");
                    }


                    else if (robber.BankCoins != 0) // If the robber has no money in their wallet but does in the bank.
                    {
                        long amountbank = (long)((float)robber.BankCoins * (rnd.Next(15, 26) / 100.0));
                        robber.Coins -= amountbank;
                        fail.WithDescription(RobbingResult + $"and was fined {amountbank} coin(s)");
                    }


                    _globalUserAccounts.SaveAccounts();
                    robber.LastRobbery = DateTime.UtcNow;
                    fail.WithFooter($"New Wallet balance: {robber.Coins}", Context.User.GetAvatarUrl());
                    await ReplyAsync("", false, fail.Build());
                }
                else  //ROBBERYS SUCCEEDS
                {
                    long amount = (long)((float)robbed.Coins * (rnd.Next(10, 31) / 100.0)); //How much can the robber steal money.

                    EmbedBuilder success = new EmbedBuilder();
                    success.WithTitle("<:rob:593876945205329966> == ROBBING == <:rob:593876945205329966>");
                    success.WithDescription(RobbingResult + $"and got {amount} coin(s)");
                    success.WithColor(new Color(0, 255, 0));
                    success.WithCurrentTimestamp();
                    robber.Coins += amount;
                    robbed.Coins -= amount;
                    robber.LastRobbery = DateTime.UtcNow;
                    _globalUserAccounts.SaveAccounts();
                    success.WithFooter($"New Wallet balance: {robber.Coins}", Context.User.GetAvatarUrl());
                    await ReplyAsync("", false, success.Build());
                }

            }

        }
        //Rob command ends here.

  /*      [Group("shop")]
        [Summary("The economy shop of KillerBot, use your coins to buy some cool stuff!")]
        public class Shop : ModuleBase<MiunieCommandContext>
        {
            private readonly GlobalUserAccounts _globalUserAccounts;
            private readonly List<Item> _items;
            public Shop(GlobalUserAccounts globalUserAccounts)
            {
                _globalUserAccounts = globalUserAccounts;
                _items = new List<Item> { new Item("ugh", 33),
                new Item("Item 2", 50),
                new Item("Item 3", 22),
                new Item("Item 4", 50),
                new Item("Item 5", 50),
                new Item("Item 6", 50),
                new Item("Item 7", 50),
                new Item("Item 8", 50),
                new Item("Item 9", 50),
                new Item("Item 10", 50),
                new Item("Item 11", 50)
            };
            }
            [Command("")]
            public async Task ShowShop(int page = 1)
            {
                const int ItemsPerPage = 9;
                // Calculate the highest accepted page number => amount of pages we need to be able to fit all users in them
                // (amount of users) / (how many to show per page + 1) results in +1 page more every time we exceed our usersPerPage  
                var lastPageNumber = 1 + (_items.Count / (ItemsPerPage + 1));
                if (page > lastPageNumber)
                {
                    await ReplyAsync($"There are not that many pages...\nPage {lastPageNumber} is the last one...");
                    return;
                }
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("KillerBot Shop List: ");
                embed.WithFooter($"Page {page}/{lastPageNumber}");
                embed.WithColor(Color.Teal);
                var application = await Context.Client.GetApplicationInfoAsync(); 
                embed.WithThumbnailUrl(application.IconUrl);  
                int endIndex = page * ItemsPerPage;
                int startIndex = endIndex - ItemsPerPage;
                for (int i = startIndex; i < endIndex && i < _items.Count; i++)
                {
                    embed.AddField($"{_items[i].name}:", $"{_items[i].price} coins", false);
                }
                await ReplyAsync("", false, embed.Build());
            }
            [Command("buy")]
            [Summary("Buy a specific item in the KillerBot Economy Shop.")]
            public async Task Buy(string item)
            {
                var account = _globalUserAccounts.GetById(Context.User.Id);
                foreach (var i in _items)
                {
                    if (item == i.name)
                    {
                        var BoughtItem = new ItemEntry(item);
                        account.Bought_Items.Add(BoughtItem);
                        account.Coins -= i.price;
                        _globalUserAccounts.SaveAccounts(Context.User.Id);
                        await ReplyAsync($"You succesfully bought {i.name}");
                    }

                }
            } 
        } */
    }
}
