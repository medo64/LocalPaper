namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualBasic;

internal static class App {
    public static void Main(string[] args) {
        var host = Environment.GetEnvironmentVariable("LP_HOST") ?? Environment.MachineName;
        if (!int.TryParse(Environment.GetEnvironmentVariable("LP_PORT"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) || !(port is > 0 and < 65535)) {
            port = 8084;
        }

        var composers = new List<DeviceDisplay>();
        composers.Add(GetComposer("any"));

        using var web = new WebServer(host, port, composers);
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


    public static DeviceDisplay GetComposer(string id) {  // TODO: currently hardcoded
        var dateComposer = new DateComposer("yyyy-MM-dd", "dddd", "HH:mm");
        return new DeviceDisplay("any", [
            (new Rectangle(0, 0, 800, 48), dateComposer)
        ]);
    }

}
