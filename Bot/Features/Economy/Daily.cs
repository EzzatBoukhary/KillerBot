using System;
using System.Collections.Generic;
using Bot.Entities;
using Bot.Features.GlobalAccounts;

namespace Bot.Features.Economy
{
    public class Daily : IDailyMiunies
    {
        private readonly GlobalUserAccounts globalUserAccountProvider;

        public Daily(GlobalUserAccounts globalUserAccountProvider)
        {
            this.globalUserAccountProvider = globalUserAccountProvider;
        }
        
        public IGlobalUserAccounts GetDaily(ulong userId)
        {
            var account = globalUserAccountProvider.GetById(userId);
            var sinceLastDaily = DateTime.UtcNow - account.LastDaily;

            if (sinceLastDaily.TotalHours < 24)
            {
                var e = new InvalidOperationException(Constants.ExDailyTooSoon);
                e.Data.Add("sinceLastDaily", sinceLastDaily);
                throw e;
            }

            account.Coins += Constants.DailyMuiniesGain;
            account.LastDaily = DateTime.UtcNow;

            globalUserAccountProvider.SaveAccounts(userId);
            return globalUserAccountProvider;
        }
    }
}
