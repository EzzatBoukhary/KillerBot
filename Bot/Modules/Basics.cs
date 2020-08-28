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
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using Bot.Entities;
using Bot.Features.GlobalAccounts;
using System.Text.RegularExpressions;

namespace Bot.Modules
{
    public class Basics : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        private readonly Logger _logger;

        public Basics(GlobalUserAccounts globalUserAccounts, GlobalGuildAccounts globalGuildAccounts, Logger logger)
        {
            _globalGuildAccounts = globalGuildAccounts;
            _logger = logger;
        }
        [Command("Hello"),Summary("Hey!")]
        [Cooldown(2)]
        public async Task SayHello()
        {
            await ReplyAsync("Hey!");
        }
        [Command("invite"), Alias("inv", "add", "addbot", "link"),Summary("Sends an invite link for the bot.")]
        [Cooldown(5)]
        public async Task InviteBot()
        {
            try
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTitle("Invite KillerBot:")
                    .WithDescription("**Add KillerBot:** [Click Here!](https://discord.com/oauth2/authorize?client_id=263753726324375572&scope=bot&permissions=1480615958) \n**Need help?** [Support Server](https://discord.gg/DNqAShq) \n**Want to support KillerBot?** [Donate!](https://www.patreon.com/KillerBot) \n \n*Note: You need `Manage Server` or `Administrator` permission in a server in order to be able to add the bot to it.*");
                await ReplyAsync("", false, emb.Build());
            }
            catch
            {
                await ReplyAsync("**Add KillerBot:** https://discord.com/oauth2/authorize?client_id=263753726324375572&scope=bot&permissions=1480615958 \n \n*Note: You need `Manage Server` or `Administrator` permission in a server in order to be able to add the bot to a server.*");
            }
        }
        [Command("donate")]
        [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task Donate()
        {
            EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("Support KillerBot!")
                    .WithDescription("Do you love KillerBot and want to support it? Feel free to donate!")
                    .AddField("Why?", "By donating to KillerBot you: \n- **Help it to continue to exist.** \n- **Show your support to KillerBot.** \n- **Help with bringing MORE FEATURES!** \n- **Get amazing rewards within KillerBot and the support server.**")
                    .AddField("How?", "All you have to do is go to KillerBot's patreon page by [Clicking Here](https://www.patreon.com/KillerBot) and donate! Make sure to join the [Support Server](https://discord.gg/DNqAShq) though so you can get the rewards! :hugging_face:")
                    .AddField("Thanks!", "We appreciate every single one of you whether you donate or not and thanks for supporting KillerBot every step on the way! :heart:")
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
            await ReplyAsync("", false, emb.Build());
        }
        [Command("server"), Alias("botserver" , "support")]
        [Cooldown(5)]
        [Summary("Link to the KillerBot HQ discord server.")]
        public async Task ServerLink()
        {
            await ReplyAsync("Permanent invite link to the server for the bot where you can get help and support: https://discord.gg/DNqAShq");
        }

        [Command("Feedback"), Alias("Fb", "suggest")]
        [Cooldown(25)]
        [Summary("Submit feedback directly to KillerBot HQ.")]
        [Remarks("Usage: k!feedback {feedback}")]
        public async Task FeedbackAsync([Summary("Your feedback you want to send about KillerBot")][Remainder] string feedback = null)
        {
            if (feedback == null)
                throw new ArgumentException("Please write a feedback to send.");

            var guild = "";
            if (Context.Message.Channel is SocketDMChannel)
                guild = "[KillerBot DMs]";
            else
                guild = $"{Context.Guild.Name}";

            var embed = new EmbedBuilder()
            {
                Color = (Color.Green)
            };

            var embed2 = new EmbedBuilder()
            {
                Color = (Color.LightGrey)
            };
            if (feedback.Contains("discord.gg/"))
            {
                embed2.WithDescription($"[Message contained a server invite link]");
            }
            else
            {
                embed2.WithDescription($"{feedback}");
            }
            embed2.WithFooter(new EmbedFooterBuilder().WithText($"Feedback from: {Context.User.Username}#{Context.User.Discriminator} | Guild: {guild}"))
            .WithAuthor($"{Context.User}", Context.User.GetAvatarUrl());

            var channel = Context.Client.GetChannel(550073251641032717) as SocketTextChannel;
            if (channel is null)
            {
                await ReplyAsync("Error 404: Couldn't find channel. Please do `k!report k!feedback results in error 404.`");
                return;
            }
            else
            {
                embed.WithDescription($"Feedback sent! Message: ```{feedback}```");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                await channel.SendMessageAsync("",false, embed2.Build());
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
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        [Summary("Change log for the current version of the bot")]
        public async Task changes()
        {
            var dot = "<:KBdot:580470791251034123>";
            var embed = new EmbedBuilder();
            embed.WithColor(Color.Green);
            embed.WithTitle("== Changelog ==");
            embed.Description = $" **== Minor Release ==** `v1.12.0` <:KBupdate:580129240889163787> \n \n**[Added]** \n \n{dot} Added `k!setslowmode`, `k!setserverslowmode` and `k!embed` commands. \n \n**[Changed/Fixed]** \n \n{dot} Fixed a major bug in russian roulette that happens sometimes with 3+ players. \n \n{dot} Pinned messages are now ignored in the purge command. \n \n{dot} Fixed a bug with the deposit command. \n \n{dot} Removed reasons from the locking commands. \n \n{dot} Changes in the rob command. \n \n{dot} Minor changes to the google commands. \n \n{dot} Improvements to the reminder commands. \n \n{dot} Fixed a bug in the mute, addrole, removerole commands. \n \n{dot} Added audit log reasons to role changes in RoleByPhrase. \n \n{dot} Fixed some typos in the commands' info.";
            embed.WithFooter(x =>

            {

                x.WithText("Last updated: August 28th - 2020 08:25 PM GMT");



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
        [Remarks("Usage: k!choose option1|option2|option3|...   SHOULD have `|` between every option.")]
        public async Task ChooseAsync([Summary("The options you want the bot to choose from")] [Remainder] string options)
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
    [Command("status")]
        [Summary("Wondering about the bot's status? Join the support server and know more about it.")]
        [Remarks("Sends a link to the bot's support server with updates about the status of the bot and connection")]
        public async Task StatusAsync()
        {
            await ReplyAsync("Join the bot's support server to know all the information about the status of the bot and connection: https://discord.gg/DNqAShq");
        }

    [Command("Avatar"), Alias("av")]
        [Summary("Shows the mentioned user's avatar, or yours if no one is mentioned.")]
        [Remarks("Usage: `k!avatar [@user]`")]
        [Ratelimit(5, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task AvatarAsync([Remainder] SocketGuildUser user = null)
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
        [Alias("sinfo", "aboutserver")]
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireContext(ContextType.Guild)]
        [Summary("Shows information about the server this was sent in like creation date, amount of members and more.")]
        public async Task sinfo()
        {
            var channel = (ITextChannel)Context.Channel;
            var guild = Context.Guild as SocketGuild;
            var ownername = guild.GetUser(guild.OwnerId);
            var textchn = guild.TextChannels.Count();
            var voicechn = guild.VoiceChannels.Count();
            int daysOld = Context.Message.CreatedAt.Subtract(guild.CreatedAt).Days;
            string daysAgo = $"" + ((daysOld == 0) ? "today!" : (daysOld == 1) ? $"yesterday!" : $"{daysOld} days ago!");
            var mem = guild.MemberCount;
            var botlist = guild.Users.Count(x => x.IsBot);
            var guildusers = mem - botlist;
            string mem2 = $"<:online2:704873654722232361> **{guild.Users.Count(x => x.Status == UserStatus.Online)}** - <:away2:704873837883555902> **{guild.Users.Count(x => x.Status == UserStatus.Idle)}** - <:dnd2:704873772406014104> **{guild.Users.Count(x => x.Status == UserStatus.DoNotDisturb)}** - <:offline2:704873716114260009> **{guild.Users.Count(x => x.Status == UserStatus.Offline)}** (<:members:704874969083019354> **{mem}** - 👤 **{guildusers}** - <:botTag:704886277920784445> **{botlist}**)";

            var createdAt = new DateTime(2015, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(guild.Id >> 22);
            var features = string.Join(", ", guild.Features);
            if (string.IsNullOrWhiteSpace(features))
                features = "-";
            var dot = "<:KBdot:580470791251034123>";
            var embed = new EmbedBuilder();
                embed.WithAuthor("== SERVER INFORMATION ==");
                embed.WithColor(Color.Blue);
                embed.WithFooter($"Created {daysAgo}");
            embed.Title += $"{guild.Name} \n";
            if (guild.PremiumTier == PremiumTier.Tier1)
                embed.Title += $"<:KBBoost:731580776117698618>";
            else if (guild.PremiumTier == PremiumTier.Tier2)
                embed.Title += $"<:KBBoost:731580776117698618> <:KBBoost:731580776117698618>";
            else if (guild.PremiumTier == PremiumTier.Tier3)
                embed.Title += $"<:KBBoost:731580776117698618> <:KBBoost:731580776117698618> <:KBBoost:731580776117698618>";
            if (guild.PremiumSubscriptionCount > 0 && guild.PremiumSubscriptionCount > 1)
                embed.Title += $" ({guild.PremiumSubscriptionCount} boosts)";
            else
                embed.Title += $"({guild.PremiumSubscriptionCount} boost)";
            embed.Description = $"{dot} ID: **{guild.Id}**" +
                $"\n{dot} Owner: **{ownername.ToString()}**" +
                $"\n{dot} Creation: **{guild.CreatedAt.DateTime.ToLongDateString()} {guild.CreatedAt.DateTime.ToLongTimeString()} UTC**" +
                $"\n{dot} Region: **{guild.VoiceRegionId.ToString()}**" +
                $"\n \n{dot} Members: {mem2}" +
                $"\n{dot} Roles: **{(guild.Roles.Count - 1).ToString()}**" +
                $"\n{dot} Channels: Text: **{textchn.ToString()}** - Voice: **{voicechn.ToString()}** - Categories: **{guild.CategoryChannels.Count}**" +
                $"\n \n{dot} Verification: **{guild.VerificationLevel.ToString()}**";

            if (guild.ExplicitContentFilter.ToString() == "MembersWithoutRoles")
                embed.Description += $"\n{dot} Explicit Content Filter: **Scans messages from members without roles**";

            else if (guild.ExplicitContentFilter.ToString() == "Disabled")
                embed.Description += $"\n{dot} Explicit Content Filter: **No message scanning**";

            else if (guild.ExplicitContentFilter.ToString() == "AllMembers")
                embed.Description += $"\n{dot} Explicit Content Filter: **Scans messages from all members**";
        
            if (Uri.IsWellFormedUriString(guild.IconUrl, UriKind.Absolute))
                embed.WithThumbnailUrl(guild.IconUrl);
            if (features != "-")
            {
                embed.Description += $"\n{dot} Features: **{features}**";
            }

            if (guild.Emotes.Any())
            {
                embed.Description += $"\n{dot} Custom Emojis `({guild.Emotes.Count})`: ";
                embed.Description += (string.Join(" ", guild.Emotes
                        .Take(11)
                        .Select(e => $"{e.ToString()}")));
            }
            if (features.Contains("INVITE_SPLASH") && (guild.SplashUrl != null))
            {
                embed.Description += $"\n{dot} [Splash Icon:]({guild.SplashUrl})";
                embed.WithImageUrl(guild.SplashUrl);
            }
            var guild2 = _globalGuildAccounts.GetById(guild.Id);
            if (guild2.KBPremium == false)
                embed.Description += $"\n{dot} [KB Premium](https://www.patreon.com/KillerBot): {Constants.fail}";
            else if (guild2.KBPremium == true)
                embed.Description += $"\n{dot} [KB Premium](https://www.patreon.com/KillerBot): <a:KBPremium:706944892215230495>";
            await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
        }
        
        [Command("roleinfo"), Summary("Returns info about a role."),Alias("RI")]
        [Example("k!roleinfo Cool role")]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireContext(ContextType.Guild)]
        public async Task Role([Remainder, Summary("The role to return information about.")] string args = null)
        {
            if (args == null)
            {
                await ReplyAsync("<:KBfail:580129304592252995> Please put the name of the role you want information on.");
                return;
            }
            EmbedBuilder embed = new EmbedBuilder();
            StringBuilder builder = new StringBuilder();
            args = args.ToLower();
            var mentionedRole = Context.Guild.Roles.Where(r => r.Name.ToLower() == args).FirstOrDefault();
            if (mentionedRole == null)
            {
                mentionedRole = Context.Guild.Roles.Where(r => r.Name.ToLower().StartsWith(args)).FirstOrDefault();
            }
            IMessage message = Context.Message;
            IGuild guild = Context.Guild;
            IMessageChannel channel = Context.Channel;
            var members = Context.Guild.Users.Where(u => u.Roles.Contains(mentionedRole)).ToList();
            if (args == "everyone" || args == "Everyone")
                mentionedRole = guild.EveryoneRole as SocketRole;
            if (mentionedRole == null)
            {
                await Context.Channel.SendMessageAsync($"Unable to find role by the name of `{args}`.");
                return;
            }
            else
            {
                await channel.TriggerTypingAsync();
                string hex = '#' + mentionedRole.Color.ToString().Substring(1).PadLeft(6, '0');
                embed.Title = $"{mentionedRole.Name}";
                embed.Color = mentionedRole.Color;
                embed.Footer = new EmbedFooterBuilder()
                {
                    Text = $"Created on: { mentionedRole.CreatedAt.DateTime.ToLongDateString() } { mentionedRole.CreatedAt.DateTime.ToLongTimeString() } (By UTC)"
                };
                embed.Title = "=== ROLE INFORMATION ===";
                embed.Timestamp = DateTime.UtcNow;

                embed.AddField(x => {
                    x.Name = $"Name";
                    x.Value = $"{mentionedRole.Name}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"ID";
                    x.Value = $"{mentionedRole.Id}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"Hoisted";
                    x.Value = $"{mentionedRole.IsHoisted}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"Managed";
                    x.Value = $"{mentionedRole.IsManaged}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"Mentionable";
                    x.Value = $"{mentionedRole.IsMentionable}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"Position";
                    x.Value = $"{mentionedRole.Position}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = "Color";
                    x.Value = hex;
                    x.IsInline = true;
                });
                var membercount = Context.Guild.Users.Where(u => u.Roles.Contains(mentionedRole)).Count();
                string memberstext = "";
                foreach (var member in members)
                {
                    memberstext += $"<@{member.Id}> ";
                }
                if (mentionedRole.ToString() == guild.EveryoneRole.ToString())
                {
                    membercount = Context.Guild.Users.Count;
                    memberstext = "Literally everyone here.";
                }
                    if (memberstext != "")
                {
                    embed.AddField(x =>
                    {
                        x.Name = $"Members ({membercount})";
                            x.Value = $"{memberstext}";
                        x.IsInline = false;
                    });
                }
                
                else if (memberstext.ToString().Length > 1024)
                {
                    embed.AddField(x =>
                    {
                        x.Name = $"Members ({membercount})";
                        x.Value = $"Loads! I can't show 'em here!";
                        x.IsInline = false;
                    });
                }
                else
                {
                    embed.AddField(x =>
                    {
                        x.Name = $"Members ({membercount})";
                        x.Value = $"None";
                        x.IsInline = false;
                    });
                }
                builder.Clear();
                string perms = "";
                if (mentionedRole.Permissions.ToList().Count == 0)
                {
                    perms += "[No permissions]";
                }
                foreach (GuildPermission perm in mentionedRole.Permissions.ToList())
                {
                    perms += $"`{perm.ToString()}`, ";
                    
                }
                embed.AddField(x => {
                    x.Name = $"Permissions";
                    x.Value = $"{perms}";
                    x.IsInline = false;
                });

                await channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("mock"), Summary("rEpEaTs yOuR tExT lIkE tHiS.")]
        public async Task Mock([Summary("tHe tExT yOu wAnT tO mOcK")][Remainder] string input = "")
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
        
        [Command("userinfo"), Alias("whois")]
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        [Summary("Gets information about the specified user")]
        [RequireContext(ContextType.Guild)]
        public async Task UserInfo([Summary("OPTIONAL: User to check their info")][Remainder] IGuildUser user = null)
        {
            user = user ?? (SocketGuildUser)Context.User;
            EmbedBuilder emb = new EmbedBuilder();

            string userRoles = DiscordHelpers.GetListOfUsersRoles(user);
            if (userRoles.Length > 1024)
                userRoles = "Too many! Woah.";

            // Find user's highest role so the embed will be coloured with the role's colour

            IRole highestRole = DiscordHelpers.GetUsersHigherstRole(user);

            if (highestRole != null)
                emb.Color = highestRole.Color;

            var picture = user.GetAvatarUrl();
            string nitro = "<:KBNitro:587753434812514324> (Possible nitro user)";
            string nitro2 = "<:KBNitro:587753434812514324>";

            // If the user has a default avatar
            string useravatar = "";
            if (string.IsNullOrEmpty(user.AvatarId))
                useravatar = $"https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png";
            else
                useravatar = user.GetAvatarUrl();
            emb.WithThumbnailUrl(useravatar);

            string userpic = "";
            // If the user has a default avatar
            if (string.IsNullOrEmpty(Context.User.AvatarId))
                userpic = $"https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png";
            else
                userpic = Context.User.GetAvatarUrl();

            emb.WithFooter($"User info requested by {Context.User.Username}", userpic);
            emb.Description = $"{user.Username}#{user.Discriminator} | {user.Id}";

            if (user.IsBot) //if the user is a bot
                emb.Description += " <:Bot:593232908735741989>";

            if (CheckForDonator.CheckIfDonator(user.Id, Context).Result == true) //if the user is a donator
                emb.Description += $" <a:KBDonator:706944857964544040>";

            emb.WithTitle("=== USER INFORMATION ===");
            emb.WithCurrentTimestamp();
            //GIF avatar looking
            if (picture == $"https://cdn.discordapp.com/avatars/{user.Id}/{user.AvatarId}.gif?size=128")
            {
                emb.Description += $" {nitro2}";
            }
            //Tags looking for nitro
            else if (user.Discriminator == "0001")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0002")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0003")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0004")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0005")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0006")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0007")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0008")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0009")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0010")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "6969")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "9999")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "1337")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0420")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0069")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "4200")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "6666")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "6669")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "4200")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "1111")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "2222")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "3333")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "4444")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "5555")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "7777")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "8888")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "6942")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "4269")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "1000")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "2000")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "3000")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "4000")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "5000")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "6000")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "7000")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "8000")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "9000")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0007")
                emb.Description += $" {nitro}";
            else if (user.Discriminator == "0070")
                emb.Description += $" {nitro}";
            emb.AddField("Created account at", $"{user.CreatedAt.DateTime.ToLongDateString()} {user.CreatedAt.DateTime.ToLongTimeString()} (By UTC)");
            
            emb.AddField("Joined server at", $"{user.JoinedAt.Value.DateTime.ToLongDateString()} {user.JoinedAt.Value.DateTime.ToLongTimeString()} (By UTC)");

            // Display the list of all of user's roles
            if (string.IsNullOrEmpty(userRoles) == false)
                emb.AddField("Role(s)", userRoles);

           
            // Display the list of all of user's permissions
            string userPermissions = GetUserPermissions(user);

            if (string.IsNullOrEmpty(userPermissions) == false)
                emb.AddField("Permissions", userPermissions);
            string statusemoji = "";

            if (user.Activity != null && user.Activity.Type.ToString() == "Streaming")
            {
                statusemoji = "<:KBStreaming:609828634596868127>";
            }
            else if (user.Status == UserStatus.Online)
            {
                statusemoji = "<:KBOnline:587753462477881468>";
            }
            else if (user.Status == UserStatus.Idle)
            {
                statusemoji = "<:KBaway:587753408363102239>";
            }
            else if (user.Status == UserStatus.DoNotDisturb)
            {
                statusemoji = "<:KBdnd:587753420543230019>";
            }
            else if (user.Status == UserStatus.Offline)
            {
                statusemoji = "<:KBOffline:587753447902937109>";
            }
            
            var status = $"{(user.Status == UserStatus.DoNotDisturb ? "Do Not Disturb" : user.Status.ToString())} {statusemoji}";
            if (user.Activity != null)
            {
                status += $"{user.Activity.Type.ToString()} **{user.Activity.Name}**";
            }
            
            emb.AddField($"Status", status);

            string KBTeam = "";
            var ID = user.Id.ToString();
            // Display the KB team position if found
            try
            {
                var kbhq = Context.Client.GetGuild(550064334714175512);
                var supportrole = kbhq.GetRole(550070352907337751);
                var user2 = kbhq.GetUser(user.Id) as IGuildUser;
                if (user2.RoleIds.Contains(supportrole.Id)) //Checks if the user has support role in KBHQ
                    KBTeam = "KB Support";
            }
            catch
            {

            }
            //Bot owner (me)
            if (ID == "223530903773773824")
                KBTeam = "Bot owner";
            //KBHQ Staff => Shane, Ehsan
            if (ID == "238353818125991936" || ID == "196354024491057152")
                KBTeam = "KBHQ Staff";
            if (string.IsNullOrEmpty(KBTeam) == false)
                emb.AddField("KillerBot Team", KBTeam);

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
                permissions += "Ban Members, ";

            if (user.GuildPermissions.SendMessages)
                permissions += "Send Messages, ";
            
            if (user.GuildPermissions.ViewChannel)
                permissions += "View channels, ";

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

            if (user.GuildPermissions.ViewAuditLog)
                permissions += "Audit log, ";

            if (user.GuildPermissions.MoveMembers)
                permissions += "Move Members, ";

            if (user.GuildPermissions.MuteMembers)
                permissions += "Mute Members, ";

            if (user.GuildPermissions.Stream)
                permissions += "Stream, ";

            if (user.GuildPermissions.PrioritySpeaker)
                permissions += "Priority Speaker, ";

            if (user.GuildPermissions.ChangeNickname)
                permissions += "Change Nickname, ";

            if (string.IsNullOrEmpty(permissions))
                permissions += "No permissions. ";

            return permissions.Remove(permissions.Length - 2);
        }

        [Command("dadjoke")]
        [Summary("Random dad joke. vErY fUnNy :)")]
        public async Task DadJoke()
        {
            var wr = (HttpWebRequest)WebRequest.Create("https://icanhazdadjoke.com/");
            wr.Accept = "application/json";
            wr.UserAgent = "KillerBot";
            await ReplyAsync(JsonConvert.DeserializeObject<Dadjoke>(new StreamReader(wr.GetResponse().GetResponseStream()).ReadToEnd()).joke);
        }
        
        [Command("embed")]
        [Summary("Create an embed in a specific channel.")]
        [Remarks("You can seperate the embed title from the description with a |")]
        [Example("k!embed #cool #00F1FF epic title | epic description")]
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task Embed([Summary("OPTIONAL: Text channel to send the embed in")]SocketTextChannel channel, [Summary("The embed's color (HEX CODE)")]string color, [Summary("Embed's title | OPTIONAL: Embed's description")][Remainder] string args = null)
        {
            var user = Context.User as SocketGuildUser;
            if (((SocketGuildUser)Context.User).GetPermissions(channel).SendMessages == false)
            {
                await ReplyAsync($"{Constants.fail} You do not have enough permissions to send that message in that channel.");
                return;
            }

            var hex = color.Replace("#", "");

            if (!new Regex("^[a-zA-Z0-9]*$").IsMatch(hex) || hex.Length != 6)
            {
                await ReplyAsync($"{Constants.fail} Please enter a valid hexadecimal. \nExample: `k!embed #general #00F1FF epic title | epic description`");
                return;
            }
            if (args == null)
            {
                await ReplyAsync($"{Constants.fail} Please include a title for the embed and an optional description. \nExample: `k!embed #general #00F1FF epic title | epic description`");
                return;
            }
            var RGB = HexToRGB(hex);
            var td = args.Split('|', StringSplitOptions.RemoveEmptyEntries);

            var embed = new EmbedBuilder()
                .WithColor(RGB.R, RGB.G, RGB.B);

            if (td.Length == 1)
            {
                embed.Title = args;
            }
            else if (td.Length == 2)
            {
                bool title = false;
                foreach (var text in td)
                {
                    if (title == false)
                    {
                        title = true;
                        embed.Title = $"{text}";
                    }
                    else
                        embed.Description = $"{text}";
                }
            }
            var msg = await channel.SendMessageAsync("", false, embed.Build());
            var emb = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithDescription($"{Constants.success} Embed sent in {channel.Mention}! \n[Jump To Message!]({msg.GetJumpUrl()})");
            await ReplyAsync("", false, emb.Build());
        }
        [Command("embed")]
        [Summary("Create an embed in the current channel.")]
        [Remarks("You can seperate the embed title from the description with a |")]
        [Example("k!embed #00F1FF epic title | epic description")]
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task Embed([Summary("The embed's color (HEX CODE)")]string color, [Summary("Embed's title | OPTIONAL: Embed's description")][Remainder] string args = null)
        {
            var hex = color.Replace("#", "");

            if (!new Regex("^[a-zA-Z0-9]*$").IsMatch(hex) || hex.Length != 6)
            {
                await ReplyAsync($"{Constants.fail} Please enter a valid hexadecimal. \nExample: `k!embed #00F1FF epic title | epic description`");
                return;
            }
            if (args == null)
            {
                await ReplyAsync($"{Constants.fail} Please include a title for the embed and an optional description. \nExample: `k!embed #00F1FF epic title | epic description`");
                return;
            }
            var RGB = HexToRGB(hex);
            var td = args.Split('|', StringSplitOptions.RemoveEmptyEntries);

            var embed = new EmbedBuilder()
                .WithColor(RGB.R, RGB.G, RGB.B);

            if (td.Length == 1)
            {
                embed.Title = args;
            }
            else if (td.Length == 2)
            {
                bool title = false;
                foreach (var text in td)
                {
                    if (title == false)
                    {
                        title = true;
                        embed.Title = $"{text}";
                    }
                    else
                        embed.Description = $"{text}";
                }
            }
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
        // Convert a hexidecimal to an RGB value (input does not include the '#')
        public static Color HexToRGB(string hex)
        {
            // First two values of the hex
            int r = int.Parse(hex.Substring(0, hex.Length - 4), System.Globalization.NumberStyles.AllowHexSpecifier);

            // Get the middle two values of the hex
            int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            // Final two values
            int b = int.Parse(hex.Substring(4), System.Globalization.NumberStyles.AllowHexSpecifier);

            return new Discord.Color(r, g, b);
        }

        [Command("FindMessageID"), Alias("getmessageid", "messageid", "fmi", "gmi")]
        [Summary("Gets the message id of a message in the current channel with the provided message text")]
        [Remarks("Keep in mind that this isn't the best efficient way to get the ID of a message. If you're having any trouble try doing it manually.")]
        [Example("k!messageid Hey find my id!")]
        [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task FindMessageIDAsync([Summary("The content of the message to search for")][Remainder] string messageContent)
        {
            if (messageContent.IsEmptyOrWhiteSpace())
            {
                _ = await ReplyAsync("You need to specify the text of the message to search for.").ConfigureAwait(false);
                return;
            }
            const int searchDepth = 100;
            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(searchDepth).FlattenAsync().ConfigureAwait(false);
            IEnumerable<IMessage> matches = messages.Where(x => x.Content.StartsWith(messageContent.Trim(), StringComparison.OrdinalIgnoreCase));
            if (matches == null || !matches.Any())
            {
                _ = await ReplyAsync($"Message not found. Hint: Only the last {searchDepth} messages in this channel are scanned.").ConfigureAwait(false);
                return;
            }
            else if (matches.Count() > 1)
            {
                _ = await ReplyAsync($"{matches.Count()} Messages found. Please be more specific.").ConfigureAwait(false);
                return;
            }
            else
            {
                _ = await ReplyAsync($"The message Id is: {matches.First().Id} \n**TIP:** You can use \"k!quote {matches.First().Id}\" to quote the message in this channel.").ConfigureAwait(false);
            }
        }

        [Command("report"), Alias("bug", "bugreport", "reportbug")]
        [Cooldown(10)]
        [Summary("Send a report about a bug to the bot owner. Spam/troll is not tolerated.")]
        public async Task BugReport([Summary("Information about the bug you want to report")][Remainder] string report = null)
        {
            if (report == null)
            {
                await ReplyAsync($"{Constants.fail} Please provide information about the bug you want to report. Make sure to explain about the bug so the team can easily find it out!");
                return;
            }
            var channel = Context.Client.GetChannel(588015155015843903) as SocketTextChannel;
            var application = await Context.Client.GetApplicationInfoAsync();

            var embed = new EmbedBuilder()
            {
                Color = (Color.Red)
            };
            if (report.Contains("discord.gg/"))
            {
                embed.WithDescription($"[Message contained a server invite link] \n \n || {report} ||");
            }
            else
            {
                embed.Description = $"{report}";
            }
            
            embed.WithFooter(new EmbedFooterBuilder().WithText($"Message from: {Context.User.Username}#{Context.User.Discriminator} | Guild: {Context.Guild.Name}"));
            var reportmsg = channel.SendMessageAsync("", false, embed.Build());
            if (Global.MessagesIdToTrack == null)
            {
                Global.MessagesIdToTrack = new Dictionary<ulong, string>();
            }

            Global.MessagesIdToTrack.Add(reportmsg.Result.Id, report);
            await reportmsg.Result.AddReactionAsync(new Emoji("✅"));
            await reportmsg.Result.AddReactionAsync(new Emoji("❌"));
            embed.Description = $"You have sent a bug report to KillerBot HQ. It will be reviewed soon.";

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}





