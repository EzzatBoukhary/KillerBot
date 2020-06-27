using Bot.Common;
using Bot.Entities;
using Bot.Handlers;
using Bot.Helpers;
using Bot.Preconditions;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Modules
{
    [Summary("Commands for listing available commands")]
    [Name("Help")]
    public class Help : InteractiveBase
    {
        private static readonly Color EMBED_COLOR = Color.DarkGreen;
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandService _commandService;
        private readonly CommandHandler _commandHandler;

        public Help(CommandService commandService, IServiceProvider serviceProvider, CommandHandler cmdHandler)
        {
            _serviceProvider = serviceProvider;
            _commandService = commandService;
            _commandHandler = cmdHandler;
        }

        [Summary("Show all available commands")]
        [Command("help")]
        public async Task HelpCommand()
        {

            List<ModuleInfo> modules = _commandService.Modules.ToList();
            var moduleCommands = new Dictionary<string, List<CommandInfo>>();

            foreach (ModuleInfo module in modules)
            {
                moduleCommands.Add(module.Name, new List<CommandInfo>());
            }
            foreach (CommandInfo command in _commandService.Commands)
            {
                moduleCommands[command.Module.Name].Add(command);
            }
            EmbedBuilder emb = new EmbedBuilder();
            List<PaginatedMessage.Page> pages = new List<PaginatedMessage.Page>();
            pages.Add(new PaginatedMessage.Page
            {
                Author = new EmbedAuthorBuilder { Name = "KillerBot Commands" },
                Color = Color.Blue,
                Description = $"Use the reactions to navigate through pages. For more help on a specific command do `{Global.BotPrefix}help [command]`" +
                "\n \n**Moderation Commands:** \nReact with :1234: then send `2` in chat." +
                "\n \n**Basic Commands:** \nReact with :1234: then send `3` in chat." +
                "\n \n**Utility Commands:** \nReact with :1234: then send `4` in chat." +
                "\n \n**Economy Commands:** \nReact with :1234: then send `5` in chat." +
                "\n \n**Fun Commands:** \nReact with :1234: then send `6` in chat." +
                "\n \n**Bot Owner Commands: (ONLY)** \nReact with :1234: then send `7` in chat."
            });
            foreach (HelpModule helpModule in HelpModulesManager.GetHelpModules())
            {
                emb.Description = "Command Usage: `k!(command name)`. You can do `k!help (command name)` for more info on a specific command. \n \n";
                foreach (CommandInfo command in helpModule.Modules.SelectMany(module => _commandHandler.GetModule(module).Commands))
                {
                    string aliases;
                    if (command.Aliases != null)
                        aliases = string.Join(", ", command.Aliases);
                    else
                        aliases = "None";

                    if (command.Module.Group != null || (command.Module.Group != null && command.Name == ""))
                    {
                        emb.Description += $"`{command.Module.Group} {command.Name}` , ";
                    }
                    else if (emb.Description.Contains(command.Name))
                    {
                        if (command.Name == "minecraft" || command.Name == "help" || command.Name == "info" || command.Name == "move")
                        {
                            emb.Description += $"`{command.Name}` , ";
                        }
                        else
                            continue;
                    }
                    else
                    {
                        emb.Description += $"`{command.Name}` , ";
                    }
                }
                pages.Add(new PaginatedMessage.Page
                {
                    Author = new EmbedAuthorBuilder { Name = helpModule.Group },
                    Description = emb.Description
                });
                emb.Description = "";
            }
            string message = Context.User.Mention;
            await PostHelpPages(message, pages);
        }

        private async Task PostHelpPages(string message, List<PaginatedMessage.Page> pages)
        {
            var pager = new PaginatedMessage
            {
                Pages = pages,
                Color = EMBED_COLOR,
                Content = message,
                FooterOverride = null,
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Options = PaginatedAppearanceOptions.Default,
                TimeStamp = DateTimeOffset.UtcNow
            };
            await PagedReplyAsync(pager, new ReactionList
            {
                Forward = true,
                Backward = true,
                Jump = true,
                First = true,
                Last = true,
                Trash = true
            });
        }
        [Command("help"), Alias("h")]
        [Remarks("Shows what a specific command or module does and what parameters it takes.")]
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        public async Task HelpQuery([Remainder] string query)
        {
            var builder = new EmbedBuilder();

            builder.Color = new Color(241, 196, 15);
            builder.Title = $"Help for '{query}'";
            builder.WithFooter($"Requested by {Context.User}", Context.User.GetAvatarUrl());
            builder.WithCurrentTimestamp();

            var result = _commandService.Search(Context, query);
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
                var paramsString = ((cmd.Parameters.Count == 0) ? "" : $"Parameters: {string.Join(", ", parameters)}") +
                                   (string.IsNullOrEmpty(cmd.Remarks) ? "" : $"\nRemarks: {cmd.Remarks}") +
                                   (string.IsNullOrEmpty(cmd.Summary) ? "" : $"\nSummary: {cmd.Summary}");
                ExampleAttribute example = cmd.Attributes.OfType<ExampleAttribute>().FirstOrDefault();
                if (example != null && !example.ExampleText.IsEmpty())
                {
                    paramsString += $"\nExample: {example.ExampleText}";
                }
                if (paramsString == "")
                    paramsString = "There is no information about this command.";
                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"{paramsString}";
                    x.IsInline = false;
                });
            }

            return builder.Build();
        }

        private async Task<Embed> HelpModule(string moduleName, EmbedBuilder builder)
        {
            var module = _commandService.Modules.ToList().Find(mod =>
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
    }
}
