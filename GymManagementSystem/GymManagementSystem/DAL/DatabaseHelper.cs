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
        private const string ConnectionString = "Data Source=gym.db;Cache=Shared;";
        private static readonly object _lock = new object();

        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            return connection;
        }

        // Initialize database with WAL mode for better concurrency
        public static void InitializeDatabase()
        {
            lock (_lock)
            {
                using var conn = GetConnection();
                conn.Open();

                // Enable WAL mode for better concurrent access
                using var pragmaCmd = conn.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA cache_size=10000;";
                pragmaCmd.ExecuteNonQuery();

                // Create all tables
                EnsureAllTablesExist();
            }
        }

        // Call this once at app start to ensure all tables exist
        public static void EnsureAllTablesExist()
        {
            using var conn = GetConnection();
            conn.Open();

            using var transaction = conn.BeginTransaction();
            try
            {
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
                        Specialty TEXT,
                        Experience TEXT
                    );",
                    
                    // Equipment table (independent table)
                    @"CREATE TABLE IF NOT EXISTS Equipment (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        EquipmentId TEXT NOT NULL UNIQUE,
                        Name TEXT NOT NULL,
                        Quantity INTEGER NOT NULL DEFAULT 0,
                        Condition TEXT,
                        Category TEXT
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
                    cmd.Transaction = transaction;
                    cmd.CommandText = command;
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
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

        // Enhanced ExecuteNonQuery with transaction support
        public static int ExecuteNonQuery(string query, params SqliteParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = query;
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                var result = cmd.ExecuteNonQuery();
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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

        // Enhanced method for batch operations
        public static void ExecuteBatch(List<(string query, SqliteParameter[] parameters)> operations)
        {
            using var conn = GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                foreach (var (query, parameters) in operations)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = query;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Database maintenance methods
        public static void VacuumDatabase()
        {
            lock (_lock)
            {
                using var conn = GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "VACUUM";
                cmd.ExecuteNonQuery();
            }
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

        // Enhanced backup method
        

        // Method to restore database
        public static bool RestoreDatabase(string backupPath)
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(backupPath))
                    {
                        throw new FileNotFoundException("Backup file not found");
                    }

                    string currentDbPath = "gym.db";

                    // Close all connections before restoring
                    SqliteConnection.ClearAllPools();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Copy the backup file to replace current database
                    File.Copy(backupPath, currentDbPath, true);

                    // Reinitialize database
                    InitializeDatabase();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Restore failed: {ex.Message}");
                    return false;
                }
            }
        }

        // Method to clear connection pool (helps with database locking)
        public static void ClearConnectionPool()
        {
            SqliteConnection.ClearAllPools();
        }

        // Method to check database integrity
        public static bool CheckDatabaseIntegrity()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA integrity_check";
                var result = cmd.ExecuteScalar()?.ToString();
                return result == "ok";
            }
            catch
            {
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

        // Method to generate unique IDs
        public static string GenerateUniqueId(string prefix)
        {
            return $"{prefix}{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(100, 999)}";
        }

        // Method to test database connection
        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}