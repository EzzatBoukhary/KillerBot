using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bot.Features.Onboarding.Tasks;
using Discord;

namespace Bot.Features.Onboarding
{
    public class Onboarding : IOnboarding
    {
        private readonly IEnumerable<IOnboardingTask> tasks;

        public Onboarding(IServiceProvider serviceProvider)
        {
            tasks = GetOnboardingTasks(serviceProvider);
        }

        public void JoinedGuild(IGuild guild)
        {
            foreach(var task in tasks)
            {
                task.OnJoined(guild);
            }
        }

        private static IEnumerable<IOnboardingTask> GetOnboardingTasks(IServiceProvider serviceProvider)
        {
            var taskType = typeof(IOnboardingTask);

            // Loops through Assemblies and types within them
            // and takes the ones that implement the task interface
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => taskType.IsAssignableFrom(p) && !p.IsInterface)
                .Select(t => (IOnboardingTask)serviceProvider.GetService(t));
        }
    }
}
