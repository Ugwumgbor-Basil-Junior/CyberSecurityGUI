-- database_setup.sql
-- Cybersecurity Awareness Chatbot — Task Assistant database schema.
--
-- This script is provided for reference / manual setup. The application
-- itself will also create the database and table automatically on first
-- run via DatabaseHelper.EnsureDatabaseExists(), as long as the MySQL
-- server is reachable with the credentials in DatabaseHelper.cs.

CREATE DATABASE IF NOT EXISTS cybersecurity_db;

USE cybersecurity_db;

CREATE TABLE IF NOT EXISTS Tasks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    ReminderDate DATETIME NULL,
    IsCompleted TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
