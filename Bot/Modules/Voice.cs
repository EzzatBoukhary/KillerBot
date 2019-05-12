/* using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Bot.Extensions;
using Bot.Preconditions;

namespace Bot.Modules
{
    public class Voice : ModuleBase<MiunieCommandContext>
    {
        const int maxTimeInMinutes = 3600000;

        [Command("Voice", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [Cooldown(10, true)]
        public async Task CreateTemporaryVoiceChannel(int lifetimeInMinutes = 0)
        {
            lifetimeInMinutes *= 60000;
            if (lifetimeInMinutes == 0)
            {
                var use = await Context.Channel.SendMessageAsync("Use: ``!Voice {Time In Minutes}``");
                await Task.Delay(5000);
                await use.DeleteAsync();
            }
            else if (lifetimeInMinutes >= maxTimeInMinutes)
            {
                var embed = new EmbedBuilder();
                embed.WithTitle("!Voice");
                embed.AddField("Use", "!Voice {Ammount in minutes}");
                embed.AddField("Maximum time", "60 minutes");
                embed.WithCurrentTimestamp();
                embed.WithColor(0, 0, 255);
                await Context.Channel.SendMessageAsync("", embed: embed.Build());
            }
            else if (lifetimeInMinutes == 60000)
            {
                await Context.Message.DeleteAsync();
                var voiceChannel = await Context.Guild.CreateVoiceChannelAsync(name: $"{Context.User.Username}'s Voice Channel ({lifetimeInMinutes / 60000} minute)");
                var msg = await Context.Channel.SendMessageAsync($"A voice channel has been created! {Context.User.Mention}!");
                await msg.ModifyAsync(m => { m.Content = $"A voice channel has been created! {Context.User.Mention}! {lifetimeInMinutes / 60000} minute left."; });
                await Task.Delay(lifetimeInMinutes);
                await voiceChannel.DeleteAsync();
                await msg.DeleteAsync();
            }
            else if (lifetimeInMinutes >= 60001 && lifetimeInMinutes <= maxTimeInMinutes)
            {
                await Context.Message.DeleteAsync();
                var voiceChannel = await Context.Guild.CreateVoiceChannelAsync(name: $"{Context.User.Username}'s Voice Channel ({lifetimeInMinutes / 60000} minutes)");
                var msg = await Context.Channel.SendMessageAsync($"A voice channel has been created! {Context.User.Mention}! {lifetimeInMinutes / 60000} minutes left.");
                await Task.Delay(lifetimeInMinutes);
                await voiceChannel.DeleteAsync();
                await msg.ModifyAsync(x => { x.Content = $"{Context.User.Mention}, {lifetimeInMinutes / 60000} minutes have passed, the voice channel has been deleted."; });
            }
        }
    }
} */
