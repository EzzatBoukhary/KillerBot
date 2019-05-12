using Bot.Preconditions;
using Discord.Commands;
using System.Threading.Tasks;
using Bot.Extensions;
using Discord;
using System.Diagnostics;
using Discord.WebSocket;
using System.Linq;
using System;
using System.Text;
using System.Collections.Generic;
using Gommon;

namespace Bot.Modules
{
    public class Basics : ModuleBase<MiunieCommandContext>
    {
        [Command("Hello")]
        [Cooldown(5)]
        public async Task SayHello()
        {
            await ReplyAsync("Hey!");
        }
        [Command("invite"), Alias("inv")]
        [Cooldown(5)]
        public async Task InviteBot()
        {
            await ReplyAsync("Add the bot to any server you want to by clicking this link: https://goo.gl/h3xHqU (shortened)");
        }
        [Command("server"), Alias("botserver" , "support")]
        [Cooldown(5)]
        [Summary("Link to the KillerBot HQ discord server.")]
        public async Task ServerLink()
        {
            await ReplyAsync("Permanent invite link to the server for the bot where you can get help and support: https://discord.gg/DNqAShq");
        }

        [Command("Feedback"), Alias("Fb")]
        [Summary("Submit feedback directly to KillerBot HQ.")]
        [Remarks("Usage: |prefix |feedback {feedback}")]
        public async Task FeedbackAsync([Remainder] string feedback)
        {
            var embed = new EmbedBuilder()
            {
                Color = (Color.Green)
            };
            embed.WithDescription($"Feedback sent! Message: ```{feedback}```");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            var embed2 = new EmbedBuilder()
            {
                Color = (Color.LightGrey)
            };
            embed2.WithDescription($"```{feedback}```")
                .WithTitle($"Feedback from {Context.User}");
            var channel = Context.Client.GetChannel(550073251641032717) as SocketTextChannel;
            if (channel is null)
            {
                await ReplyAsync("Couldn't find channel");
                return;
            }
            else
            {
                channel.SendMessageAsync("",false, embed2.Build());
            }
            

        }
        

        
       [Command("Ping", RunMode = RunMode.Async)]
        [Remarks("Returns Gateway latency, Response latency and Delta (response - gateway).")]
        public async Task Ping()
        {
            var sw = Stopwatch.StartNew();
            var client = Context.Client as DiscordSocketClient;
            var Gateway = client.Latency;
            var emb = new EmbedBuilder()
                .WithTitle("Ping results")
                .WithDescription($"**Gateway Latency:** {Gateway} ms" +
                $"\n**Response Latency:** {sw.ElapsedMilliseconds} ms")
                .WithColor(Color.Green);
            await ReplyAsync("", false, emb.Build());
        } 

        /* [Command("PingNew")]
        [Summary("Show the Gateway latency to Discord.")]
        [Remarks("Usage: |prefix|ping")]
        public async Task PingAsync()
        {
            var embed = new EmbedBuilder()
            {
                Color = (Color.Green)
            };
            embed.WithDescription("Pinging...");
            var sw = new Stopwatch();
            sw.Start();
            var msg = await Context.Channel.SendMessageAsync("", false, embed.Build());
            sw.Stop();
            await msg.ModifyAsync(x =>
            {
                embed.WithDescription(
                    $"**Ping**: {sw.ElapsedMilliseconds}ms \n" +
                    $"**API**: {Context.Client.Latency}ms");
                x.Embed = embed.Build();
            });
        } */

        [Command("changelog")]
        [Summary("Change log for the current version of the bot")]
        public async Task changes()
        {

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Green);
            embed.Description = " == Changelog == \n`0.9.0` - **Changed/Fixed** \n \n•Added reasons in audit log for kick and ban commands. \n \n•The bot now DMs the person kicked/banned a message informing them that they got kicked/banned from that server with the reason. \n \n•Ban now has 'pruned days' feature. However, its required to specify a number, so just put 0 if you don't want to prune. \nUsage `ban @User#0000 {Number of days to prune the user's messages} Reason` \n \n•Small changes in the reply messages of unban & mute commands. ";
            embed.WithFooter(x =>

            {

                x.WithText("Last updated: 12/05/2019 9:03 PM GMT");



            });


            await ReplyAsync("", false, embed.Build());
        }


        List<string> queueList = new List<string>();
        Random rand = new Random();
        string[] PredictionsTexts = new string[]
      {
        "It is very unlikely.",
        "I don't think so...",
        "Yes!",
        "I don't know",
        "No." ,
        "Of course!",
        "Ask me later." ,
        "Never!" ,
        "Definitely.",
        "You may rely on it." ,
        "My dad said no." ,
        "The whole country agreed!" ,
         "Nah."
      };
        Random rnd = new Random();
        [Command("8ball")]
        [Summary("Gives a prediction")]
        public async Task EightBall([Remainder] string input)
        {
            int randomIndex = rand.Next(PredictionsTexts.Length);
            string text = PredictionsTexts[randomIndex];
            await ReplyAsync(Context.User.Mention + ", " + text);
        }

        string[] FlipCoin = new string[]
       {
      "Heads!" ,
      "Tails!"

       };
        private ulong channelIDHere;

        [Command("flip"), Alias("flipcoin")] 
        [Summary("Flips a coin")]
        public async Task Flip()
        {
            var msgToEdit = await Context.Channel.SendMessageAsync("Fliping....");
            int randomCoinFlip = rand.Next(FlipCoin.Length);
            string coinResultToPost = FlipCoin[randomCoinFlip];
            // await ReplyAsync($"{Context.User.Mention} Result: {coinResultToPost}");
            await msgToEdit.ModifyAsync(x => { x.Content = $"{Context.User.Mention} Result: {coinResultToPost}"; });
        }
        [Command("Choose")]
        [Summary("Choose an item from a list separated by |.")]
        [Remarks("Usage: prefix choose {option1|option2|option3|...}   SHOULD have `|` between every option.")]
        public async Task ChooseAsync([Remainder] string options)
        {
            var opts = options.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var embed = new EmbedBuilder()
            {
                Color = Color.Blue
            };
            if (opts.Length <= 1)
            {
                await ReplyAsync("Must have more than 1 option for I to choose from.");
                
            }
            else
            {
                embed.WithDescription($"I choose `{opts.Random()}`.");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            
        }
    

    [Command("Avatar")]
        [Summary("Shows the mentioned user's avatar, or yours if no one is mentioned.")]
        [Remarks("Usage: `avatar [@user]`")]
        public async Task AvatarAsync(SocketGuildUser user = null)
        {
            var u = user ?? Context.User;
            var embed = new EmbedBuilder();
            
           embed .WithTitle($"Avatar: {user}")
            .WithImageUrl(u.GetAvatarUrl(ImageFormat.Auto, 1024))
             .WithFooter(x =>

             {

                 x.WithText($"Requested by: {Context.User}");

                 x.WithIconUrl(Context.User.GetAvatarUrl());

             });
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("serverinfo")]
        [Alias("sinfo","aboutserver")]
        [Summary("Information about the server the command was done in.")]
        public async Task serverinfo()
        {
            var gld = Context.Guild as SocketGuild;
            var embed = new EmbedBuilder();
            embed.Color = new Color(154, 59, 226);
            if (!string.IsNullOrWhiteSpace(gld.IconId))
                embed.ThumbnailUrl = gld.IconUrl;
            var o = gld.Owner.Username;
            var v = gld.VoiceRegionId;
            var vc = gld.VoiceChannels.Count;
            var tc = gld.TextChannels.Count;
            var c = gld.CreatedAt;
            var r = gld.Roles.Count;
            var id = gld.Id;
            var con = Context.Client.ConnectionState;
            var lev = gld.VerificationLevel;
            var us = gld.Users.Count;
            embed.Title = $"{gld.Name} server information";
            embed.Description = $"**Server name**: {gld.Name} \n \n **Created at**: {c} \n \n **Server owner**: {o} \n \n **Server region**: {v} \n \n **Voice channels**: {vc} \n \n **Text channels**: {tc} \n \n **Server ID**: {id} \n \n **Number of users** : {us} \n \n **Number of roles** : {r} \n \n **Connection state**: {con} \n \n **Verification level**: {lev}";
            await ReplyAsync("", false, embed.Build());
        }

        [Command("userinfo")]
        [Alias("user", "whois", "user profile")]
        [Summary("shows all the info of a user")]
        public async Task user(IGuildUser user)
        {
            {
                var application = await Context.Client.GetApplicationInfoAsync();
                var thumbnailurl = user.GetAvatarUrl();
                var date = $"{user.CreatedAt.Month}/{user.CreatedAt.Day}/{user.CreatedAt.Year}";
                var auth = new EmbedAuthorBuilder()

                {

                    Name = user.Username,
                    IconUrl = thumbnailurl,

                };

                var embed = new EmbedBuilder()

                {
                    ThumbnailUrl = user.GetAvatarUrl(),

                    Color = (Color.Gold),
                    Author = auth
                };

                var us = user as SocketGuildUser;
                var bot = user.IsBot;
                var D = us.Username;
                var A = us.Discriminator;
                var T = us.Id;
                var S = date;
                var C = us.Status;
                var CC = us.JoinedAt;
                embed.Title = $"**{us.Username}** Information";
                embed.Description = $"**Username**: {D}\n \n**Discriminator**: {A}\n \n **User ID**: {T}\n \n **Created at**: {S}\n \n**Current Status**: {C}\n \n  **Joined server at**: {CC}\n \n **Is bot**: {bot}";
                await ReplyAsync("", false, embed.Build());
            }
        }



        [Command("UserCount")]

        [Alias("UC")]

        [Remarks("User Count for the current server")]

        [RequireContext(ContextType.Guild)]

        public async Task Ucount()

        {

            if ((Context.Guild as SocketGuild) != null)

            {

                var botlist = ((SocketGuild)Context.Guild).Users.Count(x => x.IsBot);

                var mem = ((SocketGuild)Context.Guild).MemberCount;

                var guildusers = mem - botlist;



                var embed = new EmbedBuilder()

                    .WithTitle($"User Count for {Context.Guild.Name}")

                    .AddField(":busts_in_silhouette: Total Members", mem)

                    .AddField(":robot: Total Bots", botlist)

                    .AddField(":man_in_tuxedo: Total Users", guildusers)

                    .AddField(":newspaper2: Total Channels", ((SocketGuild)Context.Guild).Channels.Count)

                    .AddField(":microphone: Text/Voice Channels", $"{((SocketGuild)Context.Guild).TextChannels.Count}/{((SocketGuild)Context.Guild).VoiceChannels.Count}")

                    .AddField(":spy: Role Count", ((SocketGuild)Context.Guild).Roles.Count)


                    .WithFooter(x =>

                    {

                        x.WithText("KillerBot   ");

                        x.WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl());

                    });



                await ReplyAsync("", false, embed.Build());

            }
        }
    }
}





