using Bot.Preconditions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Bot.Helpers;
using System.Globalization;
using Bot.Extensions;
using Bot.Features.Lists;
using Discord.WebSocket;
using Discord.Rest;
using System.Diagnostics;
using System.Data;

namespace Bot.Modules
{
    public class auctions : ModuleBase<MiunieCommandContext>
    {
        private CommandService _service;
        private readonly ListManager _listManager;
        private int _fieldRange = 10;

        public auctions(CommandService service, ListManager listManager)
        {
            _service = service;
            _listManager = listManager;
        }

        static string auctionCheck = "";
        static ulong currentAuction = 0;
        static int hightBid;
        static SocketGuildUser highBidder = null;
        static string currentItem;

        [Command("auction")]
        [Summary("Start an auction with this command, however make sure to put the parameters right. `Example: @KillerBot auction 1000 4 Gems <info, optional>` ")]
        public async Task AuctionAsync(int startingBid, int amount, string item, [Remainder]string info = null)

        {

            if (auctionCheck == "" || auctionCheck == "over") auctionCheck = "live";
            else if (auctionCheck == "live")
            {
                var messageToDel = await ReplyAsync("Auction already started, only one at a time please.");
                DelayDeleteMessage(Context.Message, 10);
                DelayDeleteMessage(messageToDel, 10);
                return;
            }
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = $"Auction for {item} started {DateTime.UtcNow} UTC";
                x.Value = $"{amount} x {item} is up for auction with starting bid of {startingBid}\nType `bid [amount]` to bid";
            });
            var message = await ReplyAsync("", embed: embed.Build());
            hightBid = startingBid - 1;
            currentItem = item;
            currentAuction = message.Id;
        }

        private void DelayDeleteMessage(IUserMessage message, int v)
        {

        }


        [Command("auctionEnd")]
        [Summary("End an ongoing auction.")]
        public async Task AuctionOverAsync()
        {
            if (highBidder == null)
            {
                await Context.Channel.SendMessageAsync("The auction has ended with 0 bids.");
                auctionCheck = "over";
                hightBid = 0;
                currentItem = null;
                currentAuction = 0;
                return;
            }

            var embed = new EmbedBuilder();
            embed.Title = $"{highBidder} won the auction for {currentItem}";
            embed.AddField(x =>
            {
                x.Name = $"Auction ended at {DateTime.UtcNow}";
                x.Value = $"Once you pay {hightBid}, We will arrange payment and delivery of {currentItem} soon after.  Congratz! :tada: ";
                x.IsInline = false;
            });

            auctionCheck = "over";
            highBidder = null;
            hightBid = 0;
            currentItem = null;
            currentAuction = 0;
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }


        [Command("auctionCheck")]
        public async Task AuctionCheckAsyc()
        {
            var DM = await Context.User.GetOrCreateDMChannelAsync();
            if (auctionCheck == "over" || auctionCheck == "")
            {
                var message = await DM.SendMessageAsync("Sorry there isn't an auction at this time.");
                DelayDeleteMessage(Context.Message, 10);
                DelayDeleteMessage(message, 10);
                return;
            }
            var auctionStatus = Context.Channel.GetMessageAsync(currentAuction);

            var embed = auctionStatus.Result.Embeds.FirstOrDefault() as Embed;
            await DM.SendMessageAsync("", false, embed);
            DelayDeleteMessage(Context.Message, 10);
            await ReplyAsync("Check your DMs.");
        }


        [Command("bid")]
        public async Task BidAsync(string amount)
        {
            int bid;
            if (auctionCheck != "live")
            {
                var message = await ReplyAsync("Theres no auction at this time, ask someone to start one.");
                DelayDeleteMessage(Context.Message, 10);
                DelayDeleteMessage(message, 10);

                return;
            }
            else
            {
                try
                {
                    bid = Math.Abs(int.Parse(amount));
                }
                catch
                {
                    var message = await ReplyAsync("Thats not a valid bid, I'm not stupid.");
                    DelayDeleteMessage(Context.Message, 10);
                    DelayDeleteMessage(message, 10);

                    return;
                }
                if (bid <= hightBid)
                {
                    var message = await ReplyAsync("Your bid it too low, increase it and try again.");
                    DelayDeleteMessage(Context.Message, 10);
                    DelayDeleteMessage(message, 10);
                    return;
                }

                hightBid = bid;
                highBidder = Context.User as SocketGuildUser;

                await UpDateHighBidder(Context.Message as SocketUserMessage, bid);

                var message2 = await ReplyAsync($"The current high bidder is {highBidder.Mention} with a bid of {hightBid} :moneybag: ");
                DelayDeleteMessage(Context.Message, 10);
                DelayDeleteMessage(message2, 10);
            }
        }


        private async Task UpDateHighBidder(SocketUserMessage messageDetails, int bid)
        {
            var exactMessage = await messageDetails.Channel.GetMessageAsync(currentAuction) as IUserMessage;
            var embed2 = new EmbedBuilder();
            var oldField = exactMessage.Embeds.FirstOrDefault().Fields.FirstOrDefault();
            embed2.AddField(x =>
            {
                x.Name = oldField.Name;
                x.Value = oldField.Value;
                x.IsInline = oldField.Inline;
            });

            embed2.AddField(x =>
            {
                x.Name = "New High Bid!";
                x.IsInline = false;
                x.Value = $"{currentItem} highest bid is {bid} by {highBidder}";
            });
        }
    }
}