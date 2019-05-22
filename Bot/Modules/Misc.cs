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
using Humanizer;
using System.Diagnostics;
using System.Data;
using System.Runtime.InteropServices;
using Bot.Handlers;

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
           while(builder.Length > 6000)
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
        [Cooldown(5)]
        public async Task Version()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = new Color(114, 137, 218);
            builder.AddField("Version", $"The current version of the bot is: `1.0.0`");
            await ReplyAsync("", false, builder.Build());
        }


        /*  [Command("Spotify")]
          [Remarks("Usage: |prefix|spotify [user]")]
          public async Task SpotifyAsync(SocketGuildUser target = null)
          {
              var user = target ?? Context.User;
              if (user.Activity is SpotifyGame spotify)
              {
                  var emb = new EmbedBuilder()
               .WithAuthor(user)
               .WithDescription($"**Track:** [{spotify.TrackTitle}]({spotify.TrackUrl})\n" +
                                       $"**Album:** {spotify.AlbumTitle}\n" +
                                       $"**Duration:** {(spotify.Duration.HasValue ? spotify.Duration.Value.Humanize(2) : "No duration provided.")}\n" +
                                       $"**Artists:** {string.Join(", ", spotify.Artists)}")
                      .WithThumbnailUrl(spotify.AlbumArtUrl);
                      ReplyAsync("", false, emb.Build());
                  return;
              }

              await ReplyAsync("Target user isn't listening to Spotify right now.");
          } */

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
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Title = $"Help for '{query}'"
            };

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
                if (!string.IsNullOrEmpty(cmd.Summary))
                    cmdDescription += $" | {cmd.Summary}";
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
                throw new ArgumentException("Value cannot exceed 1024 characters");
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
            usage = "Usage : `|-calculate <equation>` , where `equation` must not contain any functions.";
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




        /* [Command("List")]
         [Summary("Manage lists with custom accessibility by role")]
         public async Task ManageList(params String[] input)
         {
             if (input.Length == 0) { return; }
             var user = Context.User as SocketGuildUser;
             var roleIds = user.Roles.Select(r => r.Id).ToArray();
             var availableRoles = Context.Guild.Roles.ToDictionary(r => r.Name, r => r.Id);
             var output = _listManager.HandleIO(new ListHelper.UserInfo(user.Id, roleIds), availableRoles, Context.Message.Id, input);
             RestUserMessage message;
             if (output.permission != ListHelper.ListPermission.PRIVATE)
             {
                 message = (RestUserMessage)await Context.Channel.SendMessageAsync(output.outputString, false, output.outputEmbed);
             }
             else
             {
                 var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                 message = (RestUserMessage)await dmChannel.SendMessageAsync(output.outputString, false, output.outputEmbed);
             }
             if (output.listenForReactions)
             {
                 await message.AddReactionAsync(ListHelper.ControlEmojis["up"]);
                 await message.AddReactionAsync(ListHelper.ControlEmojis["down"]);
                 await message.AddReactionAsync(ListHelper.ControlEmojis["check"]);
                 ListManager.ListenForReactionMessages.Add(message.Id, Context.User.Id);
             }
         } */

        [Command("echo")]
        [Remarks("Make The Bot Say A Message")]

        public async Task Echo([Remainder] string message)
        {
            var embed = EmbedHandler.CreateEmbed("Message by: " + Context.Message.Author.Username, message, EmbedHandler.EmbedMessageType.Info, true);

            await Context.Channel.SendMessageAsync("", false, embed);
            await Context.Message.DeleteAsync();
        }



        [Command("botinfo")]
        [Summary("Shows All Bot Info.")]
        public async Task Info()
        {
            using (var process = Process.GetCurrentProcess())
            {
                var embed = new EmbedBuilder();
                var application = await Context.Client.GetApplicationInfoAsync();  /*for lib version*/
                embed.ImageUrl = application.IconUrl;  /*pulls bot Avatar. Not needed can be removed*/
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
                            y.Name = "Discord.net version:";  /*Title*/
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
                                             y.Name = "HeapSize:";
                                             y.Value = GetHeapSize();   /*pulls ram usage of modules/heaps*/
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

        // RPS

        [Command("rps")]

        [Summary("Do this command to know more info about the game. for example `rps r` for rock, `rps p` for paper and `rps s` for scissors.'")]

        [Remarks("rock paper scissors!")]

        public async Task Rps([Optional] string input)

        {

            if (input == null)

            {

                await ReplyAsync(

                    " To play rock, paper, scissors" +

                    "\n:waning_gibbous_moon: type `@KillerBot#5438 rps rock` or `@KillerBot#5438 rps r` to pick rock" +

                    "\n\n:newspaper: type `@KillerBot#5438 rps paper` or `@KillerBot#5438 rps p` to pick paper" +

                    "\n\n✂️ type `@KillerBot#5438 rps scissors` or `@KillerBot#5438 rps s` to pick scissors"

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
