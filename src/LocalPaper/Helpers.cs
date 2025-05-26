namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.IO;
using Medo.Configuration;

internal static class Helpers {

    internal static IEnumerable<KeyValuePair<string, string>> GetConfigEntries(DirectoryInfo directory, DateOnly date) {
        var directories = new List<FileInfo>(directory.GetFiles("*.ini"));
        directories.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

        foreach (var file in directories) {
            var ini = new IniFile(file.FullName);
            foreach (var section in ini.GetSections()) {
                var sectionParts = section.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (sectionParts.Length != 3) { continue; }
                if (!int.TryParse(sectionParts[0], out int year)) {
                    if ("*".Equals(sectionParts[0], StringComparison.Ordinal)) {
                        year = 0;
                    } else {
                        continue;
                    }
                }
                if (!int.TryParse(sectionParts[1], out int month)) {
                    if ("*".Equals(sectionParts[1], StringComparison.Ordinal)) {
                        month = 0;
                    } else {
                        continue;
                    }
                }
                if (!int.TryParse(sectionParts[2], out int day)) {
                    if ("*".Equals(sectionParts[2], StringComparison.Ordinal)) {
                        day = 0;
                    } else {
                        continue;
                    }
                }

                var isMatch = (year == 0 || year == date.Year) &&
                              (month == 0 || month == date.Month) &&
                              (day == 0 || day == date.Day);

                if (isMatch) {
                    foreach (var key in ini.GetKeys(section)) {
                        foreach (var value in ini.ReadAll(section, key)) {
                            yield return new KeyValuePair<string, string>(key, value);
                        }
                    }
                }
            }
        }
    }
}
