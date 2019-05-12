using Bot.Extensions;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Bot.Modules
{
    public class CommandInfoFileGenerator : ModuleBase<MiunieCommandContext>
    {
        private readonly CommandService _service;

        public CommandInfoFileGenerator(CommandService service)
        {
            _service = service;
        }

        [Command("commandInfo"), Alias("c")]
        public async Task CommandInfo()
        {
            var file = "commands.txt";
            var builder = new StringBuilder();
            builder.Append($"Help\n---\nThese are the commands you can use \n<br/><br/>\n");
            var moduleBuilder = new StringBuilder();
            foreach (var module in _service.Modules)
            {
                await AddModuleString(module, moduleBuilder).ConfigureAwait(false);
                builder.Append($"[{module.Name}](#{module.Name.ToLower().Replace(' ', '-')})<br/>\n");
            }
            builder.Append($"\n<br/><br/>\n{moduleBuilder}");
            File.WriteAllText(file, builder.ToString());
            await ReplyAsync($"Wrote command info into '{file}'");
        }

        private async Task AddModuleString(ModuleInfo module, StringBuilder builder)
        {
            if (module is null) { return; }
            var descriptionBuilder = new List<string>();
            var duplicateChecker = new List<string>();
            descriptionBuilder.Add("\n\n| Command | Description | Remarks |\n| --- | --- | --- |");
            foreach (var cmd in module.Commands)
            {
                var result = await cmd.CheckPreconditionsAsync(Context);
                if (!result.IsSuccess || duplicateChecker.Contains(cmd.Aliases.First())) { continue; }
                duplicateChecker.Add(cmd.Aliases.First());

                var cmdDescription = $"| `{cmd.Aliases.First()}` | {cmd.Summary} | {cmd.Remarks} |";
                descriptionBuilder.Add($"{cmdDescription.Replace("\n", "<br/>")} |");
            }
            var builtString = string.Join("\n", descriptionBuilder);

            var moduleNotes = $"{module.Summary}";
            if (!string.IsNullOrEmpty(module.Summary) && !string.IsNullOrEmpty(module.Remarks))
            {
                moduleNotes += " | ";
            }
            moduleNotes += $"{module.Remarks}";

            if (!string.IsNullOrEmpty(moduleNotes))
            {
                moduleNotes += "<br/>";
            }
            builder.Append($"### {module.Name}\n\n{moduleNotes}\n{builtString}\n<br/>\n\n");
        }
    }
}
