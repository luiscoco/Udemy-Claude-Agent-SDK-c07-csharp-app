using System.Text.RegularExpressions;

namespace LessonC07.App.Lab;

/// <summary>Minimal .env support: KEY=VALUE lines, # comments. Existing environment variables win.</summary>
public static class DotEnv
{
    /// <summary>Finds .env in the current directory or its parents, stopping at the solution root.</summary>
    public static string Locate()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var d = dir; d is not null; d = d.Parent)
        {
            var candidate = Path.Combine(d.FullName, ".env");
            if (File.Exists(candidate)) return candidate;
            if (File.Exists(Path.Combine(d.FullName, "LessonC07.slnx"))) return candidate;
        }
        return Path.Combine(dir.FullName, ".env");
    }

    public static void Load(string path)
    {
        if (!File.Exists(path)) return;
        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
            var eq = trimmed.IndexOf('=');
            if (eq <= 0) continue;
            var key = trimmed[..eq].Trim();
            var value = trimmed[(eq + 1)..].Trim().Trim('"');
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    /// <summary>Sets or replaces KEY=value in the file, writing immediately.</summary>
    public static void Save(string path, string key, string value)
    {
        var contents = File.Exists(path) ? File.ReadAllText(path) : "";
        var pattern = new Regex("^\\s*" + Regex.Escape(key) + "=.*$", RegexOptions.Multiline);
        contents = pattern.IsMatch(contents)
            ? pattern.Replace(contents, key + "=" + value, 1)
            : contents.TrimEnd() + "\n" + key + "=" + value + "\n";
        File.WriteAllText(path, contents);
        Environment.SetEnvironmentVariable(key, value);
    }
}
