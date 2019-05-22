using System.Threading.Tasks;
using Bot.Extensions;
using Discord;
using Discord.Commands;


namespace Bot.Modules
{
    [Group("Tasks"), Remarks("Settings for the repeated task that run in the background")] 
    [Alias("Task", "T")]
    public class RepeatedTasks : ModuleBase<MiunieCommandContext>
    {
        [Command("")]
        [Alias("List", "L")]
        [RequireOwner]
        public Task ListTasks()
        {
            var embBuilder = new EmbedBuilder();
            embBuilder.WithAuthor("Here are all registered repeated Tasks");
            foreach (var timer in Global.TaskHander.Timers)
            {
                var enabled = timer.Value.Enabled ? "ENABLED" : "DISABLED";
                embBuilder.AddField($"{timer.Key} [{ enabled}]", $"{timer.Value.Interval / 1000}s interval.", true);
            }

            return ReplyAsync("", false, embBuilder.Build(), null);
        }

        [Command("Start")]
        [RequireOwner]
        public async Task StartTask([Remainder] string name)
        {
            var success = Global.TaskHander.StartTimer(name);
            var msgString = success ? $"{name} has been started!" : $"{name} is not a task that already exists...";
            await ReplyAsync(msgString);
        }

        [Command("Interval")]
        [RequireOwner]
        public async Task ChangeIntervalOfTask(int interval, [Remainder] string name)
        {
            var success = Global.TaskHander.ChangeInterval(name, interval);
            var msgString = success ? $"Interval of {name} has been set to {interval/1000} seconds!" : $"{name} is not a task that already exists or the interval you tried to set was too low ({Constants.MinTimerIntervall}ms is the lowest)...";
            await ReplyAsync(msgString);
        }

        [Command("Stop")]
        [RequireOwner]
        public async Task StopTask([Remainder] string name)
        {
            var success = Global.TaskHander.StopTimer(name);
            var msgString = success ? $"{name} has been stopped!" : $"{name} is not a task that already exists...";
            await ReplyAsync(msgString);
        }

    }
}  
