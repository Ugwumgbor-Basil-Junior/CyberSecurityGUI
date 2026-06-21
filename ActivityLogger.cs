using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberSecurityGUI.Services
{
    // ===== ACTIVITY LOG (Task 4) =====
    // A simple in-memory list of everything the bot has done this session.
    // Static so every part of the app (chat, tasks, quiz, NLP) can log to the same place.
    public static class ActivityLogger
    {
        private static readonly List<string> _entries = new();

        public static void Log(string description)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {description}";
            _entries.Add(entry);
        }

        // Returns the most recent entries, newest first. Default keeps the log concise (5-10 items).
        public static List<string> GetRecent(int count = 8)
        {
            return _entries
                .Skip(Math.Max(0, _entries.Count - count))
                .Reverse()
                .ToList();
        }

        public static List<string> GetAll()
        {
            return _entries.AsEnumerable().Reverse().ToList();
        }

        public static string FormatRecent(int count = 8)
        {
            var recent = GetRecent(count);
            if (recent.Count == 0)
                return "No activity recorded yet. Try adding a task or starting the quiz!";

            var sb = new StringBuilder();
            sb.AppendLine("Here's a summary of recent actions:");
            for (int i = 0; i < recent.Count; i++)
                sb.AppendLine($"{i + 1}. {recent[i]}");

            if (_entries.Count > count)
                sb.Append($"\n...and {_entries.Count - count} more. Type \"show full log\" to see everything.");

            return sb.ToString().TrimEnd();
        }

        public static string FormatAll()
        {
            var all = GetAll();
            if (all.Count == 0)
                return "No activity recorded yet.";

            var sb = new StringBuilder();
            sb.AppendLine($"Full activity log ({all.Count} actions):");
            for (int i = 0; i < all.Count; i++)
                sb.AppendLine($"{i + 1}. {all[i]}");
            return sb.ToString().TrimEnd();
        }
    }
}
