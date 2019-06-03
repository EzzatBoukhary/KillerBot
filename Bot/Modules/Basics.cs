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
using Bot.Helpers;

namespace Bot.Modules
{
    public class Basics : ModuleBase<MiunieCommandContext>
    {
        [Command("Hello"),Summary("Hey!")]
        [Cooldown(2)]
        public async Task SayHello()
        {
            await ReplyAsync("Hey!");
        }
        [Command("invite"), Alias("inv"),Summary("Sends an invite link for the bot.")]
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
        [Cooldown(30)]
        [Summary("Submit feedback directly to KillerBot HQ.")]
        [Remarks("Usage: k!feedback {feedback}")]
        public async Task FeedbackAsync([Remainder] string feedback = null)
        {
            if (feedback == null)
                throw new ArgumentException("Please write a feedback to send.");
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
            .WithFooter(new EmbedFooterBuilder().WithText($"Feedback from: {Context.User.Username}#{Context.User.Discriminator} | Guild: {Context.Guild.Name}"))
            .WithAuthor($"{Context.User}", Context.User.GetAvatarUrl());

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

            //good ping
            var Good = new EmbedBuilder()
                .WithTitle("Ping results")
                .WithDescription($"**Gateway Latency:** {Gateway} ms" +
                $"\n**Response Latency:** {sw.ElapsedMilliseconds} ms")
                .WithColor(Color.Green);

            //medium ping
            var meh = new EmbedBuilder()
                .WithTitle("Ping results")
                .WithDescription($"**Gateway Latency:** {Gateway} ms" +
                $"\n**Response Latency:** {sw.ElapsedMilliseconds} ms")
                .WithColor(Color.Orange);

            //bad ping
            var bad = new EmbedBuilder()
                .WithTitle("Ping results")
                .WithDescription($"**Gateway Latency:** {Gateway} ms" +
                $"\n**Response Latency:** {sw.ElapsedMilliseconds} ms")
                .WithColor(new Color(255, 0, 0));
            if (Gateway > 800)
            {
                await ReplyAsync("", false, bad.Build());
            }
            else if (Gateway > 100)
                await ReplyAsync("", false, meh.Build());
            else if (Gateway < 100)
            {
                await ReplyAsync("", false, Good.Build());
            }
        } 

      
        [Command("changelog")]
        [Cooldown(5)]
        [Summary("Change log for the current version of the bot")]
        public async Task changes()
        {

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Green);
            embed.WithTitle("== Changelog ==");
            embed.Description = " **== Minor Release ==** `1.2.0` <:KBupdate:580129240889163787> \n \n**[Added]** \n \n<:KBdot:580470791251034123> Warn System \nCommands: `warn`,`warnings`,`removewarn`. More will come on later updates.  \n \n<:KBdot:580470791251034123> Added <userdiscriminator> as an option for the join-leave messages. \n \n<:KBdot:580470791251034123> Bot owner command `add-money` \n \n<:KBdot:580470791251034123> Support server-related: Added logging for high pings in `connection-logs` channel. \n \n**[Changed-Fixed]** \n \n<:KBdot:580470791251034123> Fixed a bug making the announcements channel to be unset when bot restarts. \n \n<:KBdot:580470791251034123> Made `ping` command embed color related to the ping. \n \n<:KBdot:580470791251034123> Small changes to `k!money` command's replies. \n \n<:KBdot:580470791251034123> Small changes to some economy commands and `help (module)` command.";
            embed.WithFooter(x =>

            {

                x.WithText("Last updated: 02/06/2019 10:51 PM GMT");



            });


            await ReplyAsync("", false, embed.Build());
        }


        List<string> queueList = new List<string>();
        Random rand = new Random();
       
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
        [Cooldown(3)]
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

        [Command("mock"), Summary("rEpEaTs yOuR tExT lIkE tHiS.")]
        public async Task Mock([Remainder] string input = "")
        {
            string result = "";
            if (input == "")
            {
                var message = await Context.Channel.GetMessagesAsync(2).FlattenAsync();
                input = message.Last().Content;
            }

            for (int i = 0; i < input.Length; i++)
            {
                if (i % 2 == 0)
                {
                    result += char.ToLower(input[i]);
                }
                else
                {
                    result += char.ToUpper(input[i]);
                }
            }
            await ReplyAsync(result);
        }

        [Command("userinfo")]
        [Cooldown(3)]
        [Summary("Gets information about the specified user")]
        public async Task UserInfo([Remainder] IGuildUser user = null)
        {
            user = user ?? (SocketGuildUser)Context.User;
            EmbedBuilder emb = new EmbedBuilder();

            string userRoles = DiscordHelpers.GetListOfUsersRoles(user);

            // Find user's highest role so the embed will be coloured with the role's colour

            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(user);

            if (highestRole != null)
                emb.Color = highestRole.Color;

            // Display if the user is a bot or a webhook
            EmbedAuthorBuilder author = new EmbedAuthorBuilder();
            author.Name = user.Username;
            if (user.IsBot)
                author.Name += "(Bot)";
            else if (user.IsWebhook)
                author.Name += " (Webhook)";
           

            emb.Author = author;

            // If the user has a default avatar
            if (string.IsNullOrEmpty(user.AvatarId))
                emb.ThumbnailUrl = $"https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png";
            else
                emb.ThumbnailUrl = $"https://cdn.discordapp.com/avatars/{user.Id}/{user.AvatarId}.png";

            EmbedFooterBuilder footer = new EmbedFooterBuilder();
            footer.Text = $"User info requested by {Context.User.Username}";
            // If the user has a default avatar
            if (string.IsNullOrEmpty(Context.User.AvatarId))
                footer.IconUrl = $"https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png";
            else
                footer.IconUrl = $"https://cdn.discordapp.com/avatars/{Context.User.Id}/{Context.User.AvatarId}.png";
            emb.Footer = footer;

            emb.Description = $"User information for {user.Username}#{user.Discriminator} | {user.Id}";

            emb.AddField("Created account at", user.CreatedAt.ToString());

            emb.AddField("Joined server at", ((DateTimeOffset)user.JoinedAt).ToString());

            // Display the list of all of user's roles
            if (string.IsNullOrEmpty(userRoles) == false)
                emb.AddField("Role(s)", userRoles);

            // Display the list of all of user's permissions
            string userPermissions = GetUserPermissions(user);

            if (string.IsNullOrEmpty(userPermissions) == false)
                emb.AddField("Permissions", userPermissions);
            

            emb.AddField("Status", user.Status == UserStatus.DoNotDisturb ? "Do Not Disturb" : user.Status.ToString());

            await ReplyAsync("", false, emb.Build());
        }

        /// <summary>
        /// Get a list of user's permissions in a nicely formatted string.
        /// </summary>
        private string GetUserPermissions(IGuildUser user)
        {
            string permissions = "";

            if (Context.Guild.OwnerId == user.Id)
            {
                permissions += "Owner";
                return permissions;
            }

            if (user.GuildPermissions.Administrator)
            {
                permissions += "Administrator";
                return permissions;
            }

            if (user.GuildPermissions.BanMembers)
                permissions += "Ban Memebers, ";

            if (user.GuildPermissions.DeafenMembers)
                permissions += "Deafen Members, ";

            if (user.GuildPermissions.KickMembers)
                permissions += "Kick Members, ";

            if (user.GuildPermissions.ManageChannels)
                permissions += "Manage Channels, ";

            if (user.GuildPermissions.ManageEmojis)
                permissions += "Manage Emojis, ";

            if (user.GuildPermissions.ManageGuild)
                permissions += "Manage Guild, ";

            if (user.GuildPermissions.ManageMessages)
                permissions += "Manage Messages, ";

            if (user.GuildPermissions.ManageNicknames)
                permissions += "Manage Nicknames, ";

            if (user.GuildPermissions.ManageRoles)
                permissions += "Manage Roles, ";

            if (user.GuildPermissions.ManageWebhooks)
                permissions += "Manage Webhooks, ";

            if (user.GuildPermissions.MentionEveryone)
                permissions += "Mention Everyone, ";

            if (user.GuildPermissions.MoveMembers)
                permissions += "Move Members, ";

            if (user.GuildPermissions.MuteMembers)
                permissions += "Mute Members, ";

            return permissions.Remove(permissions.Length - 2);
        }
        [Command("report"), Alias("bug", "bugreport", "reportbug")]
        [Cooldown(10)]
        [Summary("Send a report about a bug to the bot owner. Spam/troll is not tolerated.")]
        public async Task BugReport([Remainder] string report)
        {

            var application = await Context.Client.GetApplicationInfoAsync();
            var message = await application.Owner.GetOrCreateDMChannelAsync();

            var embed = new EmbedBuilder()
            {
                Color = (Color.Red)
            };

            embed.Description = $"{report}";
            embed.WithFooter(new EmbedFooterBuilder().WithText($"Message from: {Context.User.Username}#{Context.User.Discriminator} | Guild: {Context.Guild.Name}"));
            await message.SendMessageAsync("", false, embed.Build());
            embed.Description = $"You have sent a message to the Bot owner (Panda#8822). He will read the message soon.";

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }


        [Command("UserCount")]
        [Cooldown(3)]
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





