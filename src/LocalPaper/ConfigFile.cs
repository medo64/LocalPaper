namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Medo.Configuration;

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

    public int Consume(string section, string key, int defaultValue) {
        if ((Consume(section, key) is string value) && int.TryParse(value, out var intValue)) {
            return intValue;
        }
        return defaultValue;
    }

    public bool Consume(string section, string key, bool defaultValue) {
        if ((Consume(section, key) is string value) && bool.TryParse(value, out var boolValue)) {
            return boolValue;
        }
        return defaultValue;
    }
}
