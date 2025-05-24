namespace LocalPaper;

using System;
using System.Threading;

internal static class Log {

    public static void Verbose(string message) {
        WriteLine(ConsoleColor.DarkGray, 'V', message);
    }

    public static void Debug(string message) {
        WriteLine(ConsoleColor.Blue, 'D', message);
    }

    public static void Info(string message) {
        WriteLine(ConsoleColor.Gray, 'I', message);
    }

    public static void Warning(string message) {
        WriteLine(ConsoleColor.Yellow, 'W', message);
    }

    public static void Error(string message) {
        WriteLine(ConsoleColor.Red, 'E', message);
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

}
