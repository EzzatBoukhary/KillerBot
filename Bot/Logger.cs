using System;
using System.IO;
using System.Threading.Tasks;
using Bot.Configuration;
using Discord;
using Discord.WebSocket;

namespace Bot
{
    public class Logger
    {
        private readonly ApplicationSettings _appSettings;

        public Logger(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        internal Task Log(LogSeverity severity, string source, string message, Exception exception = null)
        {
            Log(new LogMessage(severity, source, message, exception));
            return Task.CompletedTask;
        }

        internal Task Log(LogMessage logMessage)
        {
            string message = String.Concat(DateTime.Now.ToShortTimeString(), " [", logMessage.Source, "] ", logMessage.Message);
            if (_appSettings.LogIntoConsole)
            {
                LogConsole(message, logMessage.Severity);
                if (logMessage.Exception != null)
                {
                    LogConsole(logMessage.Exception.Message, logMessage.Severity);
                    LogConsole(logMessage.Exception.StackTrace, logMessage.Severity);
                    LogConsole(logMessage.Exception.Message, logMessage.Severity);
                }
            }

            if(_appSettings.LogIntoFile) LogFile(message);
            return Task.CompletedTask;
        }

        private void LogFile(string message)
        {
            var fileName = "Logs2.log";
            var folder = Constants.LogFolder;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            StreamWriter sw = File.AppendText($"{folder}/{fileName}");
            sw.WriteLine(message);
            sw.Close();
        }

        private void LogConsole(string message, LogSeverity severity)
        {

            Console.ForegroundColor = SeverityToConsoleColor(severity);
            Console.WriteLine(message);
            Console.ResetColor();
            LogFile(message);
        }

        private ConsoleColor SeverityToConsoleColor(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                    return ConsoleColor.Red;
                case LogSeverity.Debug:
                    return ConsoleColor.Blue;
                case LogSeverity.Error:
                    return ConsoleColor.Yellow;
                case LogSeverity.Info:
                    return ConsoleColor.Blue;
                case LogSeverity.Verbose:
                    return ConsoleColor.Green;
                case LogSeverity.Warning:
                    return ConsoleColor.Magenta;
                default:
                    return ConsoleColor.White;
            }
        }
    }
}
