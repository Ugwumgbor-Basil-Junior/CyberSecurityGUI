using System;

namespace CyberSecurityGUI.Models
{
    // ===== TASK MODEL (maps to the Tasks table in MySQL) =====
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string ReminderDisplay =>
            ReminderDate.HasValue ? $"Reminder: {ReminderDate.Value:ddd, dd MMM yyyy}" : "No reminder set";

        public string StatusDisplay => IsCompleted ? "✔ Completed" : "⏳ Pending";
    }
}
