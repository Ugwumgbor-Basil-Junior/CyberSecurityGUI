using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CyberSecurityGUI.Data
{
    // ===== TASK MODEL (DB-backed cybersecurity task) =====
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        // Friendly display helpers used directly by the XAML bindings.
        public string StatusDisplay => IsCompleted ? "DONE" : "PENDING";
        public string ReminderDisplay => ReminderDate.HasValue 
            ? $"Reminder: {ReminderDate.Value:dd MMM yyyy}"
            : "No reminder set";
    }

    // ===== TASK ASSISTANT DATA ACCESS (Task 1 - DB Integration) =====
    // Handles all CRUD operations against the Tasks table created by
    // DatabaseHelper.EnsureDatabaseExists().
    public class TaskRepository
    {
        public List<CyberTask> GetAllTasks()
        {
            var tasks = new List<CyberTask>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string sql = "SELECT Id, Title, Description, ReminderDate, IsCompleted, CreatedAt " +
                         "FROM Tasks ORDER BY IsCompleted ASC, CreatedAt DESC;";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(new CyberTask
                {
                    Id = reader.GetInt32("Id"),
                    Title = reader.GetString("Title"),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                    ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? null : reader.GetDateTime("ReminderDate"),
                    IsCompleted = reader.GetBoolean("IsCompleted"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return tasks;
        }

        public int AddTask(string title, string description, DateTime? reminderDate)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string sql = "INSERT INTO Tasks (Title, Description, ReminderDate, IsCompleted) " +
                         "VALUES (@title, @desc, @reminder, 0); SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@desc", description ?? "");
            cmd.Parameters.AddWithValue("@reminder", (object?)reminderDate ?? DBNull.Value);

            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        public void SetReminder(int taskId, DateTime reminderDate)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string sql = "UPDATE Tasks SET ReminderDate = @reminder WHERE Id = @id;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@reminder", reminderDate);
            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.ExecuteNonQuery();
        }

        public void MarkCompleted(int taskId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string sql = "UPDATE Tasks SET IsCompleted = 1 WHERE Id = @id;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteTask(int taskId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string sql = "DELETE FROM Tasks WHERE Id = @id;";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.ExecuteNonQuery();
        }
    }
}
