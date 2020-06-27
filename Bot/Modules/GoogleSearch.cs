using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Google.Apis.Customsearch.v1.Data;
using Bot.Preconditions;
using Bot.Services.Google;

namespace Bot.Modules.Fun
{
	public class GoogleSearch : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Searches Google
		// Contributors     - Creepysin, 

		[Command("google", RunMode = RunMode.Async)]
		[Summary("Searches Google")]
		[Alias("g")]
		[Cooldown(5)]
		[RequireBotPermission(GuildPermission.EmbedLinks)]
		public async Task Google([Remainder] string search = "")
		{

			if (string.IsNullOrEmpty(search))
			{
				await Context.Channel.SendMessageAsync("The search input cannot be blank!");
				return;
			}

			await GSearch(search, Context.Channel);
		}

		[Command("google", RunMode = RunMode.Async)]
		[Summary("Searches Google")]
		[Alias("g")]
		[Cooldown(5)]
		[RequireBotPermission(GuildPermission.EmbedLinks)]
		public async Task Google(int maxSearchResults = 10, [Remainder] string search = "")
		{
			if (string.IsNullOrEmpty(search))
			{
				await Context.Channel.SendMessageAsync("The search input cannot be blank!");
				return;
			}

			if (maxSearchResults > 10)
			{
				await Context.Channel.SendMessageAsync(
					$"The max search amount you have put in is too high! It has to be below 10.");
				return;
			}

			await GSearch(search, Context.Channel, maxSearchResults);
		}

		private async Task GSearch(string search, ISocketMessageChannel channel, int maxResults = 10)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithAuthor($"Google Search '{search}'", "https://upload.wikimedia.org/wikipedia/commons/thumb/5/53/Google_%22G%22_Logo.svg/588px-Google_%22G%22_Logo.svg.png", "");
            embed.WithDescription("Searching Google...");
			embed.WithFooter($"Search by {Context.User}", Context.User.GetAvatarUrl());
			embed.WithColor(new Color(237, 237, 237));
			embed.WithCurrentTimestamp();

			RestUserMessage message = await channel.SendMessageAsync("", false, embed.Build());

			Search searchListResponse = GoogleService.Search(search, GetType().ToString());

			StringBuilder description = new StringBuilder();

			int currentResult = 0;
			foreach (Result result in searchListResponse.Items)
			{
				if (currentResult == maxResults) continue;

				string link = $"**[{result.Title}]({result.Link})**\n{result.Snippet}\n\n";

				if (description.Length >= 2048)
					continue;

				if (description.Length + link.Length >= 2048)
					continue;

				description.Append(link);
				currentResult += 1;
			}

			embed.WithDescription(description.ToString());
			embed.WithCurrentTimestamp();

			await message.ModifyAsync(x => { x.Embed = embed.Build(); });
		}
	}
}