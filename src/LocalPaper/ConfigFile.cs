namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

internal class ConfigFile {

    public ConfigFile(FileInfo configFile) {
        var ini = new IniFile(configFile.FullName);
        foreach (var section in ini.GetSections()) {
            var sectionDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in ini.GetKeys(section)) {
                var value = ini.Read(section, key);
                if (value != null) { sectionDict[key] = value; }
            }
            Sections[section] = sectionDict;
        }
    }


    private readonly Dictionary<string, Dictionary<string, string>> Sections = new(StringComparer.OrdinalIgnoreCase);


    public bool HasSection(string section) {
        if (Sections.TryGetValue(section, out var sectionDict)) {
            return sectionDict.Count > 0;
        }
        return false;
    }

    internal IEnumerable<string> GetSections() {
        return Sections.Keys;
    }


    public string? Consume(string section, string key) {
        if (Sections.TryGetValue(section, out var sectionDict) && sectionDict.TryGetValue(key, out var value)) {
            sectionDict.Remove(key);
            if (sectionDict.Count == 0) { Sections.Remove(section); }
            return value;
        }
        return null;
    }

    public string Consume(string section, string key, string defaultValue) {
        if (Consume(section, key) is string value) {
            return value;
        }
        return defaultValue;
    }

    public bool Consume(string section, string key, bool defaultValue) {
        if ((Consume(section, key) is string value) && bool.TryParse(value, out var boolValue)) {
            return boolValue;
        }
        return defaultValue;
    }

    public int Consume(string section, string key, int defaultValue) {
        if ((Consume(section, key) is string value) && int.TryParse(value, out var intValue)) {
            return intValue;
        }
        return defaultValue;
    }

    public int Consume(string section, string key, int defaultValue, int minValue, int maxValue) {
        if ((Consume(section, key) is string value) && int.TryParse(value, out var intValue)) {
            if (intValue < minValue) { return minValue; }
            if (intValue > maxValue) { return maxValue; }
            return intValue;
        }
        return defaultValue;
    }

    public SKTextAlign Consume(string section, string key, SKTextAlign defaultValue) {
        if (Consume(section, key) is string value) {
            if (value.Equals("center", StringComparison.OrdinalIgnoreCase)) {
                return SKTextAlign.Center;
            } else if (value.Equals("left", StringComparison.OrdinalIgnoreCase)) {
                return SKTextAlign.Left;
            } else if (value.Equals("right", StringComparison.OrdinalIgnoreCase)) {
                return SKTextAlign.Right;
            }
        }
        return defaultValue;
    }

    public VerticalAlignment Consume(string section, string key, VerticalAlignment defaultValue) {
        if (Consume(section, key) is string value) {
            if (value.Equals("middle", StringComparison.OrdinalIgnoreCase)) {
                return VerticalAlignment.Middle;
            } else if (value.Equals("top", StringComparison.OrdinalIgnoreCase)) {
                return VerticalAlignment.Top;
            } else if (value.Equals("bottom", StringComparison.OrdinalIgnoreCase)) {
                return VerticalAlignment.Bottom;
            }
        }
        return defaultValue;
    }

}
