using Discord.WebSocket;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Bot.Configuration;
using Bot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Bot.Features;
using Bot.Features.Trivia;
using Discord.Commands;
using Bot.Features.Lists;
using Bot.Features.Onboarding;
using Bot.Features.GlobalAccounts;
using Bot.DiscordAbstractions;
 using Bot.Features.Economy;
using Bot.Features.Onboarding.Tasks;
using Bot.Providers;
using Bot.Helpers;
using Bot.Modules;
using Bot.Extensions;
using System.Linq;

namespace Bot
{
    class Program
    {
     
        private static DiscordSocketClient _client;
        private static IServiceProvider _serviceProvider;

        private static async Task Main(string[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, args);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            BlackBox.Initialize();
            
            using (_client = _serviceProvider.GetRequiredService<DiscordSocketClient>())
            {
                _serviceProvider.GetRequiredService<DiscordEventHandler>().InitDiscordEvents();
                await _serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();

                while (!await AttemptLogin()) { }

                await _client.StartAsync();                

                do
                {
                    Console.WriteLine("Press Q to exit");

                }
                while (Console.ReadKey(true).Key != ConsoleKey.Q);
            }

        }
       
        private static void ConfigureServices(IServiceCollection serviceCollection, string[] args)
        {
            // no objects are created within this function; nor are any initializers executed
            // BotSettings and ApplicationSettings are created once they are first requested, not before

            serviceCollection.AddSingleton<Logger>();
            serviceCollection.AddSingleton<TriviaGames>();
            serviceCollection.AddSingleton<DiscordEventHandler>();
            serviceCollection.AddSingleton<CommandHandler>();
            serviceCollection.AddSingleton<CommandService>();
            serviceCollection.AddSingleton<ApplicationSettings>((s) =>
            {
                BotSettings botSettings = s.GetRequiredService<BotSettings>();
                return new ApplicationSettings(args, botSettings);
            });
            serviceCollection.AddSingleton<DiscordSocketClient>((s) =>
            {
                ApplicationSettings appSettings = s.GetRequiredService<ApplicationSettings>();
                return DiscordClientFactory.GetBySettings(appSettings);
            });
            serviceCollection.AddSingleton<IDataStorage, JsonDataStorage>();
            serviceCollection.AddSingleton<ListManager>();
            serviceCollection.AddSingleton<IOnboarding, Onboarding>();
            serviceCollection.AddSingleton<HelloWorldTask>();
            serviceCollection.AddSingleton<IDiscordSocketClient, DiscordSocketClientAbstraction>();
            serviceCollection.AddSingleton<IDailyMiunies, Daily>();
            serviceCollection.AddSingleton<IMiuniesTransfer, Transfer>();

            // serviceCollection.AddSingleton<BlogHandler>();
            serviceCollection.AddSingleton<GlobalGuildAccounts>();
            serviceCollection.AddSingleton<GlobalUserAccounts>();
            serviceCollection.AddSingleton<Announcements>();
            serviceCollection.AddSingleton<RoleByPhraseProvider>();
            serviceCollection.AddSingleton<MessageRewardHandler>();
            serviceCollection.AddSingleton<RepeatedTaskFunctions>();
            serviceCollection.AddSingleton<BotSettings>();
            serviceCollection.AddSingleton<JsonDataStorage>();
        }

        private static async Task<bool> AttemptLogin()
        {
            BotSettings botSettings = _serviceProvider.GetRequiredService<BotSettings>();
            try
            {
                await _client.LoginAsync(TokenType.Bot, botSettings.config.Token);
                return true;
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException == null)
                {
                    Console.WriteLine($"An HTTP Request exception occurred.\nMessage:\n{e.Message}");
                }
                else
                {
                    Global.WriteColoredLine($"An HTTP request ran into a problem:\n{e.InnerException.Message}",
                        ConsoleColor.Red);
                }

                var shouldTryAgain = GetTryAgainRequested();
                if (!shouldTryAgain) Environment.Exit(0);
                return false;
            }
            catch (Exception)
            {
                Console.WriteLine("An exception occurred. Your token might not be configured, or it might be wrong.");

                var shouldTryAgain = GetTryAgainRequested();
                if (!shouldTryAgain) Environment.Exit(0);
                botSettings.LoadConfig();
                return false;
            }
        }

        private static bool GetTryAgainRequested()
        {
            if (Global.Headless) return false;

            Console.WriteLine("\nDo you want to try again? (y/n)");
            Global.WriteColoredLine("(not trying again closes the application)\n", ConsoleColor.Yellow);

            return Console.ReadKey().Key == ConsoleKey.Y;
        }

       

    }
}
