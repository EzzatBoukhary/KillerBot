using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Net;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Bot.Models.Giphy;
using Microsoft.Extensions.Configuration;
using Bot.Preconditions;

namespace Bot.Modules.Giphy
{
    public class GiphyCommands : ModuleBase
    {
        private GiphyApi _api;

        public GiphyCommands(GiphyApi api)
        {
            _api = api;
        }

        [Command("giphy", RunMode = RunMode.Async)]
        [Alias("gif")]
        [Ratelimit(5, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [Summary("Searches for a gif to reply to. You can optionally provide a search term.")]
        public async Task Giphy([Remainder] string args = "")
        {            
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(0, 255, 255));        
                StringBuilder sb = new StringBuilder();
                GiphyReponse r = new GiphyReponse();
            try
                {
                if (string.IsNullOrEmpty(args))
                    {
                        r = _api.GetRandomImage(string.Empty);
                        embed.Title = $"__Giphy for [**{Context.User.Username}**]__";
                    }
                    else
                    {                                              
                        r = _api.GetRandomImage(args);                        
                        embed.Title = $"__Giphy for [**{Context.User.Username}**] ({args})__";
                    }                                                        
                    
                    embed.ImageUrl = r.data.fixed_height_small_url;
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                catch (Exception ex)
                {
                    await Context.Channel.SendMessageAsync($"No result found.");
                    Console.WriteLine($"Giphy Command Error -> [{ex.Message}]");
                }
            }
        }
        
    }