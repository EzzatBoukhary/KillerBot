using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Features.GlobalAccounts;
using Bot.Helpers;
using Bot.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Ofl.Linq;
using System.Collections.Generic;

namespace Bot.Modules.Account
{
    [Group("account")]
    public class ManageUserAccount : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalUserAccounts _globalUserAccounts;
        public ManageUserAccount(GlobalUserAccounts globalUserAccounts)
        {
            _globalUserAccounts = globalUserAccounts;
        }

        [Command("info")]
        [Summary("Shows your or someone's account info if mentioned.")]
        public async Task AccountInformation([Summary("The user you want to check. (if not mentioned it'll check yours")]SocketGuildUser user = null)
        {
            user = user ?? (SocketGuildUser)Context.User;

            var userAccount = _globalUserAccounts.GetFromDiscordUser(user);

            var embed = new EmbedBuilder()
                .WithAuthor($"{user.Username}'s account information", user.GetAvatarUrl())
                .AddField("Joined at: ", user.JoinedAt.Value.DateTime.ToString())
                .AddField("Last message:", userAccount.LastMessage.ToString(), true)
                .AddField("Number of reminders: ", userAccount.Reminders.Count, true)
                .AddField("Coins: ", userAccount.NetWorth.ToString())
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter($"Requested by {Context.User.Username}", Context.User.GetAvatarUrl())
                .WithThumbnailUrl(user.GetAvatarUrl())
                .Build();

            await Context.Channel.SendMessageAsync(Context.User.Mention, false, embed);
        }

        [Command("ShowCommandHistory"), Alias("CommandHistory", "CH")]
        [Summary("Gets and sends the last 5 commands used by you (KillerBot commands only)")]
        public async Task ShowCommandHistory()
        {
          
            await Context.Channel.SendMessageAsync(GetCommandHistory(Context.UserAccount));
        }

        //Could be in the extended ModuleBase, with a few changes
        private string GetCommandHistory(GlobalUserAccount userAccount)
        {
            var commandHistory = userAccount.CommandHistory.Select(cH => $"{cH.UsageDate.ToString("G")} {cH.Command}");
            return String.Join("\n", commandHistory); //Return the command history separated by line
        }

         [Command("GetAllMyAccountData"), Alias("GetMyData", "MyData", "data")]
         public async Task GetAccountFile()
         {
             var userFilePath = _globalUserAccounts.GetAccountFilePath(Context.User.Id);
             if (String.IsNullOrEmpty(userFilePath))
             {
                 await Context.Channel.SendMessageAsync("I don't have any information about you.");
                 return;
             }

             await Context.User.SendFileAsync(userFilePath, $"This is all I got about you! Note: Open it in notepad or use an application thats lets you view .json files if you want to view your data.");
             await Context.Channel.SendMessageAsync($"{Context.User.Mention} DM sent!");
         }

         [Command("reseteconomy", RunMode = RunMode.Async),Alias("economyreset","reset-economy", "economy-reset", "clear-economy", "delete-economy", "economy-delete")]
         public async Task DeleteAccount()
         {
             var response = await AwaitMessageYesNo("I will delete all the economy data i have about you, are you sure?", "Yes", "No");
             if (response is null)
             {
                 await Context.Channel.SendMessageAsync($"{Context.User.Mention} you took so long to reply!");
             }
             else
             {
                 await EvaluateResponse(response, "Yes");
             }
         }


         private async Task EvaluateResponse(SocketMessage response, string optionYes)
         {
             var message = "";
             if (response.Content.ToLower().Contains(optionYes.ToLower()))
             {
                
                var account = _globalUserAccounts.GetById(Context.User.Id);
                if (account != null)
                {
                    account.LastDaily = DateTime.UtcNow.AddDays(-2);
                    account.Coins = 1;
                    account.BankCoins = 0;
                    account.LastRobbery = DateTime.UtcNow.AddDays(-2);
                    //account.Bought_Items = new List<UserItem>();
                    _globalUserAccounts.SaveAccounts();
                    message = "I have reset your economy data!";
                }
                else
                    message = "Looks like i don't have any information about you.";
             }
             else
             {
                 message = "Alright, nevermind then!";
             }

             await Context.Channel.SendMessageAsync(Context.User.Mention + " " + message);
         }

         private async Task<SocketMessage> AwaitMessageYesNo(string message, string optionYes, string optionNo)
         {
             await Context.Channel.SendMessageAsync(
                 $"{Context.User.Mention} {message} Reply with `{optionYes}` or `{optionNo}`");
             var response = await Context.Channel.AwaitMessage(msg => EvaluateResponse(msg, optionYes, optionNo));
             return response;
         }

         private bool EvaluateResponse(SocketMessage arg, params String[] options)
             => options.Any(option => arg.Content.ToLower().Contains(option.ToLower()) && arg.Author.Id == Context.User.Id);
     } 
    }

