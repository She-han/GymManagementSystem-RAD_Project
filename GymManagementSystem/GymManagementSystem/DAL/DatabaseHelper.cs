using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;

namespace GymManagementSystem.DAL
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Data Source=gym.db";

        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
        }

        // Call this once at app start to ensure all tables exist
        public static void EnsureAllTablesExist()
        {
            using var conn = GetConnection();
            conn.Open();

            // Create all tables in the correct order (considering foreign key dependencies)
            var commands = new[]
            {
                // Admins table
                @"CREATE TABLE IF NOT EXISTS Admins (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL
                );",
                
                // Trainers table (independent table)
                @"CREATE TABLE IF NOT EXISTS Trainers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TrainerId TEXT NOT NULL UNIQUE,
                    FullName TEXT NOT NULL,
                    ContactNumber TEXT,
                    Specialty TEXT
                );",
                
                // Equipment table (independent table)
                @"CREATE TABLE IF NOT EXISTS Equipment (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EquipmentId TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    Quantity INTEGER NOT NULL DEFAULT 0,
                    Condition TEXT
                );",
                
                // Members table (references Trainers)
                @"CREATE TABLE IF NOT EXISTS Members (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MemberId TEXT NOT NULL UNIQUE,
                    FullName TEXT NOT NULL,
                    TrainerName TEXT,
                    JoinDate TEXT,
                    SubscriptionType TEXT,
                    ContactNumber TEXT,
                    MedicalHistory TEXT
                );",
                
                // Payments table (references Members)
                @"CREATE TABLE IF NOT EXISTS Payments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PaymentId TEXT NOT NULL UNIQUE,
                    MemberId INTEGER NOT NULL,
                    Amount REAL NOT NULL DEFAULT 0.0,
                    Date TEXT NOT NULL,
                    FOREIGN KEY (MemberId) REFERENCES Members(Id) ON DELETE CASCADE
                );",
                
                // Attendance table (references Members)
                @"CREATE TABLE IF NOT EXISTS Attendance (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MemberId INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    TimeIn TEXT,
                    TimeOut TEXT,
                    FOREIGN KEY (MemberId) REFERENCES Members(Id) ON DELETE CASCADE,
                    UNIQUE(MemberId, Date)
                );"
            };

            foreach (var command in commands)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = command;
                cmd.ExecuteNonQuery();
            }
        }

        // Keep the old method for backward compatibility
        public static void EnsureAdminsTableExists()
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Admins (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL
);";
            cmd.ExecuteNonQuery();
        }

        // Helper method to check if a table exists
        public static bool TableExists(string tableName)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
            cmd.Parameters.AddWithValue("@tableName", tableName);
            var result = cmd.ExecuteScalar();
            return result != null;
        }

        // Helper method to get table schema information
        public static List<string> GetTableColumns(string tableName)
        {
            var columns = new List<string>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({tableName})";
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                columns.Add(reader.GetString("name"));
            }

            return columns;
        }

        // Helper method to execute a query and return the number of affected rows
        public static int ExecuteNonQuery(string query, params SqliteParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteNonQuery();
        }

        // Helper method to execute a query and return a single value
        public static object ExecuteScalar(string query, params SqliteParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteScalar();
        }

        // Helper method to execute a query and return a list of results
        public static List<Dictionary<string, object>> ExecuteQuery(string query, params SqliteParameter[] parameters)
        {
            var results = new List<Dictionary<string, object>>();

            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(row);
            }

            return results;
        }

        // Database maintenance methods
        public static void VacuumDatabase()
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "VACUUM";
            cmd.ExecuteNonQuery();
        }

        public static long GetDatabaseSize()
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT page_count * page_size as size FROM pragma_page_count(), pragma_page_size()";
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt64(result) : 0;
        }

        // Method to backup database (simple file copy approach)
        public static bool BackupDatabase(string backupPath)
        {
            try
            {
                // Get the current database file path
                string currentDbPath = "gym.db";

                // Ensure the backup directory exists
                string backupDir = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Copy the database file
                File.Copy(currentDbPath, backupPath, true);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Backup failed: {ex.Message}");
                return false;
            }
        }

        // Method to restore database (simple file copy approach)
        public static bool RestoreDatabase(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("Backup file not found");
                }

                string currentDbPath = "gym.db";

                // Close all connections before restoring
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Copy the backup file to replace current database
                File.Copy(backupPath, currentDbPath, true);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Restore failed: {ex.Message}");
                return false;
            }
        }

        // Alternative backup method using SQL commands
        public static bool BackupDatabaseWithSql(string backupPath)
        {
            try
            {
                using var sourceConn = GetConnection();
                using var backupConn = new SqliteConnection($"Data Source={backupPath}");

                sourceConn.Open();
                backupConn.Open();

                // Get all table names
                var tables = new List<string>();
                using var cmd = sourceConn.CreateCommand();
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
                reader.Close();

                // Copy each table
                foreach (var table in tables)
                {
                    // Get table schema
                    cmd.CommandText = $"SELECT sql FROM sqlite_master WHERE name='{table}'";
                    var createSql = cmd.ExecuteScalar()?.ToString();

                    if (!string.IsNullOrEmpty(createSql))
                    {
                        // Create table in backup database
                        using var backupCmd = backupConn.CreateCommand();
                        backupCmd.CommandText = createSql;
                        backupCmd.ExecuteNonQuery();

                        // Copy data
                        cmd.CommandText = $"SELECT * FROM {table}";
                        using var dataReader = cmd.ExecuteReader();

                        while (dataReader.Read())
                        {
                            var columnValues = new object[dataReader.FieldCount];
                            dataReader.GetValues(columnValues);

                            var insertSql = $"INSERT INTO {table} VALUES ({string.Join(",", columnValues.Select(v => v == null ? "NULL" : $"'{v}'"))})";
                            backupCmd.CommandText = insertSql;
                            backupCmd.ExecuteNonQuery();
                        }
                        dataReader.Close();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Backup failed: {ex.Message}");
                return false;
            }
        }

        // Get current date and time in consistent format
        public static string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // Get current date only
        public static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        // Get current time only
        public static string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}