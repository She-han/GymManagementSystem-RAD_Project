using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.Services
{
    public static class PaymentService
    {
        public static string GetNextPaymentId()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            var cmd = new SqliteCommand("SELECT PaymentId FROM Payments ORDER BY Id DESC LIMIT 1", conn);
            var lastId = cmd.ExecuteScalar() as string;
            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastId) && lastId.Length >= 6)
            {
                if (int.TryParse(lastId.Substring(3), out int parsedNum))
                    nextNum = parsedNum + 1;
            }
            return $"PAY{nextNum:D3}";
        }
    }
}