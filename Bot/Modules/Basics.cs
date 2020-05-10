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
        [Command("invite"), Alias("inv", "add", "addbot", "link"),Summary("Sends an invite link for the bot.")]
        [Cooldown(5)]
        public async Task InviteBot()
        {
            await ReplyAsync("**Add KillerBot to a server by clicking this link:** https://goo.gl/h3xHqU \n*Note: You need `Manage Server` or `Administrator` permission in a server in order to be able to add the bot.*");
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
            if (feedback.Contains("discord.gg/"))
            {
                embed2.WithDescription($"[Message contained a server invite link]");
            }
            else
            {
                embed2.WithDescription($"```{feedback}```");
                    }
            embed2.WithFooter(new EmbedFooterBuilder().WithText($"Feedback from: {Context.User.Username}#{Context.User.Discriminator} | Guild: {Context.Guild.Name}"))
            .WithAuthor($"{Context.User}", Context.User.GetAvatarUrl());

            var channel = Context.Client.GetChannel(550073251641032717) as SocketTextChannel;
            if (channel is null)
            {
                await ReplyAsync("Error 404: Couldn't find channel. Please do `k!report k!feedback results in error 404.`");
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
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        [Summary("Change log for the current version of the bot")]
        public async Task changes()
        {
            var dot = "<:KBdot:580470791251034123>";
            var embed = new EmbedBuilder();
            embed.WithColor(Color.Green);
            embed.WithTitle("== Changelog ==");
            embed.Description = $" **== Patch ==** `v1.10.3` <:KBupdate:580129240889163787> \n \n**[Changed/Fixed]** \n \n{dot} Fixed the `k!help` error.";
            embed.WithFooter(x =>

            {

                x.WithText("Last updated: May 10th - 2020 10:16 PM GMT");



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
        [Summary("Shows information about the server this was sent in like creation date, amount of members and more.")]
        public async Task sinfo()
        {
            var channel = (ITextChannel)Context.Channel;
            var guild = Context.Guild as SocketGuild;
            var ownername = guild.GetUser(guild.OwnerId);
            var textchn = guild.TextChannels.Count();
            var voicechn = guild.VoiceChannels.Count();
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
                embed.WithTitle(guild.Name);
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
            await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
        }
        
        [Command("roleinfo"), Summary("Returns info about a role."),Alias("RI")]
        [Example("k!roleinfo Cool role")]
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireContext(ContextType.Guild)]
        public async Task Role([Remainder, Summary("The role to return information about.")] string args)
        {

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
                    x.Name = $"Color";
                    x.Value = $"{mentionedRole.Color}";
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
                
                else if (memberstext.ToString().Length > 2048)
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
                if (mentionedRole.Permissions.ToString() == "512")
                {
                    perms = "No perms";
                }
                foreach (GuildPermission perm in mentionedRole.Permissions.ToList())
                {
                    if (perm.ToString() == "512")
                        continue;
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
        public async Task UserInfo([Summary("OPTIONAL: User to check their info")][Remainder] IGuildUser user = null)
        {
            user = user ?? (SocketGuildUser)Context.User;
            EmbedBuilder emb = new EmbedBuilder();

            string userRoles = DiscordHelpers.GetListOfUsersRoles(user);

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
            if (user.IsBot)
                emb.Description += "<:Bot:593232908735741989>";
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
            if (ID == "333988268439764994")
                KBTeam += "KB Support";
            if (ID == "238353818125991936" || ID == "196354024491057152")
                KBTeam += "KBHQ Staff";
            if (ID == "223530903773773824")
                KBTeam += "Bot owner";
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
            wr.UserAgent = "Hatsune Miku Discord Bot (speyd3r@meek.moe)";
            await ReplyAsync(JsonConvert.DeserializeObject<Dadjoke>(new StreamReader(wr.GetResponse().GetResponseStream()).ReadToEnd()).joke);
        }

        [Command("FindMessageID"), Alias("getmessageid", "messageid", "fmi", "gmi")]
        [Summary("Gets the message id of a message in the current channel with the provided message text")]
        [Remarks("Keep in mind that this isn't the best efficient way to get the ID of a message. If you're having any trouble try doing it manually.")]
        [Example("k!messageid Hey find my id!")]
        [RequireContext(ContextType.Guild)]
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
        public async Task BugReport([Summary("Information about the bug you want to report")][Remainder] string report)
        {
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





