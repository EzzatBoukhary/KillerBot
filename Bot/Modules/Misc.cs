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
using Urban.NET;
using Humanizer;
using System.Diagnostics;
using System.Data;
using System.Runtime.InteropServices;
using Bot.Handlers;
using Discord.Addons.Interactive;
using System.Net.Http;
using Gommon;
using Newtonsoft.Json.Linq;

namespace Bot.Modules
{
    public class Misc : ModuleBase<MiunieCommandContext>
    {
        private CommandService _service;
        private readonly ListManager _listManager;
        private int _fieldRange = 10;

        public Misc(CommandService service, ListManager listManager)
        {
            _service = service;
            _listManager = listManager;
        }

        /* [Cooldown(15)]
         [Command("help"), Alias("h"),
          Remarks(
              "DMs you a huge message if called without parameter - otherwise shows help to the provided command or module")]
         public async Task Help()
         {
             await Context.Channel.SendMessageAsync("Check your DMs.");

             var dmChannel = await Context.User.GetOrCreateDMChannelAsync();

             var contextString = Context.Guild?.Name ?? "DMs with me";
             var emb = new EmbedBuilder()
                 .WithTitle("These are the commands you can use:")
                 .WithColor(Color.Red)
                 .WithDescription("**__Prefix Commands:__** \nprefix add \nprefix remove \nprefix list \n \n**__Moderation Commands:__**" + "\nkick \nBan \nunban \nmute \nunmute \nchangenick \ncreatetext \ncreatevoice \npurge \nannounce \n \n**__Basic Commands__:** \nHello \nversion \ninvite \nserver \nuptime \nfeedback \necho \naccount info \naccount commandhistory \nuserinfo \nserverinfo \nping \nusercount \nbotinfo \nweather <city> \nreport, bugreport, bug, reportbug \n8ball \n \n**__Join-leave announcements:__** \n announcements setchannel \nannouncements unsetchannel \nwelcome add \nwelcome list \nwelcome remove \nleave add \nleave list \nleave remove \n \n**__Fun/Misc Commands:__** \nremind \nremind list \nremind remove \nflip or flipcoin \nrps \nquote \navatar \nchoose \ncalculate (do `help calculate` for more info) \n \n**Tags:** \ntag new \ntag edit \ntag remove \ntag list \nprivatetag/ptag new \nptag edit \nptag remove \nptag list \n \n**Combat:** \nfight \nslash \ngiveup \n \n**Auctions:** \nauction \nbid \nauctioncheck \nauctionend");


             await dmChannel.SendMessageAsync("", false, emb.Build());
         } */




        [Command("help"), Alias("h"),
            Remarks(
                "DMs you a huge message if called without parameter - otherwise shows help to the provided command or module")]
        [Cooldown(5)]
        public async Task Help()
        {
            await Context.Channel.SendMessageAsync("Check your DMs.");

            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();

            var contextString = Context.Guild?.Name ?? "DMs with me";
            var builder = new EmbedBuilder()
            {
                Title = "Help",
                Description = $"These are the commands you can use in {contextString}",
                Color = new Color(114, 137, 218)
            };

            foreach (var module in _service.Modules)
            {
                await AddModuleEmbedField(module, builder);
            }

            // We have a limit of 6000 characters for a message, so we are taking first ten fields
            // and then sending the message. In the current state it will send 2 messages.

            var fields = builder.Fields.ToList();
            while (builder.Length > 6000)
            {
                builder.Fields.RemoveRange(0, fields.Count);
                var firstSet = fields.Take(_fieldRange);
                builder.Fields.AddRange(firstSet);
                if (builder.Length > 6000)
                {
                    _fieldRange--;
                    continue;
                }
                await dmChannel.SendMessageAsync("", false, builder.Build());
                fields.RemoveRange(0, _fieldRange);
                builder.Fields.RemoveRange(0, _fieldRange);
                builder.Fields.AddRange(fields);
            }

            await dmChannel.SendMessageAsync("", false, builder.Build());

            while (builder.Length > 6000)
            {
                builder.Fields.RemoveRange(0, fields.Count);
                var secondset = fields.Take(_fieldRange);
                builder.Fields.AddRange(secondset);
                if (builder.Length > 6000)
                {
                    _fieldRange--;
                    continue;
                }
                await dmChannel.SendMessageAsync("", false, builder.Build());
                fields.RemoveRange(0, _fieldRange);
                builder.Fields.RemoveRange(0, _fieldRange);
                builder.Fields.AddRange(fields);
            }
            // Embed are limited to 24 Fields at max. So lets clear some stuff
            // out and then send it in multiple embeds if it is too big.
            builder.WithTitle("")
                .WithDescription("")
                .WithAuthor("");
            while (builder.Fields.Count > 24)
            {
                builder.Fields.RemoveRange(0, 25);
                await dmChannel.SendMessageAsync("", false, builder.Build());

            }
        }

        [Command("version"), Alias("ver")]
        [Remarks("Returns the current version of the bot.")]
        [Cooldown(3)]
        public async Task Version()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = new Color(114, 137, 218);
            builder.AddField("Version", $"The current version of the bot is: `1.5.1`");
            await ReplyAsync("", false, builder.Build());
        }


        [Command("Uptime")]
        [Remarks("Usage: k!uptime")]
        public async Task UptimeAsync()
        {

            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = Color.Green;
            builder.AddField("Uptime", $"I've been online for **{(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(3)}**!");
            await ReplyAsync("", false, builder.Build());

        }

         [Command("help"), Alias("h")]
        [Remarks("Shows what a specific command or module does and what parameters it takes.")]
        [Cooldown(5)]
        public async Task HelpQuery([Remainder] string query)
        {
            var application = await Context.Client.GetApplicationInfoAsync();  /*for lib version*/
            var builder = new EmbedBuilder();

                builder.Color = new Color(114, 137, 218);
                builder.Title = $"Help for '{query}'";
                builder.WithFooter($"Requested by {Context.User}", Context.User.GetAvatarUrl());
                builder.WithCurrentTimestamp();
                builder.ThumbnailUrl = application.IconUrl;

            var result = _service.Search(Context, query);
            if (query.StartsWith("module "))
                query = query.Remove(0, "module ".Length);
            var emb = result.IsSuccess ? HelpCommand(result, builder) : await HelpModule(query, builder);

            if (emb.Fields.Length == 0)
            {
                await ReplyAsync($"Sorry, I couldn't find anything for \"{query}\".");
                return;
            }

            await Context.Channel.SendMessageAsync("", false, emb);
        }
       
        private static Embed HelpCommand(SearchResult search, EmbedBuilder builder)
        {
            foreach (var match in search.Commands)
            {
                var cmd = match.Command;
                var parameters = cmd.Parameters.Select(p => string.IsNullOrEmpty(p.Summary) ? p.Name : p.Summary);
                var paramsString = $"Parameters: {string.Join(", ", parameters)}" +
                                 //  $"\nPreconditions: {cmd.Preconditions.Humanize()}" +
                                   (string.IsNullOrEmpty(cmd.Remarks) ? "" : $"\nRemarks: {cmd.Remarks}") +
                                   (string.IsNullOrEmpty(cmd.Summary) ? "" : $"\nSummary: {cmd.Summary}");

              
                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = paramsString;
                    x.IsInline = false;
                });
            }

            return builder.Build();
        }

        private async Task<Embed> HelpModule(string moduleName, EmbedBuilder builder)
        {
            var module = _service.Modules.ToList().Find(mod =>
                string.Equals(mod.Name, moduleName, StringComparison.CurrentCultureIgnoreCase));
            await AddModuleEmbedField(module, builder);
            return builder.Build();
        }

        private async Task AddModuleEmbedField(ModuleInfo module, EmbedBuilder builder)
        {
            if (module is null) return;
            var descriptionBuilder = new List<string>();
            var duplicateChecker = new List<string>();
            foreach (var cmd in module.Commands)
            {
                var result = await cmd.CheckPreconditionsAsync(Context);
                if (!result.IsSuccess || duplicateChecker.Contains(cmd.Aliases.First())) continue;
                duplicateChecker.Add(cmd.Aliases.First());
                var cmdDescription = $"`{cmd.Aliases.First()}`";
              //  if (!string.IsNullOrEmpty(cmd.Summary))
                //   cmdDescription += $" | {cmd.Summary}";
                if (!string.IsNullOrEmpty(cmd.Remarks))
                    cmdDescription += $" | {cmd.Remarks}";
                if (cmdDescription != "``")
                    descriptionBuilder.Add(cmdDescription);
            }

            if (descriptionBuilder.Count <= 0) return;
            var builtString = string.Join("\n", descriptionBuilder);
            var testLength = builtString.Length;
            if (testLength >= 1024)
            {
                throw new ArgumentException("Value cannot exceed 1024 characters, please do `k!report help command value limit exceeded limit` if you see this message!");
            }
            var moduleNotes = "";
            if (!string.IsNullOrEmpty(module.Summary))
                moduleNotes += $" {module.Summary}";
            if (!string.IsNullOrEmpty(module.Remarks))
                moduleNotes += $" {module.Remarks}";
            if (!string.IsNullOrEmpty(moduleNotes))
                moduleNotes += "\n";
            if (!string.IsNullOrEmpty(module.Name))
            {
                builder.AddField($"__**{module.Name}:**__",
                    $"{moduleNotes} {builtString}\n{Constants.InvisibleString}");
            }
        } 

       
        string usage;

        [Command("calculate", RunMode = RunMode.Async)]
        [Summary("Calculate anything, you can use + for addition, - for subtraction, / ÷ or for division, x or * for multiplication, PI to get the PI value, E for 2.718281828459045, % for Mod")]
        [Remarks("You can use multiple signs at once for example `calculate 4 + 2 * 4`")]
        public async Task Calculate([Remainder]string equation)
        { //Needs improvement
            usage = "Usage : `k!calculate <equation>` , where `equation` must not contain any functions.";
            //Replaces all the possible math symbols that may appear
            //Invalid for the computer to compute
            equation = equation.ToUpper()
            .Replace("x", "*")
            .Replace("X", "*")
            .Replace("÷", "/")
            .Replace("MOD", "%")
            .Replace("PI", "3.14159265359")
            .Replace("E", "2.718281828459045");

            try
            {
                string value = new DataTable().Compute(equation, null).ToString();
                if (value == "NaN")
                {
                    await Context.Channel.SendMessageAsync("Infinity or undefined");
                }
                else
                {
                    await Context.Channel.SendMessageAsync(value);
                }
            }
            catch (Exception)
            {
                await Context.Channel.SendMessageAsync("?");
            }
        }

        [Command("echo")]
        [Remarks("Make The Bot Say A Message")]

        public async Task Echo([Remainder] string message)
        {
            var embed = EmbedHandler.CreateEmbed("Message by: " + Context.Message.Author.Username, message, EmbedHandler.EmbedMessageType.Info, true);

            await Context.Channel.SendMessageAsync("", false, embed);
            await Context.Message.DeleteAsync();
        }

         [Command("ud"),Alias("urbandictionary"), Summary("Gives you the definition of your word on Urban Dictionary.")]
        [Ratelimit(5, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
          public async Task UrbanDictionary([Remainder] string word)
          {
              UrbanService client = new UrbanService();
              var data = await client.Data(word);
              var pages = new List<string>();
              var embed = new EmbedBuilder();

           // if (data.List == null)
            //    throw new ArgumentException("No results found.");
            embed.Color = new Color(200, 200, 0);
              foreach (var entry in data.List)
              {
                
                string result = $"[{entry.Word}]({entry.Permalink})\n\nDefinition: ";
                  string def = entry.Definition.Replace("[", "");
                result += def.Replace("]", "");
                result += $"\n\nExample: {entry.Example}";
                result += $"\n\n👍{entry.ThumbsUp}\t👎{entry.ThumbsDown}";
                  embed.WithDescription(result);
              }
              
            await ReplyAsync("", false, embed.Build());

           
          }
      

        //8ball
        [Command("8ball"), Summary("Answers all your questions in life.")]
        [Alias("8b", "ask")]
        [Ratelimit(10, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task EightBall([Remainder] string question = null)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException(":8ball: 8 ball requests a question.");
            string result;
            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get,
                    "https://8ball.delegator.com/magic/JSON/" + question))
                {
                    var response = await httpClient.SendAsync(request);
                    result = await response.Content.ReadAsStringAsync();
                }
            }

            result = result.Substring(result.IndexOf("er\": \"", StringComparison.Ordinal) + 6);
            result = result.Remove(result.IndexOf("\",", StringComparison.Ordinal));
            var embed = new EmbedBuilder()
                .WithColor(28, 1, 34)
                .WithAuthor(author =>
                {
                    author
                        .WithName("8 Ball Result")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/497373849042812930/581476863692636181/bot-icon.png");
                })
                .WithDescription(result);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("botinfo")]
        [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.None)]
        [Summary("Shows All Bot Info.")]
        public async Task Info()
        {
            using (var process = Process.GetCurrentProcess())
            {
                var embed = new EmbedBuilder();
                var application = await Context.Client.GetApplicationInfoAsync();  /*for lib version*/
                embed.ThumbnailUrl = application.IconUrl;  /*pulls bot Avatar. Not needed can be removed*/
                embed.WithColor(new Color(0x4900ff))  /*Hexacode colours*/

        .AddField(y =>  /*Adds a Field*/
        {
            /*new embed field*/
            y.Name = "Author:";  /*Field name here*/
            y.Value = application.Owner.Username; application.Owner.Id.ToString();  /*Pulls the owner's name*/
            y.IsInline = false;
        })
        .AddField(y =>  /*add new field, rinse and repeat*/
        {
            y.Name = "Uptime:";
            var time = DateTime.Now - process.StartTime;  /*Subtracts current time and start time to get Uptime*/
            var sb = new StringBuilder();
            if (time.Days > 0)
            {
                sb.Append($"{time.Days}d ");  /*Pulls the Uptime in Days*/
            }
            if (time.Hours > 0)
            {
                sb.Append($"{time.Hours}h ");  /*Pulls the Uptime in Hours*/
            }
            if (time.Minutes > 0)
            {
                sb.Append($"{time.Minutes}m ");  /*Pulls the Uptime in Minutes*/
            }
            sb.Append($"{time.Seconds}s ");  /*Pulls the Uptime in Seconds*/
            y.Value = sb.ToString();
            y.IsInline = true;
        })
                        .AddField(y =>
                        {
                            y.Name = "Discord.Net version:";  /*Title*/
                            y.Value = DiscordConfig.Version;  /*pulls discord lib version*/
                            y.IsInline = true;
                        })
                                 .AddField(y =>
                                 {
                                     y.Name = "Amount of servers:";
                                     y.Value = (Context.Client as DiscordSocketClient).Guilds.Count.ToString();  /*Numbers of servers the bot is in*/
                                     y.IsInline = false;
                                 })
                                         .AddField(y =>
                                         {
                                             y.Name = "Heap Size:";
                                             y.Value = ($"{GetHeapSize()} MB");   /*pulls ram usage of modules/heaps*/
                                             y.IsInline = false;
                                         })

                                             .AddField(y =>
                                             {
                                                 y.Name = "Number Of Users:";
                                                 y.Value = (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count).ToString();  /*Counts users*/
                                                 y.IsInline = false;
                                             })
                                                 .AddField(y =>
                                                 {
                                                     y.Name = "Channels:";
                                                     y.Value = (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count).ToString();  /*Gets Number of channels*/
                                                     y.IsInline = false;
                                                 });

                await this.ReplyAsync("", false, embed.Build());
            }

        }


        private static string GetUptime()
          => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        [Command("info")]
        [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task InfoBot()
        {
            using (var process = Process.GetCurrentProcess())
            {
                var embed = new EmbedBuilder();
                var application = await Context.Client.GetApplicationInfoAsync();  /*for lib version*/
                embed.ThumbnailUrl = application.IconUrl;  /*pulls bot Avatar. Not needed can be removed*/
                embed.WithColor(new Color(0x4900ff));  /*Hexacode colours*/
                embed.WithTitle("KillerBot Information");
                embed.WithDescription("KillerBot is a multi-purpose bot with unique moderation, fun and utility commands that'll change how servers work to the better. \n \n**About my dev** \nI was made with :heart: by Panda#8822 with the help of some testers and support (<@238353818125991936> and <@333988268439764994>). I work on this bot as a hobby and i love to bring joy to my users by adding the features they want. \n \n**When was i created** \nKillerBot started back in 2016 and got hosted for a while until the hosting was stopped and the bot died in 2017. I then decided to bring it back and recode it completely in Discord.Net 2.0.1 and so i did, bringing KillerBot back at 03/03/2019 in a beta state until 05/23/2019 where it was officially released. And we're still going! <:Killerbot:587360915284819998> \n \n**Links** \n[Invite me!](https://discordapp.com/oauth2/authorize?client_id=263753726324375572&scope=bot&permissions=406874134) \n[Discord Support Server](https://discord.gg/DNqAShq) \n[Apply for bot support!](https://docs.google.com/forms/d/e/1FAIpQLSeyl-pHKe9hHic1UcpKaG4lzaMCt2a6Mgaj0PPPnurSgptrIw/viewform) \n------------- \n[Discord Bot list Website](https://discordbotlist.com/bots/263753726324375572) \n[Discord Bots Website](https://discordbots.org/bot/263753726324375572)");
                embed.WithCurrentTimestamp();

                await ReplyAsync("", false, embed.Build());
            }
        }
       
        [Command("randomcat")]
        [Cooldown(3)]
        [Alias("meow","cat")]
        [Summary("Retrieve a random cat photo")]
        public async Task CatPic()
        {
            var http = new HttpClient();
            var results = await http.GetStringAsync("http://aws.random.cat/meow").ConfigureAwait(false);
            string url = JObject.Parse(results)["file"].ToString();

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Connection to random.cat failed!");

            EmbedBuilder output = new EmbedBuilder()
                .WithTitle(":cat: Meow!")
                .WithImageUrl(url)
                .WithColor(Color.Orange);
            await ReplyAsync("",false, output.Build());
        }

        // RPS

        [Command("rps")]
        [Cooldown(3)]
        [Summary("Do this command to know more info about the game. for example `rps r` for rock, `rps p` for paper and `rps s` for scissors.'")]

        [Remarks("rock paper scissors!")]

        public async Task Rps([Optional] string input)

        {

            if (input == null)

            {

                await ReplyAsync(

                    " To play rock, paper, scissors" +

                    "\n:waning_gibbous_moon: type `k!rps rock` or `k!rps r` to pick rock" +

                    "\n\n:newspaper: type `k!rps paper` or `k!rps p` to pick paper" +

                    "\n\n✂️ type `k!rps scissors` or `k!rps s` to pick scissors"

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

                        return;

                }

                var choice = new Random().Next(0, 3);



                string msg;

                if (pick == choice)

                    msg = "We both chose: " + GetRpsPick(pick) + " Draw, Try again";

                else if (pick == 0 && choice == 1 ||

                         pick == 1 && choice == 2 ||

                         pick == 2 && choice == 0)

                    msg = "My Pick: " + GetRpsPick(choice) + " Beats Your Pick: " + GetRpsPick(pick) +

                          "\nYou Lose! Try Again boi!";

                else

                    msg = "Your Pick: " + GetRpsPick(pick) + " Beats meh: " + GetRpsPick(choice) +

                          "\nCongratulations! You win!";





                var embed = new EmbedBuilder

                {

                    Title = "KillerBot - Rock Paper Scissors",

                    Description = $"{msg}",

                    ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()

                };

                await ReplyAsync("", false, embed.Build());

            }

        }



        private static string GetRpsPick(int p)

        {

            switch (p)

            {

                case 0:

                    return ":waning_gibbous_moon: ";

                case 1:

                    return ":newspaper:";

                default:

                    return "✂️";

            }

        }
        // END
    }
}
