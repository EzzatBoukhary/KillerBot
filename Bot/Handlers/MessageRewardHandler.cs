using Bot.Features.GlobalAccounts;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bot.Handlers
{
    public class MessageRewardHandler
    {
        private readonly GlobalUserAccounts _globalUserAccounts;
        public MessageRewardHandler(GlobalUserAccounts globalUserAccounts)
        {
            _globalUserAccounts = globalUserAccounts;
        }

        public async Task HandleMessageRewards(SocketMessage s)
        {
            var msg = s as SocketUserMessage;

            if (msg == null) return;
            if (msg.Channel == msg.Author.GetOrCreateDMChannelAsync()) return;            
            if (msg.Author.IsBot) return;
            
            var userAcc = _globalUserAccounts.GetById(msg.Author.Id);
            DateTime now = DateTime.UtcNow;

            // Check if message is long enough and if the coolown of the reward is up - if not return
            if (now < userAcc.LastMessage.AddSeconds(Constants.MessageRewardCooldown) || msg.Content.Length < Constants.MessageRewardMinLenght)
            { 
                return; // This Message is not eligible for a reward
            }

            // Generate a randomized reward in the configured boundries
            userAcc.Miunies += (ulong) Global.Rng.Next(Constants.MessagRewardMinMax.Item1, Constants.MessagRewardMinMax.Item2 + 1);
            userAcc.LastMessage = now;

            _globalUserAccounts.SaveAccounts(msg.Author.Id);
        }
    }
}
