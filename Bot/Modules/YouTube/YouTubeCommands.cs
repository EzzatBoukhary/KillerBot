using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.WebSocket;
using Bot.Models.YouTube;
using Google.Apis.YouTube;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Bot.Preconditions;

namespace Bot.Modules.YouTube
{
    public class YouTubeCommands : ModuleBase
    {
        private static YouTubeApi _youTubeApi = null;

        public YouTubeCommands(YouTubeApi youTubeApi)
        {
            if (_youTubeApi == null)
            {
                _youTubeApi = youTubeApi;
            }
        }
        //Command that links just one video normally so it has play button
        [Command("ysearch", RunMode = RunMode.Async)]
        [Alias("ytsearch", "yt", "youtube", "youtubesearch")]
        [Ratelimit(6, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [Summary("Search YouTube for a specific keyword")]
        public async Task SearchYouTube([Remainder] string args = "")
        {
            string searchFor = string.Empty;
            var embed = new EmbedBuilder();
            var embedThumb = Context.User.GetAvatarUrl();
            StringBuilder sb = new StringBuilder();
            List<Google.Apis.YouTube.v3.Data.SearchResult> results = null;

            embed.ThumbnailUrl = embedThumb;

            if (string.IsNullOrEmpty(args))
            {
                
                embed.Title = $"No search term provided!";
                embed.WithColor(new Color(255, 0, 0));                 
                sb.AppendLine("Please provide a term to search for!");
                embed.Description = sb.ToString();
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            else
            {
                searchFor = args;                                
                embed.WithColor(new Color(0, 255, 0));                
                results = await _youTubeApi.SearchChannelsAsync(searchFor);
            }

            if (results != null)
            {
                string videoUrlPrefix = $"https://www.youtube.com/watch?v=";
                embed.Title = $"YouTube Search For (**{searchFor}**)";
                var thumbFromVideo = results.Where(r => r.Id.Kind == "youtube#video").Take(1).FirstOrDefault();
                if (thumbFromVideo != null)
                {
                    embed.ThumbnailUrl = thumbFromVideo.Snippet.Thumbnails.Default__.Url;
                }                
                foreach (var result in results.Where(r => r.Id.Kind == "youtube#video").Take(3))
                {
                    string fullVideoUrl = string.Empty;
                    string videoId = string.Empty;
                    string description = string.Empty;
                    if (string.IsNullOrEmpty(result.Snippet.Description))
                    {
                        description = "No description available.";
                    }
                    else
                    {
                        description = result.Snippet.Description;
                    }
                    if (result.Id.VideoId != null)
                    {
                        fullVideoUrl = $"{videoUrlPrefix}{result.Id.VideoId.ToString()}";
                    }
                    sb.AppendLine($":video_camera: **__{result.Snippet.ChannelTitle}__** -> [**{result.Snippet.Title}**]({fullVideoUrl})\n\n *{description}*\n");              
                }
                if (results.Count == 0)
                {
                    sb.AppendLine(":x: No results found.");
                    embed.WithColor(new Color(255, 0, 0));
                }
                embed.Description = sb.ToString();
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }
    }
}
