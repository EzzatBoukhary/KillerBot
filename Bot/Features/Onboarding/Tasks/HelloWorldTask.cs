using Discord;

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
            
            if(defaultChannel is null)
            {
                await logger.Log(LogSeverity.Error, "Onboarding > HelloWorldTask", $"Default channel of a new guild ({guild.Name}) is null.");
                return;
            }
            
            await defaultChannel.SendMessageAsync(":wave: Thanks for adding KillerBot! Do `@KillerBot#5438 help` to know the commands you can use :)");
        }
    }
}
