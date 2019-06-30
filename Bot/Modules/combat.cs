using Bot.Preconditions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Bot.Helpers;
using System.Globalization;
using Bot.Extensions;
using Bot.Features.Lists;
using Discord.WebSocket;
using Discord.Rest;
using System.Diagnostics;
using System.Data;

namespace Bot.Modules
{
    public class combat : ModuleBase<MiunieCommandContext>
    {
        private CommandService _service;
        private readonly ListManager _listManager;
        private int _fieldRange = 10;

        public combat(CommandService service, ListManager listManager)
        {
            _service = service;
            _listManager = listManager;
        }

      
        Random rand = new Random();
        private int damage;
        static string player1;
        static string player2;
        static string whosTurn;
        static string whoWaits;
        static string placeHolder;
        static int health1 = 100;
        static int health2 = 100;
        static string SwitchCaseString = "nofight";


        [Command("fight")]
        [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.None)]
        [Alias("Fight")]
        [Summary("starts a fight with the @Mention user (example: `fight @Panda#8822`")]
        public async Task Fight(IUser user)
        {
            
            if (Context.User.Mention == user.Mention || SwitchCaseString != "nofight")
            {
                await ReplyAsync(Context.User.Mention + " sorry but there is already a fight going on, or u simply tried to kill youself.");
                return;
            }
           

            SwitchCaseString = "fight_p1";
            player1 = Context.User.Mention;
            player2 = user.Mention;

            string[] whoStarts = new string[]
         {
           Context.User.Mention,
           user.Mention,


         };
            Random rand = new Random();

            int randomIndex = rand.Next(whoStarts.Length);
            string text = whoStarts[randomIndex];

            whosTurn = text;
            if (text == Context.User.Mention)
            {
                whoWaits = user.Mention;
            }
            else
            {
                whoWaits = Context.User.Mention;
            }

            await ReplyAsync
            ("Fight started between " + Context.User.Mention + " and " + user.Mention + "!\n    `k!slash` to hit. \n"

            + player1 + " you got " + health1 + " health!\n"
            + player2 + " you got " + health2 + " health!\n \n"

            + text + " it is your turn!");
        }

        [Command("giveup")]
        [Alias("GiveUp", "Giveup", "giveUp")]
        [Summary("Stops the fight and gives up.")]
        public async Task GiveUp()
        {
            if (SwitchCaseString == "fight_p1") //we need to check if there is even a fight to stop.
            {
                await ReplyAsync("The fight stopped.");
                SwitchCaseString = "nofight";       //this lets another one start a fight again and dont let people attack anymore.
                health1 = 100;                               //we need to reset the health for both players again.
                health2 = 100;
            }
            else
            {
                await ReplyAsync("There is no fight to stop.");
            }
        }

        [Command("Slash")]
        [Alias("slash")]
        [Summary("Slashes your enemy with a sword. Good accuracy and medium damage")]
        public async Task Slash()
        {
            if (SwitchCaseString == "fight_p1")
            {

                if (whosTurn != Context.User.Mention)   //we simply use 2 if statements this time because we want to answer correctly.
                {
                    String message = Context.User.Mention;
                    message += (Context.User.Mention != player1 && Context.User.Mention != player2) 
                            ? " You are not in the game." 
                            : " it is not your turn.";
                    await ReplyAsync(message);
                    return;
                }   
            }
            else
            {
                await ReplyAsync("There is no fight at the moment. :/");
                return;
            }


            Random rand = new Random();

            int randomIndex = rand.Next(1, 5);

            if (randomIndex != 1)
            {


                Random rand2 = new Random();

                int randomIndex2 = rand2.Next(7, 15);     //just to mention again, this will only give all numbers from 7 to 14 back. Not including 15!


                await ReplyAsync(Context.User.Mention + " u hit and did " + randomIndex2 + " damage!");

                if (Context.User.Mention != player1)             //FIND THE ENEMY!
                {
                    health1 = health1 - randomIndex2;
                    if (health1 > 0)                                          //check if he/she is still alive
                    {

                        placeHolder = whosTurn;
                        whosTurn = whoWaits;
                        whoWaits = placeHolder;

                        await ReplyAsync(player1 + " got " + health1 + " health left!\n" + player2 + " got " + health2 + " health left!\n\n" + whosTurn + " ur turn!");
                    }
                    else
                    {
                        await ReplyAsync(player1 + " died. " + player2 + " won!");
                        SwitchCaseString = "nofight";                     //make sure to reset anything again
                        health1 = 100;
                        health2 = 100;
                    }
                }
                else if (Context.User.Mention == player1)               //checks this if the first one was you
                {
                    health2 = health2 - randomIndex2;
                    if (health2 > 0)
                    {

                        placeHolder = whosTurn;
                        whosTurn = whoWaits;
                        whoWaits = placeHolder;

                        await ReplyAsync(player1 + " got " + health1 + " health left!\n" + player2 + " got " + health2 + " health left!\n\n" + whosTurn + " ur turn!");
                    }
                    else
                    {
                        await ReplyAsync(player2 + " died. " + player1 + " won!");
                        SwitchCaseString = "nofight";
                        health1 = 100;
                        health2 = 100;
                    }
                }
                else
                {
                    await ReplyAsync("Sorry it seems like something went wrong. Pls type !giveup");
                }

            }
            else
            {
                await ReplyAsync(Context.User.Mention + " sorry, u missed.");

                placeHolder = whosTurn;      //this is the process of changing whosTurn. we need the placeholder
                whosTurn = whoWaits;
                whoWaits = placeHolder;

                await ReplyAsync(whosTurn + " ur turn!");
            }
        }
    }
}

    

