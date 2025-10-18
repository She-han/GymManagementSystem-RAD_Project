using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;
using System.Data;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class AddMemberDialog : Window
    {
        public string LastError { get; set; }
        private Dictionary<string, string> trainerMap = new Dictionary<string, string>();

        public AddMemberDialog()
        {
            InitializeComponent();
            MemberIdText.Text = MemberService.GetNextMemberId();
            LoadTrainers();
        }

        private void LoadTrainers()
        {
            try
            {
                TrainerCombo.Items.Clear();
                trainerMap.Clear();

                // Add "No Trainer" option
                TrainerCombo.Items.Add("No Trainer Assigned");
                trainerMap["No Trainer Assigned"] = null;

                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new SqliteCommand("SELECT Id, FullName, TrainerId, Specialty FROM Trainers ORDER BY FullName", conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var fullName = reader.GetString("FullName");
                    var specialty = reader.IsDBNull("Specialty") ? "" : reader.GetString("Specialty");

                    string displayText = fullName;
                    if (!string.IsNullOrEmpty(specialty))
                    {
                        displayText += $" ({specialty})";
                    }

                    TrainerCombo.Items.Add(displayText);
                    trainerMap[displayText] = fullName;
                }

                TrainerCombo.SelectedIndex = 0; // Select "No Trainer Assigned" by default
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trainers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Add fallback option
                TrainerCombo.Items.Clear();
                TrainerCombo.Items.Add("No Trainer Assigned");
                trainerMap.Clear();
                trainerMap["No Trainer Assigned"] = null;
                TrainerCombo.SelectedIndex = 0;
            }
        }

        // Event handler for number-only input validation
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits, spaces, hyphens, plus signs, and parentheses for phone numbers
            Regex regex = new Regex(@"^[0-9\s\-\+\(\)]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameText.Text.Trim();
            string contact = ContactText.Text.Trim();
           
            string medicalHistory = MedicalHistoryText.Text.Trim();

            // Get selected trainer
            string trainerName = null;
            if (TrainerCombo.SelectedItem != null)
            {
                string selectedTrainerDisplay = TrainerCombo.SelectedItem.ToString();
                if (trainerMap.ContainsKey(selectedTrainerDisplay))
                {
                    trainerName = trainerMap[selectedTrainerDisplay];
                }
            }

            // Get subscription type
            string subType = "";
            if (SubTypeCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                subType = selectedItem.Content.ToString();
            }

            // Validation
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowError("Full Name is required.");
                FullNameText.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(contact))
            {
                ShowError("Contact Number is required.");
                ContactText.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(subType))
            {
                ShowError("Subscription Type is required.");
                SubTypeCombo.Focus();
                return;
            }

            // Validate contact number format
            if (!IsValidPhoneNumber(contact))
            {
                ShowError("Please enter a valid contact number (7-15 digits).");
                ContactText.Focus();
                return;
            }

            // Check if member ID already exists
            if (MemberIdExists(MemberIdText.Text))
            {
                ShowError("Member ID already exists. Please try again.");
                MemberIdText.Text = MemberService.GetNextMemberId();
                return;
            }

            var member = new Member
            {
                MemberId = MemberIdText.Text,
                FullName = fullName,
                ContactNumber = contact,
                TrainerName = trainerName,
                SubscriptionType = subType,
                JoinDate = DateTime.Now.ToString("yyyy-MM-dd"),
                MedicalHistory = string.IsNullOrWhiteSpace(medicalHistory) ? null : medicalHistory
            };

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                using var transaction = conn.BeginTransaction();
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"INSERT INTO Members (MemberId, FullName, TrainerName, JoinDate, SubscriptionType, ContactNumber, MedicalHistory) 
                                       VALUES (@id, @name, @trainer, @date, @type, @contact, @medical)";

                    cmd.Parameters.AddWithValue("@id", member.MemberId);
                    cmd.Parameters.AddWithValue("@name", member.FullName);
                    cmd.Parameters.AddWithValue("@trainer", member.TrainerName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@date", member.JoinDate);
                    cmd.Parameters.AddWithValue("@type", member.SubscriptionType);
                    cmd.Parameters.AddWithValue("@contact", member.ContactNumber);
                    cmd.Parameters.AddWithValue("@medical", member.MedicalHistory ?? (object)DBNull.Value);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        transaction.Commit();
                        ShowSuccess("Member added successfully!");
                        DialogResult = true;
                    }
                    else
                    {
                        transaction.Rollback();
                        LastError = "Failed to add member - no rows affected.";
                        ShowError("Failed to add member. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                LastError = $"Failed to add member: {ex.Message}";
                ShowError($"Database error: {ex.Message}");
            }
        }

        private bool MemberIdExists(string memberId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new SqliteCommand("SELECT COUNT(*) FROM Members WHERE MemberId = @id", conn);
                cmd.Parameters.AddWithValue("@id", memberId);

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch
            {
                return false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Enhanced phone number validation
            string cleanNumber = Regex.Replace(phoneNumber, @"[^\d]", ""); // Remove non-digits
            return cleanNumber.Length >= 7 && cleanNumber.Length <= 15;
        }
    }
}