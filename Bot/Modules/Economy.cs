using System;
 using Bot.Features.Economy;
using Bot.Features.GlobalAccounts;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using Bot.Extensions;
using static Bot.Global;
using Bot.Features.Economy;
using Bot.Preconditions;
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
                var accounts =_dailyMiunies.GetDaily(Context.User.Id);
                await ReplyAsync($"{Context.User.Mention}, you just claimed your {Constants.DailyMuiniesGain} daily coins!");
            }
            catch (InvalidOperationException e)
            {
                var timeSpanString = string.Format("{0:%h} hours {0:%m} minutes {0:%s} seconds", new TimeSpan(24,0,0).Subtract((TimeSpan)e.Data["sinceLastDaily"]));
                await ReplyAsync($"You already got your daily, {Context.User.Mention}.\nCome back in {timeSpanString}.");
            }
        }

        [Command("money"), Remarks("Shows how much money you have")]
        [Alias("Cash", "Money")]
        public async Task CheckMiunies()
        {
            var account = _globalUserAccounts.GetById(Context.User.Id);
            await ReplyAsync(GetCoinsReport(account.Coins, Context.User.Mention));
        }

        [Command("money"), Remarks("Shows how much money the mentioned user has")]
        [Alias("Cash", "Money")]
        public async Task CheckMiuniesOther(IGuildUser target)
        {
            var account = _globalUserAccounts.GetById(target.Id);
            await ReplyAsync(GetCoinsReport(account.Coins, target.Mention));
        }

        [Command("Leaderboard"), Remarks("Shows a user list of the sorted by money. Pageable to see lower ranked users.")]
        [Alias("Top", "Top10", "Richest")]
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
            var lastPageNumber = 1 + (accounts.Count / (usersPerPage+1));
            if (page > lastPageNumber)
            {
                await ReplyAsync($"There are not that many pages...\nPage {lastPageNumber} is the last one...");
                return;
            }
            // Sort the accounts descending by Minuies
            var ordered = accounts.OrderByDescending(acc => acc.Coins).ToList();

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
                embB.AddField(contentName, $"{account.Coins} Coins", true);
            }

            await ReplyAsync("", false, embB.Build());
        }

        [Command("Transfer")]
        [Remarks("Transferrs specified amount of your coins to the mentioned person.")]
        [Alias("Give", "Gift")]
        public async Task TransferMinuies(IGuildUser target, ulong amount)
        {
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

        public string GetCoinsReport(ulong coins, string mention)
        {
            return $"{mention} has **{coins} coins**! {GetMiuniesCountReaction(coins, mention)}";
        }

        [Command("newslot"), Remarks("Creates a new slot machine if you feel the current one is unlucky")]
        [Alias("newslots")]
        public async Task NewSlot(int amount = 0)
        {
            Global.Slot = new Slot(amount);
            await ReplyAsync("<a:KBtick:580851374070431774> A new slot machine got generated! Good luck!");
        }

        [Command("slots"), Remarks("Play the slots! Win or lose some coins!")]
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

        [Command("showslots"), Remarks("Shows the configuration of the current slot machine")]
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
            var result = (ulong)Global.Rng.Next(Constants.WorkRewardMinMax.Item1, Constants.WorkRewardMinMax.Item2 + 1);
                var accounts = _globalUserAccounts.GetById(Context.User.Id);
                account.Coins += result;
                _globalUserAccounts.SaveAccounts();
                emb.WithColor(Color.Green);
                emb.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}",Context.User.GetAvatarUrl());
                emb.WithCurrentTimestamp();
                emb.WithDescription($"You work and recieve **{result} coins** for your hard work. <a:KBtick:580851374070431774> ");
                await ReplyAsync("", false, emb.Build());
            
        } 
       
    }
} 
