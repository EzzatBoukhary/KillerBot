using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Features.GlobalAccounts
{
    public interface IGlobalAccounts
    {
        void SaveAccounts(params ulong[] ids);
    }
}
