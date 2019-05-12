using Discord;

namespace Bot.Features.Onboarding
{
    public interface IOnboarding
    {
        void JoinedGuild(IGuild guild);
    }
}
