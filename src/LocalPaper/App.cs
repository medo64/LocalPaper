namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using SkiaSharp;

internal static class App {
    public static void Main(string[] args) {
        // Host config

        var host = Environment.GetEnvironmentVariable("LP_HOST") ?? Environment.MachineName;
        if (!int.TryParse(Environment.GetEnvironmentVariable("LP_PORT"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) || !(port is > 0 and < 65535)) {
            port = 8084;
        }


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


        // Device config

        var configDir = Environment.GetEnvironmentVariable("LP_CONFIG_DIR") ?? "/config";
        var configDirInfo = new DirectoryInfo(configDir);
        if (configDirInfo.Exists) {
            Log.Info($"Using configuration directory '{configDirInfo.FullName}'");
        } else {
            Log.Error($"Configuration directory '{configDirInfo.FullName}' does not exist");
            Environment.Exit(1);
        }

        DeviceDisplay? defaultDisplay = null;
        var displays = new List<DeviceDisplay>();

        var anyConfig = new FileInfo(Path.Combine(configDirInfo.FullName, "any", "config.ini"));
        var anyConfigAlt = new FileInfo(Path.Combine(configDirInfo.FullName, "config.ini"));
        if (anyConfig.Exists) {
            Log.Info($"Using wildcard configuration from '{anyConfig.FullName}'");
            if (anyConfigAlt.Exists) {
                Log.Warning($"Ignoring alternative wildcard configuration file '{anyConfigAlt.FullName}'");
            }
            defaultDisplay = GetDisplay("any", anyConfig, defaultTimeZone);
        } else if (anyConfigAlt.Exists) {
            Log.Info($"Using alternate wildcard configuration from '{anyConfig.FullName}'");
            defaultDisplay = GetDisplay("any", anyConfigAlt, defaultTimeZone);
        }

        foreach (var directory in configDirInfo.EnumerateDirectories()) {
            var deviceId = directory.Name;
            if (deviceId.Equals("any", StringComparison.Ordinal)) { continue; }  // already processed above

            var deviceConfig = new FileInfo(Path.Combine(directory.FullName, "config.ini"));
            if (!deviceConfig.Exists) {
                Log.Warning($"Device {deviceId} does not contain a configuration file 'config.ini'");
                continue;
            }
            defaultDisplay = GetDisplay(deviceId, deviceConfig, defaultTimeZone);
        }


        // Start webserver

        using var web = new WebServer(host, port, defaultDisplay, displays);
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


    public static DeviceDisplay GetDisplay(string displayId, FileInfo configFile, TimeZoneInfo defaultTimeZone) {  // TODO: currently hardcoded
        var config = new ConfigFile(configFile);

        var interval = config.Consume("Display", "Interval", 600);
        if (interval is < 300 or > 3600) {
            Log.Warning($"Invalid interval {interval} for display {displayId}, using default 600 seconds");
            interval = 600;
        }

        var displayWidth = config.Consume("Display", "Width", 800);
        var displayHeight = config.Consume("Display", "Height", 480);
        var displayIsInverted = config.Consume("Display", "Inverted", false);

        var timeZoneId = config.Consume("Display", "TimeZone", defaultTimeZone.Id);
        TimeZoneInfo timeZone = defaultTimeZone;
        try {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        } catch (TimeZoneNotFoundException) {
            Log.Warning($"Time zone '{timeZoneId}' not found, using default");
        } catch (InvalidTimeZoneException) {
            Log.Warning($"Invalid time zone '{timeZoneId}', using default");
        }

        if (config.HasSection("Display")) {
            Log.Warning($"Display {displayId} has unused configuration parameters");  // TODO: better message with the exact keys
        }

        Log.Info($"Display '{displayId}' is {displayWidth}x{displayHeight}; interval: {interval} seconds; time zone: {timeZone.Id}");

        var composers = new List<ComposerBag>();
        foreach (var section in config.GetSections()) {
            var kind = section.Split(['.', ':', ' '])[0];

            var left = config.Consume(section, "Left", 0, 0, displayWidth - 1);
            var right = config.Consume(section, "Right", displayWidth - 1, 0, displayWidth - 1);
            var top = config.Consume(section, "Top", 0, 0, displayHeight - 1);
            var bottom = config.Consume(section, "Bottom", displayHeight - 1, 0, displayHeight - 1);
            if (left < 0 || left > right || right >= displayWidth || top < 0 || top > bottom || bottom >= displayHeight) {
                Log.Warning($"Display '{displayId}' in section '{section}' has an invalid rectangle ({left}, {top}, {right}, {bottom}), skipping");
                continue;
            }
            var rect = new Rectangle(left, top, right - left + 1, bottom - top + 1);
            var isInverted = config.Consume(section, "Inverted", false);

            if ("Line".Equals(kind, StringComparison.Ordinal)) {
                var thickness = config.Consume(section, "Thickness", 1, 1, 100);
                composers.Add(new ComposerBag(
                    section,
                    new LineComposer(thickness),
                    rect,
                    isInverted
                ));
            } else if ("Rectangle".Equals(kind, StringComparison.Ordinal)) {
                composers.Add(new ComposerBag(
                    section,
                    new RectangleComposer(),
                    rect,
                    isInverted
                ));
            } else if ("Time".Equals(kind, StringComparison.Ordinal)) {
                var format = config.Consume(section, "Format", "dddd");
                var align = config.Consume(section, "Align", SKTextAlign.Center);
                composers.Add(new ComposerBag(
                    section,
                    new TimeComposer(format, align),
                    rect,
                    isInverted
                ));
            } else {
                Log.Warning($"Display {displayId} has unknown section '{section}'; skipping");
                continue;
            }
            Log.Info($"Display '{displayId}' composer '{kind}' in section '{section}' at ({left}, {top}, {right}, {bottom})");
        }

        return new DeviceDisplay(displayId, TimeSpan.FromSeconds(interval), displayWidth, displayHeight, displayIsInverted, timeZone, composers);
    }

}
