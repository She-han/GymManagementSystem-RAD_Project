using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GymManagementSystem.DAL;
using GymManagementSystem.Models;


namespace GymManagementSystem.Services
{
    using Microsoft.Data.Sqlite;
    using System;
    using System.Windows;




    public class InsertData
    {
        public static void AddPayment(Payment payment)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Payments (PaymentId, MemberId, Amount, Date, Status)
                            VALUES (@PaymentId, @MemberId, @Amount, @Date, @Status);";
            cmd.Parameters.AddWithValue("@PaymentId", payment.PaymentId);
            cmd.Parameters.AddWithValue("@MemberId", payment.MemberId);
            cmd.Parameters.AddWithValue("@Amount", payment.Amount);
            cmd.Parameters.AddWithValue("@Date", payment.Date);
            cmd.Parameters.AddWithValue("@Status", payment.Status ?? "");

            cmd.ExecuteNonQuery();
        }
    }

    public static class PaymentService
    {

        public static List<Payment> GetAllPament()
        {
            var payment = new List<Payment>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            var cmd = new SqliteCommand("SELECT * FROM Payments", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                payment.Add(new Payment
                {
                    Id = reader.GetInt32(0),
                    PaymentId = reader.GetString(1),
                    MemberId = reader.GetInt32(2),
                    Amount = reader.GetDouble(3),
                    Date = reader.GetString(4),
                    Status = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }
            return payment;
        }
    }
}