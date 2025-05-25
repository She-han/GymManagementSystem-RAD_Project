using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.Services
{
    public static class MemberService
    {
        public static string GetNextMemberId()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // Get max existing MemberId
            var cmd = new SqliteCommand("SELECT MemberId FROM Members ORDER BY Id DESC LIMIT 1", conn);
            var lastId = cmd.ExecuteScalar() as string;
            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastId) && lastId.Length >= 6)
            {
                // Extract number part, e.g., MEM005 -> 5
                if (int.TryParse(lastId.Substring(3), out int parsedNum))
                    nextNum = parsedNum + 1;
            }
            return $"MEM{nextNum:D3}";
        }
    }
}