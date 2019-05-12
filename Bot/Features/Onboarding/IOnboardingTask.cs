using Discord;

namespace Bot.Features.Onboarding
{
    public interface IOnboardingTask
    {
        void OnJoined(IGuild guild);
    }
}
