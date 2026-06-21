using System;
using MySql.Data.MySqlClient;

namespace CyberSecurityGUI.Data
{
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString =
            "Server=localhost;Port=3306;Database=cybersecurity_db;Uid=root;Pwd=JuniorBasil@2004;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString); 
        }
         
        public static void EnsureDatabaseExists()
        {
            try
            {
                string rootConnString = "Server=localhost;Port=3306;Uid=root;Pwd=JuniorBasil@2004;";
                using (var conn = new MySqlConnection(rootConnString))
                {
                    conn.Open();
                    using var createDbCmd = new MySqlCommand(
                        "CREATE DATABASE IF NOT EXISTS cybersecurity_db;", conn);
                    createDbCmd.ExecuteNonQuery();
                }

                using (var conn = GetConnection())
                {
                    conn.Open();
                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS Tasks (
     
Id INT AUTO_INCREMENT PRIMARY KEY,
                            Title VARCHAR(255) NOT NULL,
                            Description TEXT,
                            ReminderDate DATETIME NULL,
                            IsCompleted TINYINT(1) NOT NULL DEFAULT 0,
                            CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                        );";
                    using var createTableCmd = new MySqlCommand(createTableSql, conn);
                    createTableCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB INIT WARNING] {ex.Message}");
            }
        }
    }
}