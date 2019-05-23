using Discord;
using Discord.WebSocket;
using System;

namespace Bot.Features.Onboarding.Tasks
{
    public class HelloWorldTask : IOnboardingTask
    {
        private readonly Logger logger;
     
        public HelloWorldTask(Logger logger)
        {
            this.logger = logger;
          
        }

        public async void OnJoined(IGuild guild)
        {
            var defaultChannel = await guild.GetDefaultChannelAsync();
            var user = await guild.GetCurrentUserAsync();

            if (defaultChannel is null)
            {
                await logger.Log(LogSeverity.Error, "Onboarding > HelloWorldTask", $"Default channel of a new guild ({guild.Name}) is null.");
                return;
            }

            if (user.GetPermissions(defaultChannel).Has(ChannelPermission.SendMessages))
            {
                await defaultChannel.SendMessageAsync(":wave: Thanks for adding KillerBot! Do `@KillerBot#5438 help` to know the commands you can use :)");
                
            }
            else
                await logger.Log(LogSeverity.Error, "Onboarding > HelloWordTask", $"Missing permissions to send message. ({guild.Name})");
            return;
        }
    }
}
