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
        [Command("invite"), Alias("inv", "add", "addbot", "link"),Summary("Sends an invite link for the bot.")]
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
                embed2.WithDescription($"[Message contained a server invite link] \n \n || {feedback} ||");
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

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Green);
            embed.WithTitle("== Changelog ==");
            embed.Description = $" **== Minor Release ==** `v1.7.0` <:KBupdate:580129240889163787> \n \n**[Added]** \n \n<:KBdot:580470791251034123> New command `k!account reset-economy` \n \n<:KBdot:580470791251034123> New aliases for `k!invite` command. \n \n<:KBdot:580470791251034123> Added a user and role hierarchy system to all mute/unmute commands, changed some replies , and added audit logs reason \n \n<:KBdot:580470791251034123> New `k!addrole` and `k!removerole` commands! \nDo `k!help (command name)` for more info! \n \n<:KBdot:580470791251034123> Added 'Verification Level' and 'Explict Content Filter' to `k!serverinfo` command! \n \n<:KBdot:580470791251034123> BOT OWNER COMMAND ONLY: Added `k!owner-serverinfo` \n \n**[Changed-Fixed]** \n \n<:KBdot:580470791251034123> Changed a bit in the user hierarchy system to allow you to change your own nickname with `k!setnick` command. \n \n<:KBdot:580470791251034123> Fixed a bug in `k!serverinfo` command where the splash image won't appear if the server has more than the invite splash feature. \n \n<:KBdot:580470791251034123> Fixed the date format in `k!userinfo`, and `k!roleinfo` and made the status part in the `k!userinfo` command show streaming and the activity name if there is any! \n \n<:KBdot:580470791251034123> Made small changes to `k!report`, `k!feedback`, `k!logs`, `k!guildlist`, `k!account mydata` and `k!info` commands \n \n \nPlease report bugs using `k!report (bug)` if you see any in the future!";
            embed.WithFooter(x =>

            {

                x.WithText("Last updated: August 10th - 2019 8:39 PM GMT");



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
        [Summary("Wondering about the bot's status?")]
        [Remarks("Sends a link to the bot's support server with updates about the status of the bot and connection")]
        public async Task StatusAsync()
        {
            await ReplyAsync("Join the bot's support server to know all the information about the status of the bot and connection: https://discord.gg/DNqAShq");
        }

    [Command("Avatar")]
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
        [Cooldown(3)]
        [Alias("sinfo", "aboutserver")]
        [Summary("Shows server information.")]
        public async Task sinfo()
        {
            var channel = (ITextChannel)Context.Channel;
            var guild = Context.Guild as SocketGuild;
            var ownername = guild.GetUser(guild.OwnerId);
            var textchn = guild.TextChannels.Count();
            var voicechn = guild.VoiceChannels.Count();

            var createdAt = new DateTime(2015, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(guild.Id >> 22);
            var features = string.Join("\n", guild.Features);
            if (string.IsNullOrWhiteSpace(features))
                features = "-";
            var embed = new EmbedBuilder()
                .WithAuthor("== SERVER INFORMATION ==")
                .WithTitle(guild.Name)
                .AddField(fb => fb.WithName("ID").WithValue(guild.Id.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Owner").WithValue(ownername.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Created at").WithValue($"{ createdAt:dd/MM/yyyy HH:mm:ss} UTC (dd/mm)").WithIsInline(true))
                .AddField(fb => fb.WithName("Members").WithValue(guild.MemberCount.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Text channels").WithValue(textchn.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Voice channels").WithValue(voicechn.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Roles").WithValue((guild.Roles.Count - 1).ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Region").WithValue(guild.VoiceRegionId.ToString()).WithIsInline(true))
                .AddField(fb => fb.WithName("Verification Level").WithValue(guild.VerificationLevel.ToString()).WithIsInline(true))
                .WithColor(9896005);
            if (guild.ExplicitContentFilter.ToString() == "MembersWithoutRoles")
            {
                embed.AddField(fb =>
                     fb.WithName("Explict Content Filter")
                     .WithValue("Scans messages from members without roles").WithIsInline(false));
            }
            if (guild.ExplicitContentFilter.ToString() == "Disabled")
            {
                embed.AddField(fb =>
                     fb.WithName("Explict Content Filter")
                     .WithValue("No message scanning").WithIsInline(true));
            }
            if (guild.ExplicitContentFilter.ToString() == "AllMembers")
            {
                embed.AddField(fb =>
                     fb.WithName("Explict Content Filter")
                     .WithValue("Scans messages from all members").WithIsInline(false));
            }
            if (Uri.IsWellFormedUriString(guild.IconUrl, UriKind.Absolute))
                embed.WithThumbnailUrl(guild.IconUrl);
            if (features != "-")
            {
                embed.AddField(fb =>
                    fb.WithName("Features")
                    .WithValue(features).WithIsInline(true));
            }

            if (guild.Emotes.Any())
            {
                embed.AddField(fb =>
                    fb.WithName("Custom emojis " + $"({guild.Emotes.Count})")
                    .WithValue(string.Join(" ", guild.Emotes
                        .Take(20)
                        .Select(e => $"{e.ToString()}"))));
            }
            if (features.Contains("INVITE_SPLASH"))
            {
                embed.AddField(fb =>
                    fb.WithName("Splash Icon")
                    .WithValue("").WithIsInline(true));
                embed.WithUrl(guild.SplashUrl);
            }
            await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
        }
        
        [Command("roleinfo"), Summary("Returns info about a role."),Alias("RI","role")]
        [RequireContext(ContextType.Guild)]
        public async Task Role([Remainder, Summary("The role to return information about.")] string roleName)
        {

            EmbedBuilder embed = new EmbedBuilder();
            StringBuilder builder = new StringBuilder();

            IMessage message = Context.Message;
            IGuild guild = Context.Guild;
            IMessageChannel channel = Context.Channel;

            IReadOnlyCollection<IRole> rolesReadOnly = guild.Roles;
            IRole tgt = null;

            List<IRole> roleList = rolesReadOnly.ToList();

            foreach (IRole role in roleList)
            {
                if (role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase))
                {
                    tgt = role;
                    break;
                }
                else
                {
                    continue;
                }
            }

            if (tgt == null)
            {
                await Context.Channel.SendMessageAsync($"Unable to find role by the name of `{roleName}`.");
                return;
            }
            else
            {
                await channel.TriggerTypingAsync();

                embed.Title = $"{tgt.Name}";
                embed.Color = tgt.Color;
                embed.Footer = new EmbedFooterBuilder()
                {
                    Text = $"Created on: { tgt.CreatedAt.DateTime.ToLongDateString() } { tgt.CreatedAt.DateTime.ToLongTimeString() } (By UTC)"
                };
                embed.Title = "=== ROLE INFORMATION ===";
                embed.Timestamp = DateTime.UtcNow;

                embed.AddField(x => {
                    x.Name = $"Name";
                    x.Value = $"{tgt.Name}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"ID";
                    x.Value = $"{tgt.Id}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"Hoisted";
                    x.Value = $"{tgt.IsHoisted}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"Managed";
                    x.Value = $"{tgt.IsManaged}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"Mentionable";
                    x.Value = $"{tgt.IsMentionable}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"Position";
                    x.Value = $"{tgt.Position}";
                    x.IsInline = true;
                });
                embed.AddField(x => {
                    x.Name = $"Color";
                    x.Value = $"{tgt.Color.R}, {tgt.Color.G}, {tgt.Color.B}";
                    x.IsInline = true;
                });

                builder.Clear();
                string perms = "";
                if (tgt.Permissions.ToString().Length < 4)
                {
                    perms = "No perms";
                }
                foreach (GuildPermission perm in tgt.Permissions.ToList())
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
        
        [Command("userinfo")]
        [Cooldown(3)]
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
                emb.Description += $" {nitro}";
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
                status += $"{user.Activity.Type.ToString()} **{user.Activity.Name.ToString()}**";
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





