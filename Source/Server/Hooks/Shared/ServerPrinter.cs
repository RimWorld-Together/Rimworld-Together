using RTServer.Core;
using RTShared.Misc;
using System.Text;
using static RTShared.Misc.Printer;

namespace RTServer.Hooks.Shared
{
    public static class ServerPrinter
    {
        private static Semaphore Semaphore { get; set; } = new Semaphore(1, 1);

        private static Dictionary<LogMode, ConsoleColor> ColorDictionary { get; set; } = new Dictionary<LogMode, ConsoleColor>
        {
            { LogMode.Message, ConsoleColor.White },
            { LogMode.Warning, ConsoleColor.Yellow },
            { LogMode.Error, ConsoleColor.Red },
            { LogMode.Title, ConsoleColor.Green }
        };

        public static void CreateLogger()
        {
            Action<object, Verbosity> onMessage = delegate (object value, Verbosity importance)
            {
                if (CheckIfShouldPrint(importance)) WriteToConsole(value.ToString(), LogMode.Message, importance);
            };

            Action<object, Verbosity> onWarning = delegate (object value, Verbosity importance)
            {
                if (CheckIfShouldPrint(importance)) WriteToConsole(value.ToString(), LogMode.Warning, importance);
            };

            Action<object, Verbosity> onError = delegate (object value, Verbosity importance)
            {
                if (CheckIfShouldPrint(importance)) WriteToConsole(value.ToString(), LogMode.Error, importance);
            };

            Action<object, Verbosity> onTitle = delegate (object value, Verbosity importance)
            {
                if (CheckIfShouldPrint(importance)) WriteToConsole(value.ToString(), LogMode.Title, importance);
            };

            Printer printer = new Printer(onMessage, onWarning, onError, onTitle);
        }

        private static void WriteToConsole(string text, LogMode mode, Verbosity importance, bool writeToLogs = true)
        {
            Semaphore.WaitOne();

            try
            {
                if (CheckIfShouldPrint(importance))
                {
                    if (writeToLogs) WriteToLogs(text);

                    Console.ForegroundColor = ColorDictionary[mode];
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] | " + text);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch(Exception ex) { throw new Exception($"Logger encountered an error. This should never happen\n{ex}"); }

            Semaphore.Release();
        }

        private static void WriteToLogs(string toLog)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"[{DateTime.Now:HH:mm:ss}] | " + toLog);
            stringBuilder.Append(Environment.NewLine);

            DateTime dateTime = DateTime.Now.Date;
            string nowFileName = $"{dateTime.Year}-{dateTime.Month.ToString("D2")}-{dateTime.Day.ToString("D2")}";
            string nowFullPath = Master.SystemLogsPath + Path.DirectorySeparatorChar + nowFileName + ".txt";

            File.AppendAllText(nowFullPath, stringBuilder.ToString());
            stringBuilder.Clear();
        }

        private static bool CheckIfShouldPrint(Verbosity importance)
        {
            if (importance == Verbosity.Normal) return true;
            else if (importance == Verbosity.Verbose && Master.ServerConfig.Verbosity >= 1) return true;
            else if (importance == Verbosity.Extreme && Master.ServerConfig.Verbosity >= 2) return true;
            else if (importance == Verbosity.Ludicrous && Master.ServerConfig.Verbosity >= 3) return true;
            else return false;
        }
    }
}
