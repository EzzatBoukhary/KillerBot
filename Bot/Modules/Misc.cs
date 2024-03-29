﻿using Bot.Preconditions;
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
using System.Reflection;
using Newtonsoft.Json;
using Discord.Net;
using System.Text.RegularExpressions;

namespace Bot.Modules
{
    public class Misc : ModuleBase<MiunieCommandContext>
    {
        private CommandService _service;
        private readonly ListManager _listManager;
        private int _fieldRange = 10;
        private static readonly HttpClient client = new HttpClient();

        public Misc(CommandService service, ListManager listManager)
        {
            _service = service;
            _listManager = listManager;
        }


        [Command("version"), Alias("ver")]
        [Remarks("Returns the current version of the bot.")]
        [Cooldown(3)]
        public async Task Version()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Color = new Color(114, 137, 218);
            builder.AddField("Version", $"The current version of the bot is: `1.12.2`");
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

        [Command("calculate", RunMode = RunMode.Async)]
        [Alias("math")]
        [Ratelimit(5, 1, Measure.Minutes, RatelimitFlags.None)]
        [Summary("Calculate anything, you can use + for addition, - for subtraction, / ÷ or for division, x or * for multiplication, PI to get the PI value, E for 2.718281828459045, % for Mod")]
        [Remarks("You can use multiple signs at once for example `calculate 4 + 2 * 4`")]
        public async Task Calculate([Remainder]string equation = null)
        {
            if (equation == null)
            {
                await ReplyAsync("Usage : `{prefix}calculate <equation>` , where `equation` must not contain any functions.");
                return;
            }
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

        [Command("say")]
        [Alias("echo")]
        [Remarks("Make KillerBot say anything!")]

        public async Task Echo([Remainder] string message = null)
        {
            if (message == null)
            {
                await ReplyAsync($"{Constants.fail} Please put the message you want me to echo.");
                return;
            }
            var embed = EmbedHandler.CreateEmbed("Message by: " + Context.Message.Author.Username, message, EmbedHandler.EmbedMessageType.Info, true);

            try
            {
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            catch
            {
                await ReplyAsync("<:KBfail:580129304592252995> Oopsie! I couldn't send that embed message.");
            }
            try
            { 
                await Context.Message.DeleteAsync();
            }
            catch
            {
            }
        }

         [Command("ud"),Alias("urbandictionary"), Summary("Gives you the definition of your word on Urban Dictionary.")]
        [Ratelimit(5, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
          public async Task UrbanDictionary([Remainder] string word = null)
          {
            if (word == null)
                throw new ArgumentException("Please type a word to search");
              UrbanService client = new UrbanService();
              var data = await client.Data(word);
              var pages = new List<string>();
              var embed = new EmbedBuilder();
            if (data.List.Length == 0)
            {
                await ReplyAsync($"I'm sorry but i didn't find `{word}` in the urban dictionary...");
                return;
            }
            embed.Color = new Color(200, 200, 0);
              foreach (var entry in data.List)
              {
                
                string result = $"[{entry.Word}]({entry.Permalink})\n\nDefinition: ";
                  string def = entry.Definition.Replace("[", "");
                result += def.Replace("]", "");
                result += $"\n\nExample: {entry.Example}";
                result += $"\n\n👍{entry.ThumbsUp}\t👎{entry.ThumbsDown}";
                if (result.Length >= 2048)
                {
                    var newresult = result.Take(2047).ToString();
                    embed.WithDescription(newresult);
                }
                else
                  embed.WithDescription(result);
              }
              
            await ReplyAsync("", false, embed.Build());

           
          }
        [Command("timezones"), Alias("worldclock")]
        [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task Worldclock()
        {
            CultureInfo enAU = new CultureInfo("en-US");
            string format = "HH':'mm', 'MMM dd";
            await Context.Channel.SendMessageAsync("", false,
                new EmbedBuilder()
                .AddField(":globe_with_meridians: UTC", DateTime.UtcNow.ToString(format, enAU), true)
                .AddField(":flag_fr: Paris", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Europe/Paris").ToString(format, enAU), true)
                //.AddField(":flag_in: Mumbai", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "India Standard Time").ToString(format, enAU), true)
                //.AddField(":flag_jp: Tokyo", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Tokyo Standard Time").ToString(format, enAU), true)
                //.AddField(":bridge_at_night: San Francisco", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time").ToString(format, enAU), true)
                //.AddField(":statue_of_liberty: New York", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Eastern Standard Time").ToString(format, enAU), true)
                .AddField(":flag_in: Mumbai", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Asia/Kolkata").ToString(format, enAU), true)
                .AddField(":flag_jp: Tokyo", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Asia/Tokyo").ToString(format, enAU), true)
                .AddField(":bridge_at_night: San Francisco", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/Los_Angeles").ToString(format, enAU), true)
                .AddField(":statue_of_liberty: New York", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/New_York").ToString(format, enAU), true)
                .Build()
                );
        }
        [Command("owo")]
        [Summary("hewwo uwu! owo your text here.")]
        [Ratelimit(7, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task OWO([Remainder] [Summary("The text you want to owo-ify")]string phrase)
        {
            String str = phrase.Replace("r", "w");
            str = str.Replace("l", "w");
            await ReplyAsync($"*" + str + " uwu*");
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

                await ReplyAsync("", false, embed.Build());
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
                embed.WithDescription("KillerBot is a multi-purpose bot with unique moderation, fun and utility commands that'll change how servers work to the better. \n \n**About my dev** \nI was made with :heart: by Panda#8822 with the help of some testers and support (<@238353818125991936> , <@333988268439764994> and others). I work on this bot as a hobby and I love to bring joy to my users by adding the features they want. \n \n**When was I created** \nKillerBot started back in 2016 and got hosted for a while until the hosting was stopped and the bot died in 2017. I then decided to bring it back and recode it completely in Discord.Net 2.0.1 and so I did, bringing KillerBot back at 03/03/2019 in a beta state until 05/23/2019 where it was officially released. And we're still going! <:Killerbot:587360915284819998> \n \n**Links** \n[Invite me!](https://discord.com/oauth2/authorize?client_id=263753726324375572&scope=bot&permissions=1480615958) \n[Discord Support Server](https://discord.gg/DNqAShq) \n[Donate!](https://www.patreon.com/KillerBot) \n------------- \n**Support the bot by upvoting it on these websites!** \n[Discord Bot List Website](https://discordbotlist.com/bots/263753726324375572) \n[Top.gg Website](https://discordbots.org/bot/263753726324375572) \n[Discord.bots.gg Website](https://discord.bots.gg/bots/263753726324375572)");
                embed.WithCurrentTimestamp();

                await ReplyAsync("", false, embed.Build());
            }
        }
        [Command("Tree")]
        [RequireContext(ContextType.Guild)]
        [Summary("Shows all categories in this guild and their children channels.")]
        //[Remarks("tree")]
        public async Task TreeAsync()
        {
            var uncategorized = new StringBuilder().AppendLine(Format.Bold("Uncategorized"));
            var categories = new StringBuilder();

            foreach (var c in Context.Guild.TextChannels
                .Where(c => c.CategoryId == null)
                .Cast<SocketGuildChannel>()
                .Concat(Context.Guild.VoiceChannels
                    .Where(a => a.CategoryId == null)).OrderBy(c => c.Position))
            {
                uncategorized.AppendLine($"- {(c is IVoiceChannel ? $"{c.Name}" : $"{c.Cast<ITextChannel>()?.Mention}")}");
            }

            uncategorized.AppendLine();
            foreach (var category in Context.Guild.CategoryChannels.OrderBy(x => x.Position))
            {
                var categoryBuilder = new StringBuilder().AppendLine($"{Format.Bold(category.Name)}");
                foreach (var child in category.Channels.OrderBy(c => c.Position))
                {
                    categoryBuilder.AppendLine($"- {(child is IVoiceChannel ? $"{child.Name}" : $"{child.Cast<ITextChannel>()?.Mention}")}");
                }
                categories.AppendLine(categoryBuilder.ToString());
            }

            var res = uncategorized.AppendLine(categories.ToString()).ToString();
            if (res.Length >= 2048)
            {
                throw new ArgumentException("This guild is too large; I cannot list all channels here");
            }
            await ReplyAsync(res);
        }
        [Command("clownrate")]
        [Alias("clown-rate", "howclown", "klownrate")]
        [Summary("Check the clown rate of yourself or others!")]
        [Ratelimit(6, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task ClownRate([Remainder] [Summary("OPTIONAL: The user you want to know their clown rate.")] SocketGuildUser user = null)
        {
            string clown = "";
            if (user == null)
            {
                clown = Context.User.ToString();
            }
            else
            {
                clown = user.ToString();
            }
            int rate = (Global.Rng.Next(0, 101)); //How clown you are!
            EmbedBuilder emb = new EmbedBuilder()
                .WithColor(new Color(255, 0, 0))
                .WithTitle("== Clown Rate 🤡 ==")
                .WithFooter($"Requested by: {Context.User}", Context.User.GetAvatarUrl());
            string reply = "";
            if (rate == 0)
            {
                reply = "not a clown! \nCongrats!";
                emb.WithImageUrl("https://cdn.discordapp.com/attachments/509781635089301505/701186408668201082/KBnowclown.gif");
            }
            else if (rate == 50)
            {
                reply = "half a clown.";
            }
            else if (rate == 100)
            {
                reply = "an expert CLOWN!";
                emb.WithImageUrl("https://media.giphy.com/media/14kwRD61ir8wW4/giphy.gif");
            }
            else if (rate < 50 && rate > 0)
            {
                reply = "still an amateur clown.";
            }
            else if (rate > 50 && rate < 100)
            {
                reply = "a senior CLOWN, almost there!";
            }
            if (clown == Context.User.ToString())
                emb.WithDescription($"\n \nYou are {reply} **({rate}%)**");
            else
                emb.WithDescription($"\n \n**{clown}** is {reply} **({rate}%)**");
            await ReplyAsync("", false, emb.Build());
        }

    }
}
