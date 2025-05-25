namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;

internal static class App {
    public static void Main(string[] args) {
        // Host config

        var host = Environment.GetEnvironmentVariable("LP_HOST") ?? Environment.MachineName;
        if (!int.TryParse(Environment.GetEnvironmentVariable("LP_PORT"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) || !(port is > 0 and < 65535)) {
            port = 8084;
        }

        var composers = new List<DeviceDisplay>();
        composers.Add(GetComposer("any"));

        using var web = new WebServer(host, port, composers);


        // Time zone config

        var timeZoneId = Environment.GetEnvironmentVariable("LP_TIMEZONE") ?? "America/Los_Angeles";
        TimeZoneInfo defaultTimeZone = TimeZoneInfo.Local;
        try {
            defaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        } catch (TimeZoneNotFoundException) {
            Log.Warning($"Time zone '{timeZoneId}' not found, using system default");
        } catch (InvalidTimeZoneException) {
            Log.Warning($"Invalid time zone '{timeZoneId}', using system default");
        }
        var offsetSign = defaultTimeZone.BaseUtcOffset < TimeSpan.Zero ? "-" : "+";
        Log.Info($"Default time zone is '{defaultTimeZone.Id}' (UTC{offsetSign}{defaultTimeZone.BaseUtcOffset:hh\\:mm})");


        web.Start();

        var cancelSource = new CancellationTokenSource();
        var cancelToken = cancelSource.Token;

        Console.CancelKeyPress += (sender, e) => {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            web.Stop();
            cancelSource.Cancel();
        };

        while (!cancelToken.IsCancellationRequested) {
            Thread.Sleep(1000);
        }

        Environment.Exit(0);
    }


    public static DeviceDisplay GetDisplay(string id, TimeZoneInfo defaultTimeZone) {  // TODO: currently hardcoded
        var dateComposer = new DateComposer("yyyy-MM-dd", "dddd", "HH:mm");
        return new DeviceDisplay("any", [
            (new Rectangle(0, 0, 800, 48), dateComposer)
        ], defaultTimeZone);
    }

}
