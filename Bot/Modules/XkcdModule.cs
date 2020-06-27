﻿using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Bot.Common;
using Bot.Services;
using System;
using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Helpers;
using Bot.Preconditions;

namespace Bot.Modules
{
    [Name("XkcdModule")]
    public class XkcdModule : ModuleBase<MiunieCommandContext>
    {
        private readonly IXkcdService xkcdService;
        private readonly ILogger logger;

        public XkcdModule(IXkcdService xkcdService, ILogger<XkcdModule> logger)
        {
            this.xkcdService = xkcdService;
            this.logger = logger;
        }

        [Command("xkcd")]
        [Remarks("Gets a random xkcd comic or the latest xkcd comic by appending \"latest\" to the command")]
        [Priority(0)]
        [Example("!xkcd latest")]
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task GetXkcdAsync(string arg = "")
        {
            XkcdResponse comic = !string.IsNullOrEmpty(arg) && arg.Equals("latest", StringComparison.OrdinalIgnoreCase)
                ? await xkcdService.GetLatestComicAsync().ConfigureAwait(false)
                : await xkcdService.GetRandomComicAsync().ConfigureAwait(false);
            await EmbedComicAsync(comic, Context.Channel).ConfigureAwait(false);
        }

        [Command("xkcd")]
        [Remarks("Gets the xkcd comic with the specified number")]
        [Priority(1)]
        [Example("!xkcd 101")]
        [Ratelimit(4, 1, Measure.Minutes, RatelimitFlags.None)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task GetXkcdAsync(int number)
        {
            try
            {
                XkcdResponse comic = await xkcdService.GetComicAsync(number).ConfigureAwait(false);
                await EmbedComicAsync(comic, Context.Channel).ConfigureAwait(false);
            }
            catch (ArgumentOutOfRangeException)
            {
                _ = await ReplyAsync("The specified comic does not exist!").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while getting xkcd comic");
            }

        }

        private async Task EmbedComicAsync(XkcdResponse comic, IMessageChannel channel)
        {
            if (comic == null)
            {
                return;
            }
            string comicUrl = xkcdService.GetComicUrl(comic.Number).ToString();
            var builder = new EmbedBuilder()
                .WithImageUrl(comic.ImgUrl.ToString())
                .WithAuthor($"xkcd #{comic.Number}", "https://xkcd.com/s/919f27.ico", comicUrl)
                .WithTitle(comic.SafeTitle)
                .WithDescription(comic.Alt);
            if (int.TryParse(comic.Year, out int year) && int.TryParse(comic.Month, out int month) && int.TryParse(comic.Day, out int day))
            {
                var date = new DateTime(year, month, day);
                _ = builder.WithFooter(date.ToString("yyyy-MM-dd"));
            }
            _ = await channel.SendMessageAsync("", false, builder.Build()).ConfigureAwait(false);
        }
    }
}