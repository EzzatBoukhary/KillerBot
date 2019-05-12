/* using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;

namespace Bot.Modules
{
    [Group("Bots")]
    [Summary("Allows access to pending and archived invite links to bots. This allows for you to submit your invite links for bots so that the guild's managers can add them.")]
    public class ServerBots : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        public ServerBots(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }

        [Serializable]
        public class AllGuildsData
        {
            public List<GuildData> guilds;

            public GuildData GetGuild(ulong id)
            {
                foreach (GuildData guild in guilds)
                    if (guild.guildId == id)
                        return guild;

                return null;
            }

            public AllGuildsData()
            {
                guilds = new List<GuildData>();
            }
        }

        [Serializable]
        public class GuildData
        {
            public ulong guildId;
            public List<Submission> queue;
            public List<Submission> archive;

            public GuildData(ulong _guildId)
            {
                guildId = _guildId;
                queue = new List<Submission>();
                archive = new List<Submission>();
            }

            public Submission GetSubmissionFromQueue(ulong id)
            {
                foreach (Submission submission in queue)
                    if (submission.botId == id)
                        return submission;

                return null;
            }

            public Submission GetSubmissionFromArchives(ulong id)
            {
                foreach (Submission submission in archive)
                    if (submission.botId == id)
                        return submission;

                return null;
            }
        }

        [Serializable]
        public class Submission
        {
            public ulong userId;
            public ulong botId;
            public string name;
            public string description;
            public DateTime timeSent;

            public Submission(ulong _botId, ulong _userId, string _name, string _description, DateTime _timeSent)
            {
                botId = _botId;
                name = _name;
                userId = _userId;
                description = _description;
                timeSent = _timeSent;
            }
        }

        public static AllGuildsData data;

        const string LINK_TEMPLATE_FIRST = "https://discordapp.com/api/oauth2/authorize?client_id=";
        const string LINK_TEMPLATE_LAST = "&scope=bot&permissions=1";
        const int SUBMISSIONS_PER_PAGE = 4;

        public static Task Init(GlobalGuildAccounts globalGuildAccounts)
        {
            data = new AllGuildsData();

            foreach (SocketGuild guild in Global.Client.Guilds)
            {
                GuildData savedData = globalGuildAccounts.GetById(guild.Id).BotData;
                if (savedData == null)
                {
                    AddGuild(guild.Id);
                }
                else
                {
                    data.guilds.Add(savedData);
                }
                StoreData(guild.Id, globalGuildAccounts);
            }

            return Task.CompletedTask;
        }

        public static Task JoinedGuild(SocketGuild guild)
        {
            AddGuild(guild.Id);
            return Task.CompletedTask;
        }

        async Task ArchiveSubmission(ulong id)
        {
            GuildData guildData = data.GetGuild(Context.Guild.Id);
            if (guildData != null)
            {
                Submission submission = guildData.GetSubmissionFromQueue(id);
                if (submission != null)
                {
                    guildData.archive.Add(submission);
                    guildData.queue.Remove(submission);
                    StoreData(id);
                    await Context.Channel.SendMessageAsync("Submission successfully archived.");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Could not find the given id in the pending list.");
                }
            }
            else
            {
                await ReplyAsync("Error: Guild data not found.");
            }
        }

        static void AddGuild(ulong guildId)
        {
            data.guilds.Add(new GuildData(guildId));
        }

        async Task AddSubmission(Submission submission, ulong guildId)
        {
            GuildData guild = data.GetGuild(guildId);
            if (guild != null)
            {
                guild.queue.Add(submission);
                StoreData(guildId);
                await Context.Channel.SendMessageAsync("Submission sent!");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Could not submit.");
            }
        }

        private void StoreData(ulong id)
        {
            StoreData(id, _globalGuildAccounts);
        }

        private static void StoreData(ulong id, GlobalGuildAccounts globalGuildAccounts)
        {
            var guildAccount = globalGuildAccounts.GetById(id);
            guildAccount.Modify(g => g.SetBotData(data.GetGuild(id)), globalGuildAccounts);
        }

        [Command("add")]
        [Alias("add")]
        [Remarks("Adds your bot's invite link to the invite queue where server managers can add your bot. Usage: bots add <bot's client id> <bot's name> \"|\" <description>")]
        public async Task AddBot(params string[] args)
        {
            if (ulong.TryParse(args[0], out ulong id))
            {
                if (data.GetGuild(Context.Guild.Id).GetSubmissionFromQueue(id) != null || data.GetGuild(Context.Guild.Id).GetSubmissionFromArchives(id) != null)
                {
                    await ReplyAsync("This bot has already been submitted.");
                    return;
                }

                if (args.Length > 2)
                {
                    string botName = "";
                    string description = "";
                    bool syntax = false;
                    for (int i = 1; i < args.Length; i++)
                    {
                        if (args[i] == "|")
                            syntax = true;
                        else if (syntax)
                        {
                            description += args[i];
                            if (args.Length - 1 > i)
                                description += " ";
                        }
                        else
                        {
                            botName += args[i];
                            if (args.Length - 1 > i)
                                botName += " ";
                        }
                    }

                    if (syntax)
                    {
                        await AddSubmission(new Submission(id, Context.User.Id, botName, description, DateTime.Now), Context.Guild.Id);
                    }
                    else
                        await ReplyAsync("Please include the description of your bot.");

                }
                else
                {
                    await ReplyAsync("Please include your bot's name.");
                }
            }
            else
            {
                await ReplyAsync("Please type a valid bot id.");
            }
        }

        [Command("list")]
        [Alias("list")]
        [Remarks("Views all pending bots' invite links in the order requested at. Usage: bots list <page number> <archives/pending>")]
        public async Task ViewBots(params string[] args)
        {
            if (!(args.Length == 2))
            {
                await ReplyAsync("Please use the right number of arguments.");
            }
            if (int.TryParse(args[0], out int page))
            {
                if (page <= 0)
                {
                    await ReplyAsync("You're really going to try that one on me??");
                    return;
                }

                List<Submission> list;
                if (args[1] == "pending")
                    list = data.GetGuild(Context.Guild.Id).queue;
                else if (args[1] == "archives")
                    list = data.GetGuild(Context.Guild.Id).archive;
                else
                {
                    await ReplyAsync("Please specify either archives or pending lists.");
                    return;
                }

                if (list.Count == 0)
                {
                    await ReplyAsync("There are no submissions in this list.");
                    return;
                }

                decimal pages = Math.Ceiling((decimal)(list.Count) / SUBMISSIONS_PER_PAGE);

                EmbedBuilder builder = new EmbedBuilder
                {
                    Title = $"**Bot links from {args[1]} list**",
                    Description = "",
                    Color = new Color(119, 165, 239),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Page {page}/{pages}"
                    }
                };

                if (pages >= page)
                {
                    for (int i = 0; i < SUBMISSIONS_PER_PAGE; i++)
                    {
                        try
                        {
                            int index = i + (SUBMISSIONS_PER_PAGE * (page - 1));

                            if (index < list.Count)
                            builder.Description += $"{index + 1}. [{list[index].name}]({LINK_TEMPLATE_FIRST + list[index].botId + LINK_TEMPLATE_LAST})" +
                                $"by **{Context.Client.GetUser(list[index].userId).Username}**:\n{list[index].description}\n" +
                                $"*Client ID: {list[index].botId}*\n{list[index].timeSent} {TimeZone.CurrentTimeZone.StandardName}\n\n";
                        }
                        catch (IndexOutOfRangeException)
                        {
                            break;
                        }
                    }

                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    await ReplyAsync("There are not that many pages of submissions.");
                }
            }
            else
            {
                await ReplyAsync("Argument 1 should be an integer.");
            }
        }

        [Command("remove"), RequireUserPermission(GuildPermission.ManageGuild)]
        [Alias("remove")]
        [Remarks("Removes a submission from the pending or archives list. Requires ManageGuild Permission. Usage: bots remove <bot's client id> <archives/pending>")]
        public async Task RemoveBot(params string[] args)
        {
            if (!(args.Length == 2))
            {
                await ReplyAsync("Please use the right number of arguments.");
                return;
            }

            if (ulong.TryParse(args[0], out ulong id))
            {
                Submission toRemove;
                GuildData guildData = data.GetGuild(Context.Guild.Id);
                if (guildData != null)
                {
                    if (args[1] == "archives")
                    {
                        toRemove = guildData.GetSubmissionFromArchives(id);
                        if (toRemove != null)
                        {
                            guildData.archive.Remove(toRemove);
                            StoreData(id);
                            await ReplyAsync("Successfully removed submission from the archives list.");
                        }
                        else
                            await ReplyAsync("Could not find bot in archives list.");
                    }
                    else if (args[1] == "pending")
                    {
                        toRemove = guildData.GetSubmissionFromQueue(id);
                        if (toRemove != null)
                        {
                            guildData.queue.Remove(toRemove);
                            StoreData(id);
                            await ReplyAsync("Successfully removed submission from the pending list.");
                        }
                        else
                            await ReplyAsync("Could not find bot in pending list.");
                    }
                    else
                        await ReplyAsync("Please specify either archives or pending lists.");
                }
                else
                    await ReplyAsync("Error getting guild data.");
            }
            else
            {
                await ReplyAsync("Please type a valid bot id.");
            }
        }

        [Command("archive"), RequireUserPermission(GuildPermission.ManageGuild)]
        [Alias("archive")]
        [Remarks("Archives a submission from the pending list. Requires ManageGuild Permission. Usage: bots archive <bot's client id>")]
        public async Task ArchiveBot(params string[] args)
        {
            if (args.Length != 1)
            {
                await ReplyAsync("Please use the correct amount of arguments.");
                return;
            }

            if (ulong.TryParse(args[0], out ulong id))
            {
                await ArchiveSubmission(id);
            }
            else
            {
                await ReplyAsync("Please send a valid client id.");
            }
        }
    }
}  */
