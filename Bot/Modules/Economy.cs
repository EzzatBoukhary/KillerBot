using System;
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
using System.Runtime.InteropServices;
using Bot.Helpers;
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace Bot.Modules
{

    public class GenericCompare<T> : IEqualityComparer<T> where T : class
    {
        private Func<T, object> _expr { get; set; }
        public GenericCompare(Func<T, object> expr)
        {
            this._expr = expr;
        }
        public bool Equals(T x, T y)
        {
            var first = _expr.Invoke(x);
            var sec = _expr.Invoke(y);
            if (first != null && first.Equals(sec))
                return true;
            else
                return false;
        }
        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
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

        [Command("Daily"), Summary("Gives you some coins, but can only be used once a day")]
        [Alias("GetDaily", "ClaimDaily")]
        public async Task GetDaily()
        {
            var account = _globalUserAccounts.GetById(Context.User.Id);

            for (int i = 0; i < account.Bought_Items.Count; i++) // If the user has specific items bought from the shop
            {
                if (account.Bought_Items[i].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                {
                    if (DateTime.UtcNow - account.Bought_Items[i].Date > account.Bought_Items[i].Duration)
                    {
                        account.Bought_Items.Remove(account.Bought_Items[i]);
                        _globalUserAccounts.SaveAccounts(Context.User.Id);
                        
                    }
                    else if (account.Bought_Items[i].name == "Double Day")
                    {
                        try
                        {
                            var accounts = _dailyMiunies.GetDaily(Context.User.Id);
                            account.Coins += Constants.DailyMuiniesGain;
                            _globalUserAccounts.SaveAccounts(Context.User.Id);
                            var time_left = account.Bought_Items[i].Duration - (DateTime.UtcNow - account.Bought_Items[i].Date);
                            await ReplyAsync($"{Context.User.Mention}, you just claimed your {Constants.DailyMuiniesGain * 2} daily coins! {account.Bought_Items[i].name} is in effect! (**{time_left.Days}d ,{time_left.Hours}h ,{time_left.Minutes}m left**)");
                        }
                        catch (InvalidOperationException e)
                        {
                            var timeSpanString = string.Format("{0:%h} hours {0:%m} minutes {0:%s} seconds", new TimeSpan(24, 0, 0).Subtract((TimeSpan)e.Data["sinceLastDaily"]));
                            await ReplyAsync($"You already got your daily, {Context.User.Mention}.\nCome back in {timeSpanString}.");
                        }
                        return;
                    }
                   
                }
                
                continue;
            }
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

        [Command("money"), Summary("Shows how much money you have")]
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

        [Command("money"), Summary("Shows how much money the mentioned user has")]
        [Alias("Cash", "balance", "coins", "bal")]
        public async Task CheckMiuniesOther([Remainder] [Summary("user to check")]IGuildUser target)
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

        [Command("Leaderboard"), Summary("Shows a user list of the sorted by money. Pageable to see lower ranked users.")]
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
                .WithTitle("These are the richest people in this server:")
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
        [Summary("Transfers specified amount of your coins to the mentioned person.")]
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

        [Command("newslot"), Summary("Creates a new slot machine if you feel the current one is unlucky"), Remarks("Usage: k!newslots [Optional: (amount of pieces)]")]
        [Alias("newslots")]
        public async Task NewSlot([Summary("OPTIONAL: Amount of items in the slot machine")]int amount = 0)
        {
            Global.Slot = new Slot(amount);
            await ReplyAsync("<a:KBtick:580851374070431774> A new slot machine got generated! Good luck!");
        }

        [Command("slots"), Summary("Play the slots! Win or lose some coins!")]
        [Alias("slot")]
        [Ratelimit(5, 3, Measure.Minutes ,RatelimitFlags.None)]
        public async Task SpinSlot(uint amount)
        {
            if (amount < 1 || amount > 10000)
            {
                await ReplyAsync("Sorry but you should insert an amount more than 1 and less than (or equal to) 10,000 coins.");
                return;
            }
            var account = _globalUserAccounts.GetById(Context.User.Id);
            if (account.Coins < amount)
            {
                await ReplyAsync($"Sorry but it seems like you don't have enough coins in your wallet... You only have {account.Coins}.");
                return;
            }

            var slotEmojis = Global.Slot.Spin();
            var payoutAndFlavour = Global.Slot.GetPayoutAndFlavourText(amount);
            if (payoutAndFlavour.Item1 > 10000)
            {
                await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
                await Task.Delay(700);
                await ReplyAsync($"You used **{amount} coins** but you'll only get **10,000 coins** for that since you won more than the 10k maximum amount.");
                account.Coins += 10000;
                _globalUserAccounts.SaveAccounts(Context.User.Id);
                return;
            }
            bool Double = false;
            bool NoLoss = false;
            for (int i = 0; i < account.Bought_Items.Count; i++)
            {

                if (account.Bought_Items[i].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                {
                    if (DateTime.UtcNow - account.Bought_Items[i].Date > account.Bought_Items[i].Duration)
                    {
                        account.Bought_Items.Remove(account.Bought_Items[i]);
                        _globalUserAccounts.SaveAccounts(Context.User.Id);
                    }
                    else if (account.Bought_Items[i].name == "No Loss Day")
                    {
                        NoLoss = true;
                    }
                    else if (account.Bought_Items[i].name == "Double Day")
                    {
                        if (payoutAndFlavour.Item1 * 2 > 10000)
                        {
                            await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
                            await Task.Delay(700);
                            await ReplyAsync($"You used **{amount} coins** but you'll only get **10,000 coins** for that since you won more than the 10k maximum amount.");
                            account.Coins += 10000;
                            _globalUserAccounts.SaveAccounts(Context.User.Id);
                            return;
                        }
                        Double = true;
                    }

                }
                continue;
            }
            if (NoLoss == true && Double == false)
            {
                if (payoutAndFlavour.Item1 > 0)
                {
                    account.Coins += payoutAndFlavour.Item1;
                    _globalUserAccounts.SaveAccounts();
                    await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
                    await Task.Delay(700);
                    await ReplyAsync($"You used **{amount} coin(s)** {payoutAndFlavour.Item2}");
                }
                else
                {
                    await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
                    await Task.Delay(700);
                    await ReplyAsync($"You used **{amount} coin(s)** {payoutAndFlavour.Item2} \n:cloud_tornado: PHEW! That was close, **No Loss Day** just saved you from losing that money! ");
                }

            }
            else if (Double == true && NoLoss == false)
            {
                account.Coins -= amount;
                _globalUserAccounts.SaveAccounts(Context.User.Id);
                if (payoutAndFlavour.Item1 > 0)
                {
                    account.Coins += payoutAndFlavour.Item1 * 2;
                    _globalUserAccounts.SaveAccounts(Context.User.Id);
                    await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
                    await Task.Delay(700);
                    await ReplyAsync($"You used **{amount} coin(s)** {payoutAndFlavour.Item2} \n:money_mouth: **Double Day** is in effect and gave you double the amount! ({payoutAndFlavour.Item1 * 2} coins)");
                }
                else
                {
                    await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
                    await Task.Delay(700);
                    await ReplyAsync($"You used **{amount} coin(s)** {payoutAndFlavour.Item2} ");
                }
            }
            else if (Double == true && NoLoss == true)
            {
                if (payoutAndFlavour.Item1 > 0)
                {
                    account.Coins += payoutAndFlavour.Item1 * 2;
                    _globalUserAccounts.SaveAccounts();
                    await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
                    await Task.Delay(700);
                    await ReplyAsync($"You used **{amount} coin(s)** {payoutAndFlavour.Item2} \n:money_mouth: **Double Day** is in effect and gave you double the amount! ({payoutAndFlavour.Item1 * 2} coins)");
                }
                else
                {
                    await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
                    await Task.Delay(700);
                    await ReplyAsync($"You used **{amount} coin(s)** {payoutAndFlavour.Item2} \n:cloud_tornado: PHEW! That was close, **No Loss Day** just saved you from losing that money! ");
                }
            }
            else
            {
                account.Coins -= amount;
                _globalUserAccounts.SaveAccounts(Context.User.Id);
                if (payoutAndFlavour.Item1 > 0)
                {
                    account.Coins += payoutAndFlavour.Item1;
                    _globalUserAccounts.SaveAccounts();

                }
                    await ReplyAsync($"**[  :slot_machine:  SLOTS ]** \n------------------\n{slotEmojis}");
                    await Task.Delay(700);
                    await ReplyAsync($"You used **{amount} coin(s)** {payoutAndFlavour.Item2}");
            }
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
        [Cooldown(7200)]
        public async Task Work()
        {
            
            int randomIndex = Global.Rng.Next(Constants.Jobs.Length);
            string job = Constants.Jobs[randomIndex];
            if (job.Contains("A") || job.Contains("E") || job.Contains("I") || job.Contains("O") || job.Contains("U") || job.Contains("Y"))
            {
                job = $"an {job}";
            }
            else
            {
                job = $"a {job}";
            }
            var account = _globalUserAccounts.GetById(Context.User.Id);
            var emb = new EmbedBuilder();
            var result = Global.Rng.Next(Constants.WorkRewardMinMax.Item1, Constants.WorkRewardMinMax.Item2 + 1);
            for (int i = 0; i < account.Bought_Items.Count; i++) // If the user has specific items bought from the shop
            {
                if (account.Bought_Items[i].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                {
                    if (DateTime.UtcNow - account.Bought_Items[i].Date > account.Bought_Items[i].Duration)
                    {
                        account.Bought_Items.Remove(account.Bought_Items[i]);
                        _globalUserAccounts.SaveAccounts(Context.User.Id);
                    }
                    else if (account.Bought_Items[i].name == "Double Day")
                    {
                        account.Coins += result * 2;
                        _globalUserAccounts.SaveAccounts();
                        emb.WithColor(Color.Green);
                        emb.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
                        emb.WithCurrentTimestamp();
                        emb.WithDescription($"<a:KBtick:580851374070431774> You work as {job} and earn **{result * 2} coins**. Double Day is in effect and gave you double the amount!");
                        await ReplyAsync("", false, emb.Build());
                        return;
                    }
                    
                }
                continue;
            }
            account.Coins += result;
            _globalUserAccounts.SaveAccounts();
            emb.WithColor(Color.Green);
            emb.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl());
            emb.WithCurrentTimestamp();
            emb.WithDescription($"<a:KBtick:580851374070431774> You work as {job} and earn **{result} coins**.");
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

        [Command("rob")]
        [Alias("steal")]
        [Remarks("Really want that money that person has? go for it, but don't get caught!")]
        public async Task Rob([Remainder] [NoSelf] IGuildUser target = null)
        {
            if (target == null)
            {
                await ReplyAsync("<:rob:593876945205329966> Please specify the user you want to rob!");
                return;
            }
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
                bool Double = false;
                bool NoLoss = false;
                for (int i = 0; i < robber.Bought_Items.Count; i++) // If the user has specific items bought from the shop
                {
                    if (robber.Bought_Items[i].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                    {
                        if (DateTime.UtcNow - robber.Bought_Items[i].Date > robber.Bought_Items[i].Duration)
                        {
                            robber.Bought_Items.Remove(robber.Bought_Items[i]);
                        }
                        else if (robber.Bought_Items[i].name == "Double Day")
                        {
                            Double = true;
                        }
                        else if (robber.Bought_Items[i].name == "No Loss Day")
                        {
                            NoLoss = true;
                        }
                       
                    }
                    continue;
                }
                if (NoLoss == false && Double == true)
                {
                    if (randomMessage == 1) //ROBBERY FAILS
                    {
                        long amount = (long)((float)robber.NetWorth * (rnd.Next(15, 26) / 100.0)); //How much can the robber lose money.
                        EmbedBuilder fail = new EmbedBuilder();
                        fail.WithTitle("<:rob:593876945205329966> == ROBBERY == <:rob:593876945205329966>");
                        fail.WithColor(new Color(255, 0, 0));
                        fail.WithCurrentTimestamp();


                        if (robber.NetWorth > 0 && amount != 0) // If wallet has coins and not debt
                        {
                            robber.Coins -= amount;
                            fail.WithDescription(RobbingResult + $"and was fined {amount} coin(s)");
                        }


                        else if (robber.NetWorth == 0 || amount == 0) // If the robber is completely broke.
                        {
                            fail.WithDescription(RobbingResult + $"and due to being broke, the police gave them just a warning.");
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
                        success.WithTitle("<:rob:593876945205329966> == ROBBERY == <:rob:593876945205329966>");
                        success.WithDescription(RobbingResult + $"and got {amount * 2} coin(s). **Double Day** is in effect and gave you double the amount!");
                        success.WithColor(new Color(0, 255, 0));
                        success.WithCurrentTimestamp();
                        robber.Coins += amount * 2;
                        robbed.Coins -= amount;
                        robber.LastRobbery = DateTime.UtcNow;
                        _globalUserAccounts.SaveAccounts();
                        success.WithFooter($"New Wallet balance: {robber.Coins}", Context.User.GetAvatarUrl());
                        await ReplyAsync("", false, success.Build());
                    }
                    return;
                }
                else if (NoLoss == true && Double == false)
                {
                    if (randomMessage == 1) //ROBBERY FAILS
                    {
                        long amount = (long)((float)robber.NetWorth * (rnd.Next(15, 26) / 100.0)); //How much can the robber lose money.
                        EmbedBuilder fail = new EmbedBuilder();
                        fail.WithTitle("<:rob:593876945205329966> == ROBBERY == <:rob:593876945205329966>");
                        fail.WithColor(new Color(255, 0, 0));
                        fail.WithCurrentTimestamp();


                        if (robber.NetWorth > 0 && amount != 0) // If wallet has coins and not debt
                        {
                            fail.WithDescription(RobbingResult + $"and was fined {amount} coin(s). :spy: YEET, you manage to get away from that fine with the help of **No Loss Day** though!");
                        }


                        else if (robber.NetWorth == 0 || amount == 0) // If the robber is completely broke.
                        {
                            fail.WithDescription(RobbingResult + $"and due to being broke, the police gave them just a warning.");
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
                        success.WithTitle("<:rob:593876945205329966> == ROBBERY == <:rob:593876945205329966>");
                        success.WithDescription(RobbingResult + $"and got {amount} coin(s).");
                        success.WithColor(new Color(0, 255, 0));
                        success.WithCurrentTimestamp();
                        robber.Coins += amount;
                        robbed.Coins -= amount;
                        robber.LastRobbery = DateTime.UtcNow;
                        _globalUserAccounts.SaveAccounts();
                        success.WithFooter($"New Wallet balance: {robber.Coins}", Context.User.GetAvatarUrl());
                        await ReplyAsync("", false, success.Build());
                    }
                    return;
                }
                else if (NoLoss == true && Double == true)
                {
                    if (randomMessage == 1) //ROBBERY FAILS
                    {
                        long amount = (long)((float)robber.NetWorth * (rnd.Next(15, 26) / 100.0)); //How much can the robber lose money.
                        EmbedBuilder fail = new EmbedBuilder();
                        fail.WithTitle("<:rob:593876945205329966> == ROBBERY == <:rob:593876945205329966>");
                        fail.WithColor(new Color(255, 0, 0));
                        fail.WithCurrentTimestamp();


                        if (robber.NetWorth > 0 && amount != 0) // If wallet has coins and not debt
                        {
                            fail.WithDescription(RobbingResult + $"and was fined {amount} coin(s). :spy: YEET, you manage to get away from that fine with the help of **No Loss Day** though!");
                        }


                        else if (robber.NetWorth == 0 || amount == 0) // If the robber is completely broke.
                        {
                            fail.WithDescription(RobbingResult + $"and due to being broke, the police gave them just a warning.");
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
                        success.WithTitle("<:rob:593876945205329966> == ROBBERY == <:rob:593876945205329966>");
                        success.WithDescription(RobbingResult + $"and got {amount * 2} coin(s). **Double Day** is in effect and gave you double the amount!");
                        success.WithColor(new Color(0, 255, 0));
                        success.WithCurrentTimestamp();
                        robber.Coins += amount * 2;
                        robbed.Coins -= amount;
                        robber.LastRobbery = DateTime.UtcNow;
                        _globalUserAccounts.SaveAccounts();
                        success.WithFooter($"New Wallet balance: {robber.Coins}", Context.User.GetAvatarUrl());
                        await ReplyAsync("", false, success.Build());
                    }
                    return;
                }
                    if (randomMessage == 1) //ROBBERY FAILS
                {
                    long amount = (long)((float)robber.NetWorth * (rnd.Next(15, 26) / 100.0)); //How much can the robber lose money.
                    EmbedBuilder fail = new EmbedBuilder();
                    fail.WithTitle("<:rob:593876945205329966> == ROBBERY == <:rob:593876945205329966>");
                    fail.WithColor(new Color(255, 0, 0));
                    fail.WithCurrentTimestamp();


                    if (robber.NetWorth > 0 && amount != 0) // If wallet has coins and not debt
                    {
                        robber.Coins -= amount;
                        fail.WithDescription(RobbingResult + $"and was fined {amount} coin(s)");
                    }


                    else if (robber.NetWorth == 0 || amount == 0) // If the robber is completely broke.
                    {
                        fail.WithDescription(RobbingResult + $"and due to being broke, the police gave them just a warning.");
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
                    success.WithTitle("<:rob:593876945205329966> == ROBBERY == <:rob:593876945205329966>");
                    success.WithDescription(RobbingResult + $"and got {amount * 2} coin(s).");
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

        // RPS

        [Command("rps")]
        [Ratelimit(5, 2, Measure.Minutes, RatelimitFlags.None)]
        [Summary("Do this command to know more info about the game. for example.'")]
        [Example("k!rps 30 rock")]
        [Remarks("Put a bet and play rock paper scissors")]

        public async Task Rps([Summary("Your bet (positive number)")] uint bet = 0 , [Summary("Your pick")] string input = null)
        {
            var user = _globalUserAccounts.GetById(Context.User.Id);
            bool Double = false;
            bool NoLoss = false;
            if (user.Coins < bet)
            {
                await ReplyAsync("You betted more coins than what you have in your wallet!");
                return;
            }
            var embed = new EmbedBuilder();

            for (int i = 0; i < user.Bought_Items.Count; i++) // If the user has specific items bought from the shop
            {
                if (user.Bought_Items[i].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                {
                    if (DateTime.UtcNow - user.Bought_Items[i].Date > user.Bought_Items[i].Duration)
                    {
                        user.Bought_Items.Remove(user.Bought_Items[i]);
                    }
                    else if (user.Bought_Items[i].name == "Double Day")
                    {
                        Double = true;
                    }
                    else if (user.Bought_Items[i].name == "No Loss Day")
                    {
                        NoLoss = true;
                    }

                }
                continue;
            }

            if (input == null || bet == 0)
            {
                await ReplyAsync(
                    " To play rock, paper, scissors:" +
                    "\n\n Specify the amount of coins you want to bet, then" +
                    "\n:new_moon: type `rock` or `r` to pick rock" +
                    "\n\n:newspaper: type `paper` or `p` to pick paper" +
                    "\n\n✂️ type `scissors` or `s` to pick scissors" 
                );
            }

            else
            {
                int pick;
                switch (input)

                {

                    case "r":

                    case "rock":

                        pick = 0;

                        break;

                    case "p":

                    case "paper":

                        pick = 1;

                        break;

                    case "scissors":

                    case "s":

                        pick = 2;

                        break;

                    default:
                        await ReplyAsync("Only choose one of the 3 weapons! (rock/paper/scissors)");
                        return;

                }
                var choice = new Random().Next(0, 3);
                string msg;

                if (pick == choice)
                {
                    embed.WithTitle("Draw!");
                    embed.WithColor(new Color(255, 140, 0));
                    msg = "We both chose: " + GetRpsPick(pick) + "\nNah, not today.\nNo money was gained/lost.";
                }

                else if (pick == 0 && choice == 1 ||
                         pick == 1 && choice == 2 ||
                         pick == 2 && choice == 0)
                {
                    embed.WithTitle("You lose!");
                    embed.WithColor(new Color(255, 0, 0));
                    if (NoLoss == true)
                    {
                        msg = "My Pick: " + GetRpsPick(choice) + " Beats Your Pick: " + GetRpsPick(pick) +
                          $"\nTry Again boi! \nYou lost **{bet} coins**...or NOT \n:spy: **No Loss Day** is here to save you!";
                    }
                    else
                    {
                        msg = "My Pick: " + GetRpsPick(choice) + " Beats Your Pick: " + GetRpsPick(pick) +
                          $"\nTry Again boi! \nYou lost **{bet} coins**!";
                        user.Coins -= bet;
                        _globalUserAccounts.SaveAccounts(user.Id);
                    }
                }
                else
                {
                    embed.WithTitle("You win!");
                    embed.WithColor(new Color(0, 255, 0));
                    if (Double == true)
                    {
                        msg = "Your Pick: " + GetRpsPick(pick) + " Beats mine: " + GetRpsPick(choice) +
                               $"\nCongratulations! \nYou gained **{bet * 2} coins**! \n**Double Day** is in effect and gave you double the amount! **({bet * 3} coins)**";
                        user.Coins -= bet;
                        user.Coins += bet * 3;
                        _globalUserAccounts.SaveAccounts(user.Id);
                    }
                    else
                    {
                        msg = "Your Pick: " + GetRpsPick(pick) + " Beats mine: " + GetRpsPick(choice) +
                              $"\nCongratulations! \nYou gained **{bet * 2} coins**!";
                        user.Coins -= bet;
                        user.Coins += bet * 2;
                        _globalUserAccounts.SaveAccounts(user.Id);
                    }
                }                
                    embed.WithDescription($"{msg}");
                    embed.WithFooter($"New Wallet balance: {user.Coins}", Context.User.GetAvatarUrl());
            
                await ReplyAsync("", false, embed.Build());
            }
        }

        private static string GetRpsPick(int p)

        {

            switch (p)

            {

                case 0:

                    return ":new_moon:";

                case 1:

                    return ":newspaper:";

                default:

                    return "✂️";

            }

        }
        // END
        // ============ Economy Shop ============

        [Command("inventory"), Alias("inven")]
        [Summary("The list of items you have if there's any.")]
        [Remarks("To get new stuff do `k!buy {item}`")]
        public async Task Inv([Summary("The number of the page in the shop menu if any other pages exist.")]int page = 1)
        {
            const int ItemsPerPage = 3;
            var account = _globalUserAccounts.GetById(Context.User.Id);
            // Calculate the highest accepted page number => amount of pages we need to be able to fit all users in them
            // (amount of users) / (how many to show per page + 1) results in +1 page more every time we exceed our usersPerPage  
            var lastPageNumber = 1 + (account.Bought_Items.Count / (ItemsPerPage + 1));
            if (page > lastPageNumber)
            {
                await ReplyAsync($"There are not that many pages...\nPage {lastPageNumber} is the last one...");
                return;
            }
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("Inventory", Context.User.GetAvatarUrl());
            embed.WithFooter($"Page {page}/{lastPageNumber} (The time is set by UTC timezone)");
            embed.WithColor(Color.Blue);
            embed.WithTitle("This is the list of items you have:");
            int endIndex = page * ItemsPerPage;
            int startIndex = endIndex - ItemsPerPage;
            for (int i = startIndex; i < endIndex && i < account.Bought_Items.Count; i++)
            {
                var duration = "";
                if (account.Bought_Items[i].Duration == new TimeSpan(long.MaxValue))
                    duration = "Permanent";
                else if (DateTime.UtcNow - account.Bought_Items[i].Date > account.Bought_Items[i].Duration)
                {
                    duration = "The item's duration is finished.";
                }
                else
                {
                    var time_left = account.Bought_Items[i].Duration - (DateTime.UtcNow - account.Bought_Items[i].Date);
                    duration = $"{time_left.Days}D , {time_left.Hours}h , {time_left.Minutes}m";
                }
                embed.Description += $":white_small_square: **{account.Bought_Items[i].name}** \n \n:calendar: Bought on: {account.Bought_Items[i].Date.ToShortDateString()} {account.Bought_Items[i].Date.ToShortTimeString()} \n \n:alarm_clock: Time left: {duration}\n \n \n";
            }
            if (account.Bought_Items.Count == 0)
            {
                embed.Description += "There's nothing here... :worried: \n \n \nYou can do `k!shop` and check out what you can get yourself there!";
            }
            await ReplyAsync("", false, embed.Build());
        }

        [Group("shop")]
        [Summary("The economy shop of KillerBot, use your coins to buy some cool stuff!")]
        public class Shop : ModuleBase<MiunieCommandContext>
        {
            private readonly GlobalUserAccounts _globalUserAccounts;
            private readonly List<Item> _items;
            public Shop(GlobalUserAccounts globalUserAccounts)
            {
                _globalUserAccounts = globalUserAccounts;
                _items = new List<Item> { new Item("KillerBot Starter Pack", 2000, "Buy KillerBot's starter pack to get some special stuff! **[COMING SOON]**"),
                new Item("No Loss Day", 4000, "No money loss for a whole 24 hours from slot machines or robbers.", new TimeSpan(1, 0, 0, 0)),
                new Item("Double Day", 8000, "Get double the won money in any economy command for 24 hours!", new TimeSpan(1, 0, 0, 0)),
                //new Item("Pro Robber", 8000, "Get a higher chance for a successful robbing."),

            };
            }
            [Command("")]
            [Summary("Shows the KillerBot Economy Shop menu.")]
            [Ratelimit(4, 1,Measure.Minutes ,RatelimitFlags.None)]
            public async Task ShowShop([Summary("The number of the page in the shop menu if any other pages exist.")]int page = 1)
            {
                const int ItemsPerPage = 6;
                // Calculate the highest accepted page number => amount of pages we need to be able to fit all users in them
                // (amount of users) / (how many to show per page + 1) results in +1 page more every time we exceed our usersPerPage  
                var lastPageNumber = 1 + (_items.Count / (ItemsPerPage + 1));
                if (page > lastPageNumber)
                {
                    await ReplyAsync($"There are not that many pages...\nPage {lastPageNumber} is the last one...");
                    return;
                }
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("KillerBot's Economy Shop: ");
                embed.WithFooter($"Page {page}/{lastPageNumber} | Note: The item's name is the one before (coins)");
                embed.WithDescription("Here you can buy items for cool rewards, just do [k!shop buy {item name}](https://cdn.discordapp.com/attachments/497373849042812930/620597247515688980/KB_How_To_Shop.jpg)");
                embed.WithColor(new Color(0, 255, 247));
                var application = await Context.Client.GetApplicationInfoAsync();
                embed.WithThumbnailUrl("https://cdn.discordapp.com/attachments/497373849042812930/620076029104357412/output-onlinepngtools_8.png");
                int endIndex = page * ItemsPerPage;
                int startIndex = endIndex - ItemsPerPage;
                for (int i = startIndex; i < endIndex && i < _items.Count; i++)
                {
                    embed.AddField($"{_items[i].name} ({_items[i].price} coins)", $"{_items[i].description}", false);
                }
                await ReplyAsync("", false, embed.Build());
            }

            [Command("buy")]
            [Summary("Buy a specific item in the KillerBot Economy Shop.")]
            [Ratelimit(7, 1, Measure.Minutes, RatelimitFlags.None)]
            public async Task Buy([Remainder] [Summary("The name of the item you want to buy from the shop (k!shop)")] string item = null)
            {
                if (item == null)
                {
                    await ReplyAsync("Please specify the item you need to buy. You can find the list of items by doing `k!shop`");
                    return;
                }

                var account = _globalUserAccounts.GetById(Context.User.Id);
                var item2 = item.ToUpper();
                Item i = _items.Find((a) => a.name.ToUpper() == item2);
                
                if (i == null)
                {
                    await Context.Channel.SendMessageAsync($"<:KBfail:580129304592252995> Sorry but i couldn't find `{item}` in the shop. Make sure you type the item's name correctly from `k!shop`. \n \n**How to buy items:** https://cdn.discordapp.com/attachments/497373849042812930/620597247515688980/KB_How_To_Shop.jpg");
                    return;
                }

                else if (account.Coins < i.price)
                {
                    await ReplyAsync($"<:KBfail:580129304592252995> The item's price is **{i.price} coins** and you only have **{account.Coins} coin(s)** in your wallet! (You need {i.price - account.Coins} coins more)");
                    return;
                }


                for (int it = 0; it < account.Bought_Items.Count; it++)
                {
                    bool item_found = false;

                    if (account.Bought_Items[it].name == i.name && DateTime.UtcNow - account.Bought_Items[it].Date <= account.Bought_Items[it].Duration)
                    {
                        await ReplyAsync("You already have this item in your inventory.");
                        item_found = true;
                        return;
                    }
                    if (account.Bought_Items[it].Duration.CompareTo(new TimeSpan(long.MaxValue)) < 0)
                    {
                        if (DateTime.UtcNow - account.Bought_Items[it].Date > account.Bought_Items[it].Duration && item_found != true)
                        {
                            account.Bought_Items.Remove(account.Bought_Items[it]);

                        }
                    }
                    continue;
                }
                account.Bought_Items.Add(new UserItem(i));
                account.Coins -= i.price;
                _globalUserAccounts.SaveAccounts(Context.User.Id);
                await ReplyAsync($"<a:KBtick:580851374070431774> You succesfully bought **{i.name}** for **{i.price} coins** from the KillerBot Economy Shop!");
            }
            

            [Command("item-info"), Alias("item", "iinfo", "iteminfo")]
            [Summary("Sends you all the info you need about a specified item from the economy shop.")]
            [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.None)]
            public async Task iteminfo([Remainder] [Summary("The name of the item you want info about.")] string item = null)
            {
                if (item == null)
                {
                    await ReplyAsync("Please specify the item you need to know information about. You can find the list of items by doing `k!shop`");
                    return;
                }
                var item2 = item.ToUpper();
                Item i = _items.Find((a) => a.name.ToUpper() == item2);
                
                if (i == null)
                {
                    await Context.Channel.SendMessageAsync($"<:KBfail:580129304592252995> Sorry but i couldn't find `{item}` in the shop. Make sure you type the item's name correctly from `k!shop`. \n \n**Example:** https://cdn.discordapp.com/attachments/596436675383656459/608481133243400202/unknown.png");
                    return;
                }

                var duration = "";
                if (i.Duration == new TimeSpan(long.MaxValue))
                    duration = "Permanent";
                else
                {
                    duration = $"{i.Duration.Days}D , {i.Duration.Hours}h , {i.Duration.Minutes}m";
                }
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithAuthor("=== Item Information ===", "https://upload.wikimedia.org/wikipedia/en/thumb/3/35/Information_icon.svg/1024px-Information_icon.svg.png")
                    .AddField($"Item name: {i.name}", $"**Description:** {i.description} \n**Price:** {i.price} coins \n**Duration:** {duration}");
                await ReplyAsync("", false, emb.Build());
            }
        } 
    }
}

