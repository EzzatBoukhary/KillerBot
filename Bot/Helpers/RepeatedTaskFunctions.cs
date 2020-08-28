using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Bot.Entities;
using Bot.Features.GlobalAccounts;
using Discord.WebSocket;

namespace Bot.Helpers
{

    public class RepeatedTaskFunctions
    {
        private readonly GlobalUserAccounts _globalUserAccounts;
        private readonly DiscordSocketClient _client;
        public RepeatedTaskFunctions(GlobalUserAccounts globalUserAccounts, DiscordSocketClient client)
        {
            _globalUserAccounts = globalUserAccounts;
            _client = client;
        }

        public Task InitRepeatedTasks()
        {
            // Look for expired reminders every 3 seconds
            Global.TaskHander.AddRepeatedTask("Reminders", 3000, new ElapsedEventHandler(CheckReminders));
            //GameStatus every 1 hour
            Global.TaskHander.AddRepeatedTask("Servers", 3600000, new ElapsedEventHandler(CheckServerCount));
            // Help Message every 2 hours
            Global.TaskHander.AddRepeatedTask("Help Message", 7200000, new ElapsedEventHandler(SendHelpMessage));
            return Task.CompletedTask;
            
        }

        private static async void SendHelpMessage(object sender, ElapsedEventArgs e)
        {
            var general = Global.Client.GetChannel(403278466746810370) as SocketTextChannel;
            general?.SendMessageAsync("hmm");
        }

        private async void CheckReminders(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.UtcNow;
            // Get all accounts that have at least one reminder that needs to be sent out
            var accounts = _globalUserAccounts.GetFilteredAccounts(acc => acc.Reminders.Any(rem => rem.DueDate < now));
            foreach (var account in accounts)
            {
                var guildUser = Global.Client.GetUser(account.Id);
                var dmChannel = await guildUser?.GetOrCreateDMChannelAsync();
                if (dmChannel == null) return;

                var toBeRemoved = new List<ReminderEntry>();

                foreach (var reminder in account.Reminders)
                {
                    if (reminder.DueDate >= now) continue;
                    await dmChannel.SendMessageAsync(":alarm_clock: Reminder: " + reminder.Description + $"\n(Original Message: <{reminder.MsgURL}>)");
                    // Usage of a second list because trying to use 
                    // accountReminders.Remove(reminder) would break the foreach loop
                    toBeRemoved.Add(reminder);
                }
                // Remove all elements that needs to be removed
                toBeRemoved.ForEach(remRem => account.Reminders.Remove(remRem));
                _globalUserAccounts.SaveAccounts(account.Id);
            }
        }
        private async void CheckServerCount(object sender, ElapsedEventArgs e)
        {

            _client.Guilds.Count();
        }
    }
}
