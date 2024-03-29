﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bot.Extensions;
using Discord.Commands;
using Discord.WebSocket;
using Bot.Features.GlobalAccounts;
using Bot.Providers;
using Discord;
using System.Collections.Generic;
using Bot.Common;
using Bot.Entities;

namespace Bot.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;
        private readonly IServiceProvider _serviceProvider;
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        private readonly GlobalUserAccounts _globalUserAccounts;
        private readonly RoleByPhraseProvider _roleByPhraseProvider;
        private readonly Logger _logger;

        public CommandHandler(DiscordSocketClient client, CommandService cmdService, IServiceProvider serviceProvider, GlobalGuildAccounts globalGuildAccounts, GlobalUserAccounts globalUserAccounts, RoleByPhraseProvider roleByPhraseProvider, Logger logger)
        {
            _client = client;
            _cmdService = cmdService;
            _serviceProvider = serviceProvider;
            _globalGuildAccounts = globalGuildAccounts;
            _globalUserAccounts = globalUserAccounts;
            _roleByPhraseProvider = roleByPhraseProvider;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            Global.Client = _client;
            _client.MessageReceived += CheckForDM;
        }
        /// <summary>
		/// Install all the modules
		/// </summary>
		/// <returns></returns>
		public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        /// <summary>
        /// Checks all the help modules in the config
        /// </summary>
       /* public void CheckHelpModules()
        {
            foreach (string module in HelpModulesManager.GetHelpModules().SelectMany(helpModule => helpModule.Modules.Where(module => GetModule(module) == null)))
            {
                Global.Log($"There is no module called {module}! Reset the help modules or fix the help modules in the config file!", ConsoleColor.Red);
            }
        }*/
         public async Task HandleCommandAsync(SocketMessage s)
         {
             if (!(s is SocketUserMessage msg)) { return; }
             //if (msg.Channel is SocketDMChannel) { return; }
             if (msg.Author.IsBot) { return; }
            var context = new MiunieCommandContext(_client, msg, _globalUserAccounts);
            if (!(msg.Channel is SocketDMChannel))
            {
                try
                {
                    var user = _globalUserAccounts.GetById(msg.Author.Id);
                    var guild = _globalGuildAccounts.GetById(context.Guild.Id);
                    if (user.Blacklisted == true || guild.Blacklisted == true) { return; }
                }
                catch
                {
                    await _logger.Log(LogSeverity.Critical, "CommandHandler.cs", "User/Guild.json not found.");
                }
                await _roleByPhraseProvider.EvaluateMessage(
                     context.Guild,
                     context.Channel,
                     context.Message.Content,
                     (SocketGuildUser)context.User
                 );
            }
             var argPos = 0;
             if (msg.HasStringPrefix("k!", ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos)
                || CheckPrefix(ref argPos, context))
             {
                 var prefix = "k!";
                 var cmdSearchResult = _cmdService.Search(context, argPos);
                CommandMatch cmd = cmd;
                if (!cmdSearchResult.IsSuccess) { return; }
                foreach (var cmds in cmdSearchResult.Commands)
                {
                    if (cmdSearchResult.IsSuccess)
                    {
                        cmd = cmds;
                    }
                }

                 context.RegisterCommandUsage();

                 var executionTask = _cmdService.ExecuteAsync(context, argPos, _serviceProvider);

 #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                 executionTask.ContinueWith(task =>
                 {
                     if (!task.Result.IsSuccess && (task.Result.Error == CommandError.Unsuccessful || task.Result.ErrorReason.Contains("Cooldown")))
                     {
                         var guild = "";
                         if (msg.Channel is SocketDMChannel)
                         {
                             guild = "DMs";
                         }
                         else
                             guild = $"{context.Guild.Id}";
                         _logger.Log(LogSeverity.Error, $"Guild:{guild} - User:{context.User.Id}", task.Result.ErrorReason);
                     }
                     var command = "";
                     if (cmd.Command.Module.Group != null || (cmd.Command.Module.Group != null && cmd.Command.Name == ""))
                     {
                         command = $"{cmd.Command.Module.Group} {cmd.Command.Name}";
                     }
                     else
                     {
                         command = cmd.Command.Name;
                     }
                     if (task.Result.IsSuccess == true) { return; }
                     /*else if (task.Result.Error == CommandError.UnknownCommand)
                     {
                         var poss = GetPossibleCommands(context);
                         EmbedBuilder errormsg = new EmbedBuilder();
                         errormsg.WithColor(Color.Red);
                         errormsg.WithCurrentTimestamp();
                         errormsg.WithTitle("Unknown Command");
                         errormsg.WithDescription(poss);
                         context.Channel.SendMessageAsync("", false, errormsg.Build());
                         return;
                     }*/
                     else if (task.Result.Error == CommandError.BadArgCount)
                     {
                         EmbedBuilder errormsg = new EmbedBuilder();
                         errormsg.WithColor(Color.Red);
                         errormsg.WithCurrentTimestamp();
                         errormsg.WithTitle("Bad Usage:");
                         errormsg.WithDescription($"Command did not have the right amount of parameters. \nType `{prefix}help {command}` for more info.");
                         context.Channel.SendMessageAsync("", false, errormsg.Build());
                         return;
                     }
                     else if (task.Result.Error == CommandError.UnmetPrecondition)
                     {
                         EmbedBuilder errormsg = new EmbedBuilder();
                         errormsg.WithColor(Color.Red);
                         errormsg.WithCurrentTimestamp();
                         errormsg.WithTitle("Unmet Precondition");
                         errormsg.WithDescription($"**{task.Result.ErrorReason}** \nA precondition for the command was not met. Type `{prefix}help {command}` for more info.");
                         context.Channel.SendMessageAsync("", false, errormsg.Build());
                         return;
                     }
                     else if (task.Result.Error == CommandError.Unsuccessful)
                     {
                         EmbedBuilder errormsg = new EmbedBuilder();
                         errormsg.WithColor(Color.Red);
                         errormsg.WithCurrentTimestamp();
                         errormsg.WithTitle("Unsuccessful:");
                         errormsg.WithDescription($"The command excecution was unsuccessfull, this was automatically reported to the KillerBot Team.");
                         context.Channel.SendMessageAsync("", false, errormsg.Build());
                         return;
                     }
                     else if (task.Result.Error == CommandError.ParseFailed)
                     {
                         EmbedBuilder errormsg = new EmbedBuilder();
                         errormsg.WithColor(Color.Red);
                         errormsg.WithCurrentTimestamp();
                         errormsg.WithTitle("Parse Failed:");
                         errormsg.WithDescription($"Command could not be parsed. \nMake sure to do `{prefix}help {command}` for more info on the command's usage.");
                         context.Channel.SendMessageAsync("", false, errormsg.Build());
                         return;
                     }
                     else if (task.Result.Error == CommandError.MultipleMatches)
                     {
                         EmbedBuilder errormsg = new EmbedBuilder();
                         errormsg.WithColor(Color.Red);
                         errormsg.WithCurrentTimestamp();
                         errormsg.WithTitle("Multiple Matches");
                         errormsg.WithDescription($"Multiple matches found. Please be more specific.");
                         context.Channel.SendMessageAsync("", false, errormsg.Build());
                         return;
                     }
                     else if (task.Result.Error == CommandError.ObjectNotFound)
                     {
                         EmbedBuilder errormsg = new EmbedBuilder();
                         errormsg.WithColor(Color.Red);
                         errormsg.WithCurrentTimestamp();
                         errormsg.WithTitle("Object Reference:");
                         errormsg.WithDescription($"**{task.Result.ErrorReason}**");
                         context.Channel.SendMessageAsync("", false, errormsg.Build());
                         return;
                     }
                     else
                     {
                         const string errTemplate = "{0}, Error: {1}.";
                         var errMessage = string.Format(errTemplate, context.User.Mention, task.Result.ErrorReason);
                         context.Channel.SendMessageAsync(errMessage);
                     }
                 });
                 // Because this call is not awaited, execution of the current method continues before the call is completed
             }
         }
        /// <summary>
        /// Checks all the help modules in the config
        /// </summary>
        public void CheckHelpModules()
        {
            foreach (string module in HelpModulesManager.GetHelpModules().SelectMany(helpModule => helpModule.Modules.Where(module => GetModule(module) == null)))
            {
                Global.Log($"There is no module called {module}! Reset the help modules or fix the help modules in the config file!", ConsoleColor.Red);
            }
        }
        /// <summary>
        /// Get a modules
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public ModuleInfo GetModule(string moduleName)
        {
            IEnumerable<ModuleInfo> result = from a in _cmdService.Modules
                                             where a.Name == moduleName
                                             select a;

            ModuleInfo module = result.FirstOrDefault();
            return module;
        }
    
    public async Task CheckForDM(SocketMessage parameterMessage)
        {
            var channel = _client.GetChannel(587746737175789585) as SocketTextChannel;
            var message = parameterMessage as SocketUserMessage;
            int argPos = 0;
            if ((message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix("k!", ref argPos))) return;

            if (message.Channel is IDMChannel && message.Author != _client.CurrentUser)
            {
                var info = await _client.GetApplicationInfoAsync();
                var emb = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTitle($"New DM: ");
                if (message.Content.ToString().Contains("discord.gg/"))
                {
                    emb.WithDescription($"<@{message.Author.Id}>: [Message contains a server invite link]");
                }
                else if (message.Content.ToString() == null || message.Content.Length == 0)
                    emb.WithDescription($"<@{message.Author.Id}>: [Message was an embed]");

                else
                {
                    emb.WithDescription($"<@{message.Author.Id}>: {message.Content}");
                    if (message.Attachments.Count != 0)
                    {
                        emb.Description += "\n \n**Attachments:** \n";
                        int number = 0;
                        foreach (var attachment in message.Attachments)
                        {
                            number += 1;
                            emb.Description += $"[Attachment: {number}]({attachment.Url})\n";
                            emb.WithImageUrl(attachment.Url);
                        }

                    }
                }

                emb.WithTimestamp(message.Timestamp)
                .WithFooter($"From: {message.Author}", message.Author.GetAvatarUrl());
                await channel.SendMessageAsync("", false, emb.Build());
            }
        }

        private bool CheckPrefix(ref int argPos, SocketCommandContext context)
        {
            if (context.Guild is null) return false;
            var prefixes = _globalGuildAccounts.GetById(context.Guild.Id).Prefixes;
            var tmpArgPos = 0;
            var success = prefixes.Any(pre =>
            {
                if (!context.Message.Content.StartsWith(pre)) return false;
                tmpArgPos = pre.Length;
                return true;
            });
            argPos = tmpArgPos;
            return success;
        }

        /*private string GetPossibleCommands(SocketCommandContext context)
        {
            var prefix = "k!";
            var commandText = context.Message.ToString();
            List<string> possibleCommands =
                        _cmdService
                            .Modules
                            .SelectMany(module => module.Commands)
                            .SelectMany(command => command.Aliases.Select(a => $"{prefix}{a}"))
                            .Distinct()
                            .Where(alias => alias.Contains(commandText, StringComparison.OrdinalIgnoreCase))
                            .ToList();

            string message = "";
            if (possibleCommands == null || possibleCommands.Count < 1)
            {
                message = $"Command *{commandText}* was not found. Type {prefix}help to get a list of commands";
            }
            else if (possibleCommands.Count == 1)
            {
                message = $"Did you mean *{possibleCommands.First()}* ? Type {prefix}help to get a list of commands";
            }
            else if (possibleCommands.Count > 1 && possibleCommands.Count < 5)
            {
                message = $"Did you mean one of the following commands:{Environment.NewLine}{string.Join(Environment.NewLine, possibleCommands)}{Environment.NewLine}Type {prefix}help to get a list of commands";
            }
            else
            {
                message = $"{possibleCommands.Count} possible commands have been found matching your input. Please be more specific.";
            }
            return message;
        }*/

    }
}

