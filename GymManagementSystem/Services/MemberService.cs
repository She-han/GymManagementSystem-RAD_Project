using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagementSystem.DAL;
using GymManagementSystem.Models;
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

   

        // Insert a new member into the table
        public static void InsertData(Member member)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            var cmd = new SqliteCommand(@"
            INSERT INTO Member (MemberId, FullName, TrainerName, JoinDate, SubscriptionType, ContactNumber, MedicalHistory)
            VALUES (@MemberId, @FullName, @TrainerName, @JoinDate, @SubscriptionType, @ContactNumber, @MedicalHistory);
        ", conn);

            cmd.Parameters.AddWithValue("@MemberId", member.MemberId);
            cmd.Parameters.AddWithValue("@FullName", member.FullName);
            cmd.Parameters.AddWithValue("@TrainerName", member.TrainerName ?? "");
            cmd.Parameters.AddWithValue("@JoinDate", member.JoinDate ?? "");
            cmd.Parameters.AddWithValue("@SubscriptionType", member.SubscriptionType ?? "");
            cmd.Parameters.AddWithValue("@ContactNumber", member.ContactNumber ?? "");
            cmd.Parameters.AddWithValue("@MedicalHistory", member.MedicalHistory ?? "");

            cmd.ExecuteNonQuery();
        }


        public static List<Member> GetAllMembers() { 
          List<Member> members = new List<Member>();
          using var conn = DatabaseHelper.GetConnection();
          conn.Open();
            var cmd = new SqliteCommand("SELECT * FROM member",conn);
           using var reader= cmd.ExecuteReader();
            while (reader.Read()) {
                members.Add(new Member() {
                    Id = reader.GetInt32(0),
                    MemberId = reader.GetString(1),
                    FullName = reader.GetString(2),

                });
            }
            return members;
        }
    }
}