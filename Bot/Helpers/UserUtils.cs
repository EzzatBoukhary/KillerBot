﻿using Discord;
using Discord.WebSocket;

namespace Bot.Helpers
{
	public static class UserUtils
	{
		/// <summary>
		/// Kicks a user
		/// </summary>
		/// <param name="user"></param>
		/// <param name="kicker"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static void KickUser(SocketGuildUser user, SocketUser kicker, string message)
		{
			user.KickAsync($"{message} | Kicked by {kicker.Username}").GetAwaiter().GetResult();
		}

		/// <summary>
		/// Bans a user
		/// </summary>
		/// <param name="user"></param>
		/// <param name="kicker"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static void BanUser(SocketGuildUser user, SocketUser kicker, string message)
		{
			user.BanAsync(0, $"{message} | Banned by {kicker.Username}").GetAwaiter().GetResult();
		}
	}
}
