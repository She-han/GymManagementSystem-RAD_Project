using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GymManagementSystem.Models;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace GymManagementSystem.Services
{
    public static class PdfReportGenerator
    {
        private static readonly Font TitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
        private static readonly Font HeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(46, 196, 182));
        private static readonly Font SubHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
        private static readonly Font NormalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
        private static readonly Font SmallFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY);

        public static string GenerateMemberReport(DateTime? startDate, DateTime? endDate, string savePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(document, new FileStream(savePath, FileMode.Create));
                document.Open();

                // Add header
                AddReportHeader(document, "Member Report");
                AddDateRange(document, startDate, endDate);

                // Fetch members data
                var members = GetMembersData(startDate, endDate);

                // Add summary
                Paragraph summary = new Paragraph($"Total Members: {members.Count}", SubHeaderFont);
                summary.SpacingAfter = 10f;
                document.Add(summary);

                // Create table
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 10f, 20f, 15f, 15f, 20f, 20f });

                // Add headers
                AddTableHeader(table, new[] { "ID", "Full Name", "Member ID", "Contact", "Subscription", "Join Date" });

                // Add data rows
                foreach (var member in members)
                {
                    AddTableCell(table, member.Id.ToString());
                    AddTableCell(table, member.FullName);
                    AddTableCell(table, member.MemberId);
                    AddTableCell(table, member.ContactNumber ?? "N/A");
                    AddTableCell(table, member.SubscriptionType ?? "N/A");
                    AddTableCell(table, member.JoinDate ?? "N/A");
                }

                document.Add(table);
                AddFooter(document);
                document.Close();

                return savePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating member report: {ex.Message}");
            }
        }

        public static string GenerateEquipmentReport(DateTime? startDate, DateTime? endDate, string savePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(document, new FileStream(savePath, FileMode.Create));
                document.Open();

                AddReportHeader(document, "Equipment Report");
                AddDateRange(document, startDate, endDate);

                var equipment = GetEquipmentData(startDate, endDate);

                // Summary statistics
                var totalItems = equipment.Sum(e => e.Quantity);
                var goodCondition = equipment.Where(e => e.Condition?.ToLower() == "good").Sum(e => e.Quantity);
                var needMaintenance = equipment.Where(e => e.Condition?.ToLower() == "poor" || e.Condition?.ToLower() == "fair").Sum(e => e.Quantity);

                Paragraph summary = new Paragraph($"Total Equipment Types: {equipment.Count} | Total Items: {totalItems} | Good Condition: {goodCondition} | Need Maintenance: {needMaintenance}", SubHeaderFont);
                summary.SpacingAfter = 10f;
                document.Add(summary);

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 15f, 25f, 15f, 20f, 25f });

                AddTableHeader(table, new[] { "Equipment ID", "Name", "Quantity", "Condition", "Category" });

                foreach (var eq in equipment)
                {
                    AddTableCell(table, eq.EquipmentId);
                    AddTableCell(table, eq.Name);
                    AddTableCell(table, eq.Quantity.ToString());
                    AddTableCell(table, eq.Condition ?? "N/A");
                    AddTableCell(table, eq.Category ?? "N/A");
                }

                document.Add(table);
                AddFooter(document);
                document.Close();

                return savePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating equipment report: {ex.Message}");
            }
        }

        public static string GenerateTrainerReport(DateTime? startDate, DateTime? endDate, string savePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(document, new FileStream(savePath, FileMode.Create));
                document.Open();

                AddReportHeader(document, "Trainer Report");
                AddDateRange(document, startDate, endDate);

                var trainers = GetTrainersData(startDate, endDate);

                Paragraph summary = new Paragraph($"Total Trainers: {trainers.Count}", SubHeaderFont);
                summary.SpacingAfter = 10f;
                document.Add(summary);

                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 10f, 20f, 15f, 15f, 20f, 20f });

                AddTableHeader(table, new[] { "ID", "Full Name", "Trainer ID", "Contact", "Specialty", "Experience" });

                foreach (var trainer in trainers)
                {
                    AddTableCell(table, trainer.Id.ToString());
                    AddTableCell(table, trainer.FullName);
                    AddTableCell(table, trainer.TrainerId);
                    AddTableCell(table, trainer.ContactNumber ?? "N/A");
                    AddTableCell(table, trainer.Specialty ?? "N/A");
                    AddTableCell(table, trainer.Experience ?? "N/A");
                }

                document.Add(table);
                AddFooter(document);
                document.Close();

                return savePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating trainer report: {ex.Message}");
            }
        }

        public static string GeneratePaymentReport(DateTime? startDate, DateTime? endDate, string savePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(document, new FileStream(savePath, FileMode.Create));
                document.Open();

                AddReportHeader(document, "Payment Report");
                AddDateRange(document, startDate, endDate);

                var payments = GetPaymentsData(startDate, endDate);
                var totalRevenue = payments.Sum(p => p.Amount);

                Paragraph summary = new Paragraph($"Total Payments: {payments.Count} | Total Revenue: ${totalRevenue:N2}", SubHeaderFont);
                summary.SpacingAfter = 10f;
                document.Add(summary);

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 15f, 20f, 20f, 20f, 25f });

                AddTableHeader(table, new[] { "Payment ID", "Member ID", "Amount", "Status", "Date" });

                foreach (var payment in payments)
                {
                    AddTableCell(table, payment.PaymentId ?? "N/A");
                    AddTableCell(table, payment.MemberId.ToString());
                    AddTableCell(table, $"${payment.Amount:N2}");
                    AddTableCell(table, payment.Status ?? "N/A");
                    AddTableCell(table, payment.Date ?? "N/A");
                }

                document.Add(table);
                AddFooter(document);
                document.Close();

                return savePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating payment report: {ex.Message}");
            }
        }

        public static string GenerateAllReports(DateTime? startDate, DateTime? endDate, string savePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(document, new FileStream(savePath, FileMode.Create));
                document.Open();

                AddReportHeader(document, "Complete Gym Management Report");
                AddDateRange(document, startDate, endDate);

                // Members Section
                document.Add(new Paragraph("MEMBERS", HeaderFont) { SpacingBefore = 10f, SpacingAfter = 10f });
                var members = GetMembersData(startDate, endDate);
                document.Add(new Paragraph($"Total Members: {members.Count}", NormalFont) { SpacingAfter = 5f });

                PdfPTable memberTable = new PdfPTable(4);
                memberTable.WidthPercentage = 100;
                memberTable.SetWidths(new float[] { 25f, 25f, 25f, 25f });
                AddTableHeader(memberTable, new[] { "Full Name", "Member ID", "Contact", "Join Date" });
                foreach (var member in members)
                {
                    AddTableCell(memberTable, member.FullName);
                    AddTableCell(memberTable, member.MemberId);
                    AddTableCell(memberTable, member.ContactNumber ?? "N/A");
                    AddTableCell(memberTable, member.JoinDate ?? "N/A");
                }
                document.Add(memberTable);

                // Equipment Section
                document.Add(new Paragraph("EQUIPMENT", HeaderFont) { SpacingBefore = 20f, SpacingAfter = 10f });
                var equipment = GetEquipmentData(startDate, endDate);
                document.Add(new Paragraph($"Total Equipment Types: {equipment.Count} | Total Items: {equipment.Sum(e => e.Quantity)}", NormalFont) { SpacingAfter = 5f });

                PdfPTable equipTable = new PdfPTable(4);
                equipTable.WidthPercentage = 100;
                equipTable.SetWidths(new float[] { 30f, 20f, 25f, 25f });
                AddTableHeader(equipTable, new[] { "Name", "Quantity", "Condition", "Category" });
                foreach (var eq in equipment)
                {
                    AddTableCell(equipTable, eq.Name);
                    AddTableCell(equipTable, eq.Quantity.ToString());
                    AddTableCell(equipTable, eq.Condition ?? "N/A");
                    AddTableCell(equipTable, eq.Category ?? "N/A");
                }
                document.Add(equipTable);

                // Trainers Section
                document.Add(new Paragraph("TRAINERS", HeaderFont) { SpacingBefore = 20f, SpacingAfter = 10f });
                var trainers = GetTrainersData(startDate, endDate);
                document.Add(new Paragraph($"Total Trainers: {trainers.Count}", NormalFont) { SpacingAfter = 5f });

                PdfPTable trainerTable = new PdfPTable(4);
                trainerTable.WidthPercentage = 100;
                trainerTable.SetWidths(new float[] { 30f, 25f, 25f, 20f });
                AddTableHeader(trainerTable, new[] { "Full Name", "Contact", "Specialty", "Experience" });
                foreach (var trainer in trainers)
                {
                    AddTableCell(trainerTable, trainer.FullName);
                    AddTableCell(trainerTable, trainer.ContactNumber ?? "N/A");
                    AddTableCell(trainerTable, trainer.Specialty ?? "N/A");
                    AddTableCell(trainerTable, trainer.Experience ?? "N/A");
                }
                document.Add(trainerTable);

                // Payments Section
                document.Add(new Paragraph("PAYMENTS", HeaderFont) { SpacingBefore = 20f, SpacingAfter = 10f });
                var payments = GetPaymentsData(startDate, endDate);
                var totalRevenue = payments.Sum(p => p.Amount);
                document.Add(new Paragraph($"Total Payments: {payments.Count} | Total Revenue: ${totalRevenue:N2}", NormalFont) { SpacingAfter = 5f });

                PdfPTable paymentTable = new PdfPTable(4);
                paymentTable.WidthPercentage = 100;
                paymentTable.SetWidths(new float[] { 25f, 25f, 25f, 25f });
                AddTableHeader(paymentTable, new[] { "Payment ID", "Amount", "Status", "Date" });
                foreach (var payment in payments)
                {
                    AddTableCell(paymentTable, payment.PaymentId ?? "N/A");
                    AddTableCell(paymentTable, $"${payment.Amount:N2}");
                    AddTableCell(paymentTable, payment.Status ?? "N/A");
                    AddTableCell(paymentTable, payment.Date ?? "N/A");
                }
                document.Add(paymentTable);

                AddFooter(document);
                document.Close();

                return savePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating complete report: {ex.Message}");
            }
        }

        // Helper methods
        private static void AddReportHeader(Document document, string title)
        {
            Paragraph header = new Paragraph("FlexFit Gym Management System", TitleFont);
            header.Alignment = Element.ALIGN_CENTER;
            header.SpacingAfter = 5f;
            document.Add(header);

            Paragraph reportTitle = new Paragraph(title, HeaderFont);
            reportTitle.Alignment = Element.ALIGN_CENTER;
            reportTitle.SpacingAfter = 10f;
            document.Add(reportTitle);

            Paragraph generatedDate = new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", SmallFont);
            generatedDate.Alignment = Element.ALIGN_CENTER;
            generatedDate.SpacingAfter = 15f;
            document.Add(generatedDate);

            document.Add(new LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, -5f));
            document.Add(new Paragraph(" "));
        }

        private static void AddDateRange(Document document, DateTime? startDate, DateTime? endDate)
        {
            string dateRange = "Date Range: ";
            if (startDate.HasValue && endDate.HasValue)
            {
                dateRange += $"{startDate.Value:yyyy-MM-dd} to {endDate.Value:yyyy-MM-dd}";
            }
            else
            {
                dateRange += "All Time";
            }

            Paragraph dateRangePara = new Paragraph(dateRange, NormalFont);
            dateRangePara.SpacingAfter = 10f;
            document.Add(dateRangePara);
        }

        private static void AddTableHeader(PdfPTable table, string[] headers)
        {
            foreach (var header in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, SubHeaderFont));
                cell.BackgroundColor = new BaseColor(46, 196, 182);
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Padding = 8f;
                table.AddCell(cell);
            }
        }

        private static void AddTableCell(PdfPTable table, string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, NormalFont));
            cell.Padding = 5f;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
        }

        private static void AddFooter(Document document)
        {
            document.Add(new Paragraph(" "));
            document.Add(new LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, -5f));
            Paragraph footer = new Paragraph("© 2025 FlexFit - All Rights Reserved", SmallFont);
            footer.Alignment = Element.ALIGN_CENTER;
            footer.SpacingBefore = 10f;
            document.Add(footer);
        }

        // Database query methods
        private static List<Member> GetMembersData(DateTime? startDate, DateTime? endDate)
        {
            var members = new List<Member>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = "SELECT * FROM Members";
            if (startDate.HasValue && endDate.HasValue)
            {
                query += " WHERE date(JoinDate) BETWEEN @startDate AND @endDate";
            }
            query += " ORDER BY Id DESC";

            var cmd = new SqliteCommand(query, conn);
            if (startDate.HasValue && endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@startDate", startDate.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@endDate", endDate.Value.ToString("yyyy-MM-dd"));
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                members.Add(new Member
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    MemberId = reader.GetString(reader.GetOrdinal("MemberId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    TrainerName = reader.IsDBNull(reader.GetOrdinal("TrainerName")) ? null : reader.GetString(reader.GetOrdinal("TrainerName")),
                    JoinDate = reader.IsDBNull(reader.GetOrdinal("JoinDate")) ? null : reader.GetString(reader.GetOrdinal("JoinDate")),
                    SubscriptionType = reader.IsDBNull(reader.GetOrdinal("SubscriptionType")) ? null : reader.GetString(reader.GetOrdinal("SubscriptionType")),
                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? null : reader.GetString(reader.GetOrdinal("ContactNumber")),
                    MedicalHistory = reader.IsDBNull(reader.GetOrdinal("MedicalHistory")) ? null : reader.GetString(reader.GetOrdinal("MedicalHistory"))
                });
            }

            return members;
        }

        private static List<Equipment> GetEquipmentData(DateTime? startDate, DateTime? endDate)
        {
            var equipment = new List<Equipment>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            // Note: Equipment table doesn't have date field, so we return all equipment
            string query = "SELECT * FROM Equipment ORDER BY Id DESC";

            var cmd = new SqliteCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                equipment.Add(new Equipment
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    EquipmentId = reader.GetString(reader.GetOrdinal("EquipmentId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    Condition = reader.IsDBNull(reader.GetOrdinal("Condition")) ? null : reader.GetString(reader.GetOrdinal("Condition")),
                    Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? null : reader.GetString(reader.GetOrdinal("Category"))
                });
            }

            return equipment;
        }

        private static List<Trainer> GetTrainersData(DateTime? startDate, DateTime? endDate)
        {
            var trainers = new List<Trainer>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = "SELECT * FROM Trainers";
            if (startDate.HasValue && endDate.HasValue)
            {
                query += " WHERE date(JoinDate) BETWEEN @startDate AND @endDate";
            }
            query += " ORDER BY Id DESC";

            var cmd = new SqliteCommand(query, conn);
            if (startDate.HasValue && endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@startDate", startDate.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@endDate", endDate.Value.ToString("yyyy-MM-dd"));
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                trainers.Add(new Trainer
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    TrainerId = reader.GetString(reader.GetOrdinal("TrainerId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? null : reader.GetString(reader.GetOrdinal("ContactNumber")),
                    Specialty = reader.IsDBNull(reader.GetOrdinal("Specialty")) ? null : reader.GetString(reader.GetOrdinal("Specialty")),
                    Experience = reader.IsDBNull(reader.GetOrdinal("Experience")) ? null : reader.GetString(reader.GetOrdinal("Experience"))
                });
            }

            return trainers;
        }

        private static List<Payment> GetPaymentsData(DateTime? startDate, DateTime? endDate)
        {
            var payments = new List<Payment>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            string query = "SELECT * FROM Payments";
            if (startDate.HasValue && endDate.HasValue)
            {
                query += " WHERE date(Date) BETWEEN @startDate AND @endDate";
            }
            query += " ORDER BY Id DESC";

            var cmd = new SqliteCommand(query, conn);
            if (startDate.HasValue && endDate.HasValue)
            {
                cmd.Parameters.AddWithValue("@startDate", startDate.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@endDate", endDate.Value.ToString("yyyy-MM-dd"));
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                payments.Add(new Payment
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    PaymentId = reader.IsDBNull(reader.GetOrdinal("PaymentId")) ? null : reader.GetString(reader.GetOrdinal("PaymentId")),
                    MemberId = reader.GetInt32(reader.GetOrdinal("MemberId")),
                    Amount = reader.GetDouble(reader.GetOrdinal("Amount")),
                    Date = reader.IsDBNull(reader.GetOrdinal("Date")) ? null : reader.GetString(reader.GetOrdinal("Date")),
                    Status = "Completed" // Default status
                });
            }

            return payments;
        }
    }
}
