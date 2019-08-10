using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bot.Extensions;
using Discord.Commands;
using Discord.WebSocket;
using Bot.Features.GlobalAccounts;
using Bot.Providers;
using Discord;

namespace Bot.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmdService;
        private readonly IServiceProvider _serviceProvider;
        private readonly GlobalGuildAccounts _globalGuildAccounts;
        private readonly GlobalUserAccounts _globalUserAccounts;
        private readonly RoleByPhraseProvider _roleByPhraseProvider;

        public CommandHandler(DiscordSocketClient client, CommandService cmdService, IServiceProvider serviceProvider, GlobalGuildAccounts globalGuildAccounts, GlobalUserAccounts globalUserAccounts, RoleByPhraseProvider roleByPhraseProvider)
        {
            _client = client;
            _cmdService = cmdService;
            _serviceProvider = serviceProvider;
            _globalGuildAccounts = globalGuildAccounts;
            _globalUserAccounts = globalUserAccounts;
            _roleByPhraseProvider = roleByPhraseProvider;

        }

        public async Task InitializeAsync()
        {
            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            Global.Client = _client;
            _client.MessageReceived += CheckForDM;
        }

        public async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) { return; }
            if (msg.Channel is SocketDMChannel) { return; }
            if (msg.Author.IsBot) { return; }
            var user = _globalUserAccounts.GetById(msg.Author.Id);
            if (user.Blacklisted == true) { return; }
            var context = new MiunieCommandContext(_client, msg, _globalUserAccounts);

            await _roleByPhraseProvider.EvaluateMessage(
                context.Guild,
                context.Message.Content,
                (SocketGuildUser) context.User
            );

            var argPos = 0;
            if (msg.HasStringPrefix("k!", ref argPos)
               || msg.HasMentionPrefix(_client.CurrentUser, ref argPos)
               || CheckPrefix(ref argPos, context))
            {
                
                var cmdSearchResult = _cmdService.Search(context, argPos);
                if (!cmdSearchResult.IsSuccess) { return; }
                
                context.RegisterCommandUsage();
                
                var executionTask = _cmdService.ExecuteAsync(context, argPos, _serviceProvider);

                #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                executionTask.ContinueWith(task =>
                {
                    if (task.Result.IsSuccess || task.Result.Error == CommandError.UnknownCommand) return;
                    
                    if (task.Result.IsSuccess || task.Result.Error == CommandError.BadArgCount || task.Result.Error == CommandError.ParseFailed)
                    {
                        EmbedBuilder errormsg = new EmbedBuilder();
                        errormsg.WithColor(Color.Red);
                        errormsg.WithCurrentTimestamp();
                        errormsg.WithTitle("Bad Usage:");
                        errormsg.WithDescription($"{task.Result.ErrorReason} \n \n**Please do `k!help {cmdSearchResult.Text}`**");
                        context.Channel.SendMessageAsync("", false, errormsg.Build());
                        return;
                    }
                    else if (task.Result.IsSuccess || task.Result.ErrorReason.Contains("Object reference not set to an instance of an object"))
                    {
                        EmbedBuilder errormsg = new EmbedBuilder();
                        errormsg.WithColor(Color.Red);
                        errormsg.WithCurrentTimestamp();
                        errormsg.WithTitle("Object Reference:");
                        errormsg.WithDescription($"{task.Result.ErrorReason} \n \n**Please report this to the bot owner if you think this wasn't supposed to happen by doing `k!report [bug]`**");
                        context.Channel.SendMessageAsync("", false, errormsg.Build());
                        return;
                    }
                        const string errTemplate = "{0}, Error: {1}.";
                    var errMessage = string.Format(errTemplate, context.User.Mention, task.Result.ErrorReason);
                    context.Channel.SendMessageAsync(errMessage);
                });
                 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async Task CheckForDM(SocketMessage parameterMessage)
        {
            var channel = _client.GetChannel(587746737175789585) as SocketTextChannel;
            var message = parameterMessage as SocketUserMessage;
            int argPos = 0;
            if ((message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix("k!", ref argPos))) return;

            if (message.Channel is IDMChannel && message.Author != _client.CurrentUser)
            {
                var info = await _client.GetApplicationInfoAsync();
                var emb = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithTitle($"New DM: ");
                    if (message.Content.ToString().Contains("discord.gg/"))
                {
                    emb.WithDescription($"<@{message.Author.Id}>: [Message contains a server invite link] \n \n|| {message.Content} ||");
                }
                    else if (message.Content.ToString() == null || message.Content.Length == 0)
                    emb.WithDescription($"<@{message.Author.Id}>: [Message was an image or an embed]");

                else
                {
                    emb.WithDescription($"<@{message.Author.Id}>: {message.Content}");
                }
                    
                    emb.WithTimestamp(message.Timestamp)
                    .WithFooter($"From: {message.Author}", message.Author.GetAvatarUrl());
                await channel.SendMessageAsync("", false, emb.Build());
            }
        }

        private bool CheckPrefix(ref int argPos, SocketCommandContext context)
        {
            if (context.Guild is null) return false;
            var prefixes = _globalGuildAccounts.GetById(context.Guild.Id).Prefixes;
            var tmpArgPos = 0;
            var success = prefixes.Any(pre =>
            {
                if (!context.Message.Content.StartsWith(pre)) return false;
                tmpArgPos = pre.Length;
                return true;
            });
            argPos = tmpArgPos;
            return success;
        }
        
}
}
