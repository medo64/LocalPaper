namespace LocalPaper;

using System;
using System.Collections.Generic;

internal static class Battery {

    private static readonly Dictionary<string, int> BatteryLevels = new(StringComparer.OrdinalIgnoreCase);

    public static int RecordVoltage(string device, float voltage) {
        var percentage = (int)Math.Min(Math.Max(Math.Round((voltage - 3.2) * 100, 0), 0), 100);  // assume linear from 4.2V to 3.2V
        BatteryLevels[device] = percentage;
        return percentage;
    }

    public static int? GetPercentage(string device) {
        if (BatteryLevels.TryGetValue(device, out var level)) {
            return level;
        }
        return null;
    }

}
