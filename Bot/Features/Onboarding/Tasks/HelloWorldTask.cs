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
                await logger.Log(LogSeverity.Warning, "Onboarding > HelloWorldTask", $"Default channel of a new guild ({guild.Name}) is null.");
                return;
            }

            if (user.GetPermissions(defaultChannel).Has(ChannelPermission.SendMessages) && user.GetPermissions(defaultChannel).Has(ChannelPermission.EmbedLinks) && user.GetPermissions(defaultChannel).Has(ChannelPermission.ViewChannel))
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(new Color(0, 255, 0))
                    .WithTitle("Thanks For Adding KillerBot!")
                    .WithThumbnailUrl("https://cdn.discordapp.com/avatars/263753726324375572/fcd725df065227cd3e8a328b292cb3c9.png?size=1024")
                    .WithFooter(" KillerBot Dev Team", "https://cdn.discordapp.com/avatars/263753726324375572/fcd725df065227cd3e8a328b292cb3c9.png?size=1024")
                    .WithDescription("<:authorized:603396459978555393> Thanks for adding KillerBot to the server! I'm a multi-purpose Discord bot with a variation in commands between moderation, fun, and utility! \n \nYou can find the commands you can use by doing `k!help` or just `@killerbot help` if you don't want to use that prefix. \n \nIf you need help with a specific command you can also do `k!help [module]` and you'll get more information about it. And if you need any support or having difficulties just join our [Support Server](https://discord.gg/DNqAShq) <:support:603396476663758870> \n \nAlright, i hope you have fun and enjoy the features i have! \nPeace :wave: :smile:");
                await defaultChannel.SendMessageAsync("", false, emb.Build());
                
            }
            else
                await logger.Log(LogSeverity.Error, "Onboarding > HelloWordTask", $"Missing permissions to send message. ({guild.Name})");
            return;
        }
    }
}
