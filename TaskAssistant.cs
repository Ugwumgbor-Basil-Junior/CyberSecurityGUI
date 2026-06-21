using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CyberSecurityGUI.Data;

namespace CyberSecurityGUI.Services
{
    // ===== NLP SIMULATION + TASK ASSISTANT (Task 1 & Task 3) =====
    // Performs simple string-manipulation based "NLP" (string.Contains / regex) to detect
    // intents like "add task", "set a reminder", "show activity log" even when phrased
    // differently, and routes them to the TaskRepository / ActivityLogger.
    public class TaskAssistant
    {
        private readonly TaskRepository _repo = new();

        // Tracks the most recently added task id so a follow-up like
        // "remind me in 3 days" can attach a reminder without re-stating the task.
        private int? _pendingTaskId;
        private string? _pendingTaskTitle;

        public bool TryHandle(string rawInput, out string response) 
        {
            string input = rawInput.Trim();
            string lower = input.ToLower();

            // ---------- ACTIVITY LOG ----------
            if (lower.Contains("activity log") || lower.Contains("what have you done") ||
                lower.Contains("show log") || lower.Contains("recent actions"))
            {
                response = lower.Contains("full") || lower.Contains("show more")
                    ? ActivityLogger.FormatAll()
                    : ActivityLogger.FormatRecent();
                return true;
            }

            // ---------- VIEW TASKS ----------
            if ((lower.Contains("show") || lower.Contains("view") || lower.Contains("list")) && lower.Contains("task"))
            {
                var tasks = _repo.GetAllTasks();
                if (tasks.Count == 0)
                {
                    response = "You don't have any tasks yet. Try: \"Add task - Enable two-factor authentication\"";
                    return true;
                }
                var sb = new System.Text.StringBuilder("Here are your cybersecurity tasks:\n");
                foreach (var t in tasks.Take(10))
                    sb.AppendLine($"• [{t.StatusDisplay}] {t.Title} — {t.ReminderDisplay}");
                response = sb.ToString().TrimEnd();
                return true;
            }

            // ---------- REMINDER FOLLOW-UP (e.g. "remind me in 3 days") ----------
            if (_pendingTaskId.HasValue && (lower.Contains("remind") || Regex.IsMatch(lower, @"\b(yes|tomorrow|\d+\s*day)")))
            {
                DateTime? reminder = ExtractDate(lower);
                if (reminder.HasValue)
                {
                    _repo.SetReminder(_pendingTaskId.Value, reminder.Value);
                    ActivityLogger.Log($"Reminder set for '{_pendingTaskTitle}' on {reminder.Value:dd MMM yyyy}");
                    response = $"Got it! I'll remind you about \"{_pendingTaskTitle}\" on {reminder.Value:dddd, dd MMM yyyy}.";
                    _pendingTaskId = null;
                    _pendingTaskTitle = null;
                    return true;
                }
                if (lower.Contains("no") || lower.Contains("skip"))
                {
                    response = "No problem, no reminder set. Let me know if you change your mind!";
                    _pendingTaskId = null;
                    _pendingTaskTitle = null;
                    return true;
                }
            }

            // ---------- ADD TASK (NLP: many phrasings) ----------
            // e.g. "add task - Review privacy settings", "add a task to enable 2FA",
            // "create task: update password", "remind me to update my password tomorrow"
            var taskMatch = Regex.Match(lower,
                @"(?:add(?:\s+a)?\s+task(?:\s+to)?|create(?:\s+a)?\s+task(?:\s+to)?|new task(?:\s+to)?)\s*[:\-]?\s*(.+)",
                RegexOptions.IgnoreCase);

            var reminderMatch = Regex.Match(lower,
                @"remind(?:\s+me)?\s+to\s+(.+?)(\s+(?:tomorrow|in\s+\d+\s*days?|on\s+.+))?$",
                RegexOptions.IgnoreCase);

            if (taskMatch.Success)
            {
                string title = CleanTitle(taskMatch.Groups[1].Value);
                DateTime? reminder = ExtractDate(lower);
                int id = _repo.AddTask(title, $"Cybersecurity task: {title}", reminder);
                ActivityLogger.Log($"Task added: '{title}'" + (reminder.HasValue ? $" (reminder set for {reminder:dd MMM yyyy})" : " (no reminder set)"));

                if (reminder.HasValue)
                {
                    response = $"Task added: \"{title}\". Reminder set for {reminder:dddd, dd MMM yyyy}. ✅";
                }
                else
                {
                    _pendingTaskId = id;
                    _pendingTaskTitle = title;
                    response = $"Task added with the description \"{title}\". Would you like a reminder? (e.g. \"remind me in 3 days\")";
                }
                return true;
            }

            if (reminderMatch.Success)
            {
                string title = CleanTitle(reminderMatch.Groups[1].Value);
                DateTime? reminder = ExtractDate(lower) ?? DateTime.Now.AddDays(1);
                int id = _repo.AddTask(title, $"Cybersecurity task: {title}", reminder);
                ActivityLogger.Log($"Reminder set for '{title}' on {reminder:dd MMM yyyy}");
                response = $"Reminder set for \"{title}\" on {reminder:dddd, dd MMM yyyy}.";
                return true;
            }

            // ---------- MARK TASK COMPLETE / DELETE (basic NLP) ----------
            if (lower.Contains("complete") && lower.Contains("task"))
            {
                var tasks = _repo.GetAllTasks();
                var match = tasks.FirstOrDefault(t => lower.Contains(t.Title.ToLower()));
                if (match != null)
                {
                    _repo.MarkCompleted(match.Id);
                    ActivityLogger.Log($"Task marked complete: '{match.Title}'");
                    response = $"Marked \"{match.Title}\" as completed. Nice work staying secure! ✅";
                    return true;
                }
            }

            response = "";
            return false;
        }

        private static string CleanTitle(string raw)
        {
            string title = raw.Trim();
            title = Regex.Replace(title, @"\s+(tomorrow|in\s+\d+\s*days?|on\s+.+)$", "", RegexOptions.IgnoreCase);
            title = title.Trim(' ', '-', ':', '.');
            if (title.Length > 0)
                title = char.ToUpper(title[0]) + title.Substring(1);
            return string.IsNullOrWhiteSpace(title) ? "Untitled cybersecurity task" : title;
        }

        // Very small "NLP-style" date extraction using keyword + regex matching.
        private static DateTime? ExtractDate(string lower)
        {
            if (lower.Contains("tomorrow"))
                return DateTime.Now.AddDays(1);

            var inDays = Regex.Match(lower, @"in\s+(\d+)\s*day");
            if (inDays.Success && int.TryParse(inDays.Groups[1].Value, out int days))
                return DateTime.Now.AddDays(days);

            var weekMatch = Regex.Match(lower, @"in\s+(\d+)\s*week");
            if (weekMatch.Success && int.TryParse(weekMatch.Groups[1].Value, out int weeks))
                return DateTime.Now.AddDays(weeks * 7);

            if (lower.Contains("next week"))
                return DateTime.Now.AddDays(7);

            // standalone "3 days" / "yes" responses during follow-up
            var bareDays = Regex.Match(lower, @"^(\d+)\s*days?$");
            if (bareDays.Success && int.TryParse(bareDays.Groups[1].Value, out int bd))
                return DateTime.Now.AddDays(bd);

            return null;
        }
    }
}
