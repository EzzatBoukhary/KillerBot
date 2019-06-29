using System;
using System.Collections.Generic;

namespace Bot
{
    public static class Constants
    {
        internal static readonly string ResourceFolder = "resources";
        internal static readonly string UserAccountsFolder = "users";
        internal static readonly string ServerAccountsFolder = "servers";
        internal static readonly string LogFolder = "logs";
        internal static readonly string InvisibleString = "\u200b";
        public const ulong DailyMuiniesGain = 250;
        public const int WorkRewardCooldown = 3600;
        public const int MessageRewardCooldown = 30;
        public const int MessageRewardMinLenght = 20;
        public const int MaxMessageLength = 2000;
        public static readonly Tuple<int, int> MessagRewardMinMax = Tuple.Create(1, 5);
        public static readonly Tuple<int, int> WorkRewardMinMax = Tuple.Create(20, 200);
        public static readonly int MinTimerIntervall = 3000;
        public const int MaxCommandHistoryCapacity = 5;
        public static readonly IList<string> DidYouKnows = new List<string> {
            "I was made by Panda#8822",
            "If you don't know what to add, you can add some of my messages.",
            "A lot of commands have shorter and easier to use aliases!"
        }.AsReadOnly();
       
        // Exception messages
        public static readonly string ExDailyTooSoon = "Cannot give daily sooner than 24 hours after the last one.";
        public static readonly string ExTransferSameUser = "Cannot transfer coins to the same user.";
        public static readonly string ExTransferToKB = "Cannot transfer coins to myself.";
        public static readonly string ExTransferNotEnoughFunds = "Cannot transfer coins, not enough funds.";
    }
}
