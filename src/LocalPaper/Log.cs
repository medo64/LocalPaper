namespace LocalPaper;

using System;
using System.Threading;

internal static class Log {

    public static void Verbose(string message) {
        if (MinimumLogLevel > LogLevel.Verbose) { return; }
        WriteLine(ConsoleColor.DarkGray, 'V', message);
    }

    public static void Debug(string message) {
        if (MinimumLogLevel > LogLevel.Debug) { return; }
        WriteLine(ConsoleColor.Blue, 'D', message);
    }

    public static void Info(string message) {
        if (MinimumLogLevel > LogLevel.Information) { return; }
        WriteLine(ConsoleColor.Gray, 'I', message);
    }

    public static void Warning(string message) {
        if (MinimumLogLevel > LogLevel.Warning) { return; }
        WriteLine(ConsoleColor.Yellow, 'W', message);
    }

    public static void Error(string message) {
        if (MinimumLogLevel > LogLevel.Error) { return; }
        WriteLine(ConsoleColor.Red, 'E', message);
    }

    public static void Critical(string message) {
        if (MinimumLogLevel > LogLevel.Critical) { return; }
        WriteLine(ConsoleColor.Red, 'E', message);
        Environment.Exit(1);
    }


    private static readonly Lock SyncRoot = new();

    private static void WriteLine(ConsoleColor color, char levelIndicator, string message) {
        var now = DateTime.Now;
        lock (SyncRoot) {
            Console.ForegroundColor = color;
            Console.WriteLine($"{now:yyyy-MM-dd HH:mm:ss} {levelIndicator}: {message}");
            Console.ResetColor();
        }
    }

    public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
}


public enum LogLevel {
    Verbose = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}
