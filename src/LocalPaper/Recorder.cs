namespace LocalPaper;

using System;
using System.Collections.Generic;

internal static class Recorder {

    private static readonly Dictionary<string, BatteryLevel> BatteryLevels = new(StringComparer.OrdinalIgnoreCase);

    public static void RecordBattery(string displayId, BatteryLevel level) {
        Log.Verbose($"Recorded battery for {displayId} ({level.Voltage:0.00}V, {level.Percentage}%)");
        BatteryLevels[displayId] = level;
    }

    public static BatteryLevel GetBatteryLevel(string displayId) {
        if (BatteryLevels.TryGetValue(displayId, out var level)) {
            return level;
        }
        return new BatteryLevel();
    }


    private static readonly Dictionary<string, WirelessLevel> WirelessLevels = new(StringComparer.OrdinalIgnoreCase);

    public static void RecordWireless(string displayId, WirelessLevel level) {
        Log.Verbose($"Recorded wireless RSSI for {displayId} ({level.Rssi}dBm, {level.Percentage}%)");
        WirelessLevels[displayId] = level;
    }

    public static WirelessLevel GetWirelessLevel(string displayId) {
        if (WirelessLevels.TryGetValue(displayId, out var level)) {
            return level;
        }
        return new WirelessLevel();
    }

}
