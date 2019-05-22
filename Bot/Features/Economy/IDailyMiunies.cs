using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Features.Economy
{
    public interface IDailyMiunies
    {
        GlobalAccounts.IGlobalUserAccounts GetDaily(ulong userId);
    }
}
