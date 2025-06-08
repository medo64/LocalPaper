namespace LocalPaper;

using System;
using System.Collections.Generic;

internal static class Recorder {

    private static readonly Dictionary<string, BatteryLevel> BatteryLevels = new(StringComparer.OrdinalIgnoreCase);

    public static void RecordBattery(string displayId, BatteryLevel level) {
        var list = new List<string>();
        if (level.Voltage is not null) { list.Add($"{level.Voltage.Value:0.00}V"); }
        if (level.Percentage is not null) { list.Add($"{level.Percentage.Value}%"); }
        if (level.IsCharging) { list.Add("charging"); }
        Log.Verbose($"Recorded battery for {displayId} ({string.Join(", ", list)})");
        BatteryLevels[displayId] = level;
    }

    public static BatteryLevel GetBatteryLevel(string displayId) {
        return BatteryLevels.TryGetValue(displayId, out var level) ? level : new BatteryLevel();
    }


    private static readonly Dictionary<string, WirelessLevel> WirelessLevels = new(StringComparer.OrdinalIgnoreCase);

    public static void RecordWireless(string displayId, WirelessLevel level) {
        var list = new List<string>();
        if (level.Rssi is not null) { list.Add($"{level.Rssi.Value:0} dBm"); }
        if (level.Percentage is not null) { list.Add($"{level.Percentage.Value}%"); }
        Log.Verbose($"Recorded wireless RSSI for {displayId} ({string.Join(", ", list)})");
        WirelessLevels[displayId] = level;
    }

    public static WirelessLevel GetWirelessLevel(string displayId) {
        return WirelessLevels.TryGetValue(displayId, out var level) ? level : new WirelessLevel();
    }

}
