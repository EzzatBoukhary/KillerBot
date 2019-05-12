using Bot.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Features.GlobalAccounts
{
    public interface IGlobalUserAccounts : IGlobalAccounts
    {
        GlobalUserAccount GetById(ulong userId);
    }
}
