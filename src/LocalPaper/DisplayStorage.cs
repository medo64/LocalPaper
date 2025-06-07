namespace LocalPaper;

using System;
using System.Collections.Generic;

internal static class DisplayStorage {

    private static readonly Dictionary<string, int> BatteryPercentages = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, float> BatteryVoltages = new(StringComparer.OrdinalIgnoreCase);

    public static int RecordBatteryVoltage(string displayId, float voltage) {
        var percentage = (int)Math.Min(Math.Max(Math.Round((voltage - 3.2) * 100, 0), 0), 100);  // assume linear from 4.2V to 3.2V
        Log.Verbose($"Recorded battery for {displayId} ({voltage:0.00}V, {percentage}%)");

        BatteryPercentages[displayId] = percentage;
        BatteryVoltages[displayId] = voltage;
        return percentage;
    }

    public static int? GetBatteryPercentage(string displayId) {
        if (BatteryPercentages.TryGetValue(displayId, out var percentage)) {
            return percentage;
        }
        return null;
    }

    public static float? GetBatteryVoltage(string displayId) {
        if (BatteryVoltages.TryGetValue(displayId, out var voltage)) {
            return voltage;
        }
        return null;
    }


    private static readonly Dictionary<string, int> WirelessPercentages = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, int> WirelessRssi = new(StringComparer.OrdinalIgnoreCase);
    private static readonly int MinRssi = -90;
    private static readonly int MaxRssi = -30;

    public static int RecordWirelessRssi(string displayId, int rssi) {
        //var percentage = (int)Math.Min(Math.Max(Math.Round(((double)rssi + 100) * 2, 0), 0), 100);
        var percentage = (int)Math.Min(Math.Max(Math.Round(100.0 * (rssi - MinRssi) / (MaxRssi - MinRssi), 0), 0), 100);
        Log.Verbose($"Recorded wireless RSSI for {displayId} ({rssi}dBm, {percentage}%)");

        WirelessPercentages[displayId] = percentage;
        WirelessRssi[displayId] = rssi;
        return percentage;
    }

    public static int? GetWireleassPercentage(string displayId) {
        if (WirelessPercentages.TryGetValue(displayId, out var percentage)) {
            return percentage;
        }
        return null;
    }

    public static int? GetWirelessRssi(string displayId) {
        if (WirelessRssi.TryGetValue(displayId, out var rssi)) {
            return rssi;
        }
        return null;
    }

}
