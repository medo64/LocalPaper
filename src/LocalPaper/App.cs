namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;

internal static class App {
    public static void Main(string[] args) {
        var ips = new List<IPAddress>();
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
            if (IPAddress.IsLoopback(ip)) { continue; }
            if (ip.IsIPv6LinkLocal) { continue; }
            if (ip.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6) {
                ips.Add(ip);
            }
        }

        if (ips.Count == 0) {
            Log.Error("No local IP address found.");
            Environment.Exit(1);
        }

        Log.Debug($"Local IP Addresses: {string.Join(", ", ips)}");

        var composers = new List<DeviceDisplay>();
        composers.Add(GetComposer("any"));

        using var web = new WebServer(ips, 8084, composers);
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
