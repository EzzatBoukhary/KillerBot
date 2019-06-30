namespace Bot.Features.Economy
{
    public interface IMiuniesTransfer
    {
        void UserToUser(ulong sourceUserId, ulong targetUserId, long amount);
    }
}