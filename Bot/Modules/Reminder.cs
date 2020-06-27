 using System;
using System.Globalization;
using System.Threading.Tasks;
using Bot.Entities;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;
using Bot.Helpers;
using Discord;
using Discord.Commands;

namespace Bot.Modules
{
    public class ReminderFormat
    {
        public static string[] Formats =
        {
            // Used to parse stuff like 1d14h2m11s and 1d 14h 2m 11s could add/remove more if needed

            "d'd'",
            "d'd'm'm'", "d'd 'm'm'",
            "d'd'h'h'", "d'd 'h'h'",
            "d'd'h'h's's'", "d'd 'h'h 's's'",
            "d'd'm'm's's'", "d'd 'm'm 's's'",
            "d'd'h'h'm'm'", "d'd 'h'h 'm'm'",
            "d'd'h'h'm'm's's'", "d'd 'h'h 'm'm 's's'",

            "h'h'",
            "h'h'm'm'", "h'h m'm'",
            "h'h'm'm's's'", "h'h 'm'm 's's'",
            "h'h's's'", "h'h s's'",
            "h'h'm'm'", "h'h 'm'm'",
            "h'h's's'", "h'h 's's'",

            "m'm'",
            "m'm's's'", "m'm 's's'",

            "s's'"
        };
    }

 
    [Group("Reminder"), Alias("Remind", "r", "alarm", "remindme")]
    [Summary("Tell the bot to remind you in some amount of time. The bot will send you a DM with the text you specified.")]
    public class Reminder : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalUserAccounts _globalUserAccounts;

        public Reminder(GlobalUserAccounts globalUserAccounts)
        {
            _globalUserAccounts = globalUserAccounts;
        }

        [Command(""), Priority(0), Remarks("Usage: {prefix}remind {thing to remind} in {time until I remind you} (__in__ is very important to be there.)")]
        [Summary("Adds a reminder.")]
        [Example("k!remind DO THE THING! :rage: in 2d 23h 3m 12s")]
        public async Task AddReminder([Summary("The thing you want the bot to remind you about, in (how much time till it sends the reminder)")][Remainder] string args = null)
        {
            if (args == null)
            {
                await ReplyAsync("<:KBfail:580129304592252995> No arguments were given! Please do `k!help remind` for more information.");
                return;
            }
            string[] splittedArgs = null;
            if (args.Contains(" in ")) splittedArgs = args.Split(new string[] {" in "}, StringSplitOptions.None);
             if (splittedArgs == null || splittedArgs.Length < 2)
            {
                await ReplyAsync("I think you need help on how to use the command.. don't you?\n" +
                                 "Let me REMIND you how to do so: `k!remind DO THE THING! :rage: in 2d 23h 3m 12s`\n" +
                                 "And the ` in ` before the time parameters is very important!");
                return;
            } 

            var timeString = splittedArgs[splittedArgs.Length - 1];
            if (timeString == "24h")
                timeString = "1d";

            splittedArgs[splittedArgs.Length - 1] = "";
            var reminderString = string.Join(" in ", splittedArgs, 0, splittedArgs.Length - 1);
            var timeDateTime = DateTime.UtcNow;
            try
            {
               timeDateTime = DateTime.UtcNow + TimeSpan.ParseExact(timeString, ReminderFormat.Formats, CultureInfo.CurrentCulture);
            }
            catch
            {
                await ReplyAsync("Please provide a valid time I can remind you in. Make sure its a positive 2 digit number. \nExample: `k!remind DO THE THING! :rage: in 2d 23h 3m 12s` \nNotes: The `in` before the time parameters is very important! and the time parameters can't be more than 2 digit numbers.");
                return;
            }
            var newReminder = new ReminderEntry(timeDateTime, reminderString);

            var account = _globalUserAccounts.GetById(Context.User.Id);

            account.Reminders.Add(newReminder);
            _globalUserAccounts.SaveAccounts(Context.User.Id);


            var timezone = account.TimeZone ?? "UTC";
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById($"{timezone}");
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(timeDateTime, tz);

            var bigmess2 =
                $"{reminderString}\n\n" +
                $"I will send you a DM in  __**{localTime}**__ `by {timezone}`\n";

            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.User);
            embed.WithCurrentTimestamp();
            embed.WithColor(Color.Blue);
            embed.WithTitle("I will remind you through DM:");
            embed.AddField($"**____**", $"{bigmess2}");

            ReplyAsync("", false, embed.Build());
        } 

        [Command("remindAt")]
        [Priority(1)]
        [Remarks("Add a reminder on a specific date and time (UTC/GMT+0)")]
        [Summary("How to use: e.g: `remind 2019-03-12 ANY_TEXT at 14:22` __at__ is very important (UTC time) to be there as well as the yyyy-mm-dd format. ")]
        public async Task AddReminderOn(string timeOn, [Remainder] string args)
        {
            string[] splittedArgs = { };
            if (args.ToLower().Contains("  at "))
                splittedArgs = args.ToLower().Split(new[] {"  at "}, StringSplitOptions.None);
            else if (args.ToLower().Contains(" at  "))
                splittedArgs = args.ToLower().Split(new[] {" at  "}, StringSplitOptions.None);
            else if (args.ToLower().Contains("  at  "))
                splittedArgs = args.ToLower().Split(new[] {"  at  "}, StringSplitOptions.None);
            else if (args.ToLower().Contains(" at "))
                splittedArgs = args.ToLower().Split(new[] {" at "}, StringSplitOptions.None);

            if (!DateTime.TryParse(timeOn, out var myDate)) //|| myDate < DateTime.Now
            {
                await ReplyAsync("Date input is not correct, you can try this `yyyy-mm-dd`");
                return;
            }

            if (splittedArgs == null)
            {
                await ReplyAsync("If you don't know how to use the command it should be like:\n" +
                                 "remind 2018-08-22 ANY_TEXT at 14:22`\n" +
                                 "And the ` in ` before the timeparameters is very important.");
                return;
            }

            var account = _globalUserAccounts.GetById(Context.User.Id);
            var timezone = account.TimeZone ?? "UTC";
            var tz = TimeZoneInfo.FindSystemTimeZoneById($"{timezone}");
            var timeString = splittedArgs[splittedArgs.Length - 1];

            splittedArgs[splittedArgs.Length - 1] = "";

            var reminderString = string.Join(" at ", splittedArgs, 0, splittedArgs.Length - 1);
            var hourTime = TimeSpan.ParseExact(timeString, "h\\:mm", CultureInfo.CurrentCulture);
            var timeDateTime = TimeZoneInfo.ConvertTimeToUtc(myDate + hourTime, tz);
            var newReminder = new ReminderEntry(timeDateTime, reminderString);

            account.Reminders.Add(newReminder);
            _globalUserAccounts.SaveAccounts(Context.User.Id);

            var bigmess2 =
                $"{reminderString}\n\n" +
                $"I will send you a DM in  __**{myDate + hourTime}**__ `by {timezone}`\n";

            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.User);
            embed.WithCurrentTimestamp();
            embed.WithColor(Color.Blue);
            embed.WithTitle("I will remind you through DM:");
            embed.AddField($"**____**", $"{bigmess2}");
            ReplyAsync("", false, embed.Build());
        }



        [Command("List"), Priority(2), Summary("List all your reminders")]
        public async Task ShowReminders()
        {
            var reminders = _globalUserAccounts.GetById(Context.User.Id).Reminders;
            var embB = new EmbedBuilder()
                .WithTitle("Your Reminders (Times are in UTC / GMT+0)")
                //.WithFooter("Did you know? " + Global.GetRandomDidYouKnow())
                .WithDescription("To delete a reminder use the command `{prefix}reminder remove <number>` " +
                                 "and the number is the one to the left of the Dates inside the [].");

            for (var i = 0; i < reminders.Count; i++)
            {
                embB.AddField($"[{i+1}] {reminders[i].DueDate:f}", reminders[i].Description, true);
            }

            await ReplyAsync("", false, embB.Build(), null);
        }

        [Command("Remove"), Priority(2), Summary("Delete one of your reminders")]
        public async Task DeleteReminder(int index)
        {
            var reminders = _globalUserAccounts.GetById(Context.User.Id).Reminders;
            var responseString = "Reminder doesn't exist, maybe use `{prefix}remind list` before you try to " +
                                 "delete a reminder.";
            if (index > 0 && index <= reminders.Count)
            {
                reminders.RemoveAt(index - 1);
                _globalUserAccounts.SaveAccounts(Context.User.Id);
                responseString = $"Deleted the reminder with index {index}!";
            }

            await ReplyAsync(responseString);
        }
    }
} 
