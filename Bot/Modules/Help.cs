using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Bot.Common;
using Bot.Entities;
using Bot.Handlers;
using Bot.Helpers;
using Bot.Preconditions;

namespace Bot.Modules
{
	public class Help : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - The two help commands
		// Contributors     - Creepysin, 

		private readonly CommandService _cmdService;
		private readonly CommandHandler _commandHandler;

		public Help(CommandService commandService, CommandHandler cmdHandler)
		{
			_cmdService = commandService;
			_commandHandler = cmdHandler;
		}

		[Command("help")]
		[Alias("h")]
		[Summary("Gets help")]
		public async Task HelpCmd()
		{
			try
			{
                EmbedBuilder emb = new EmbedBuilder();
                emb.WithColor(Color.Teal);
                emb.WithTitle("KillerBot Commands");
                emb.WithDescription($"For more help on a specific command do `{Global.BotPrefix}help [command]`");
				//StringBuilder builder = new StringBuilder();
				//builder.Append(
					//$"```# KillerBot Normal Commands```\nFor more help on a specific command do `{Global.BotPrefix}help [command]`.\n");

				//Basic Commands
				foreach (HelpModule helpModule in HelpModulesManager.GetHelpModules())
				{
                    emb.Description += $"\n\n**{helpModule.Group}** - ";
					//builder.Append($"\n**{helpModule.Group}** - ");

					foreach (CommandInfo cmd in helpModule.Modules.SelectMany(module => _commandHandler.GetModule(module).Commands))
					{
                        if (cmd.Module.Group != null || (cmd.Module.Group != null && cmd.Name == ""))
                        {
                            emb.Description += $"`{cmd.Module.Group} {cmd.Name}`, ";
                        }
                        else if (emb.Description.Contains(cmd.Name))
                        {
                            if (cmd.Name == "minecraft" || cmd.Name == "help")
                            {
                                emb.Description += $"`{cmd.Name}`, ";
                            }
                            else
                            continue;
                        }
                        else
                        {
                            emb.Description += $"`{cmd.Name}`, ";
                        }
                        //builder.Append($"`{cmd.Name}` ");
                    }
				}
                await Context.Channel.SendMessageAsync("", false, emb.Build());
                //await Context.Channel.SendMessageAsync(builder.ToString());
            }
			catch (NullReferenceException)
			{
				await Context.Channel.SendMessageAsync(
					$"Sorry, but it looks like the bot owner doesn't have the help options configured correctly. \n`k!report (this problem)` to let the bot owner know about this!");

				Global.Log("The help options are configured incorrectly!", ConsoleColor.Red);
			}
		}

        /*[Command("help")]
		[Alias("h", "command", "chelp", "ch")]
		[Summary("Gets help on a specific command")]
		public async Task HelpSpecific([Remainder] string query)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"Help for {query}");
			embed.WithColor(new Color(241, 196, 15));

			SearchResult result = _cmdService.Search(Context, query);
            if (result.IsSuccess)
                foreach (CommandMatch command in result.Commands)
                {
                    ExampleAttribute example = command.Command.Attributes.OfType<ExampleAttribute>().FirstOrDefault();
                    embed.AddField(command.Command.Name,
                        $"Summary: {command.Command.Summary}\nAlias: {FormatAliases(command.Command)}\nUsage: `{command.Command.Name} {FormatParms(command.Command)}` \nExample: {example.ExampleText}");
                }
			if (embed.Fields.Count == 0)
				embed.WithDescription("Nothing was found for " + query);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
		} */
        [Command("help"), Alias("h")]
        [Remarks("Shows what a specific command or module does and what parameters it takes.")]
        [Cooldown(5)]
        public async Task HelpQuery([Remainder] string query)
        {
            var builder = new EmbedBuilder();

            builder.Color = new Color(241, 196, 15);
            builder.Title = $"Help for '{query}'";
            builder.WithFooter($"Requested by {Context.User}", Context.User.GetAvatarUrl());
            builder.WithCurrentTimestamp();

            var result = _cmdService.Search(Context, query);
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
                ExampleAttribute example = cmd.Attributes.OfType<ExampleAttribute>().FirstOrDefault();
                if (example != null && !example.ExampleText.IsEmpty())
                {
                    paramsString += $"\nExample: {example.ExampleText}";
                }

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
            var module = _cmdService.Modules.ToList().Find(mod =>
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

        private string FormatAliases(CommandInfo commandInfo)
		{
			IReadOnlyList<string> aliases = commandInfo.Aliases;

			StringBuilder format = new StringBuilder();

			int count = aliases.Count;
			int currentCount = 1;
			foreach (string alias in aliases)
			{
				format.Append(alias);

				if (currentCount != count) format.Append(", ");
				currentCount += 1;
			}

			return format.ToString();
		}

		private string FormatParms(CommandInfo commandInfo)
		{
			IReadOnlyList<ParameterInfo> parms = commandInfo.Parameters;

			StringBuilder format = new StringBuilder();
			int count = parms.Count;
			if (count != 0) format.Append("[");
			int currentCount = 1;
			foreach (ParameterInfo parm in parms)
			{
				format.Append(parm);

				if (currentCount != count) format.Append(", ");
				currentCount += 1;
			}

			if (count != 0) format.Append("]");

			return format.ToString();
		}
	}
}