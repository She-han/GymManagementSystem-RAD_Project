using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class AddMemberDialog : Window
    {
        public string LastError { get; set; }

        public AddMemberDialog()
        {
            InitializeComponent();
            MemberIdText.Text = MemberService.GetNextMemberId();
        }

        // Event handler for number-only input validation
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits, spaces, hyphens, and plus signs for phone numbers
            Regex regex = new Regex(@"^[0-9\s\-\+]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameText.Text.Trim();
            string contact = ContactText.Text.Trim();
            string trainer = TrainerText.Text.Trim();
            string medicalHistory = MedicalHistoryText.Text.Trim();

            // Get the content of the selected ComboBoxItem
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

            // Validate contact number format (basic validation)
            if (!IsValidPhoneNumber(contact))
            {
                ShowError("Please enter a valid contact number.");
                ContactText.Focus();
                return;
            }

            var member = new Member
            {
                MemberId = MemberIdText.Text,
                FullName = fullName,
                ContactNumber = contact,
                TrainerName = string.IsNullOrWhiteSpace(trainer) ? null : trainer,
                SubscriptionType = subType,
                JoinDate = DateTime.Now.ToString("yyyy-MM-dd"),
                MedicalHistory = string.IsNullOrWhiteSpace(medicalHistory) ? null : medicalHistory
            };

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = conn.CreateCommand();
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
                    MessageBox.Show("Member added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    LastError = "Failed to add member - no rows affected.";
                    ShowError("Failed to add member. Please try again.");
                }
            }
            catch (Exception ex)
            {
                LastError = $"Failed to add member: {ex.Message}";
                ShowError($"Database error: {ex.Message}");
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

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Basic phone number validation - allows digits, spaces, hyphens, and plus signs
            // Must be at least 7 digits long
            string cleanNumber = Regex.Replace(phoneNumber, @"[^\d]", ""); // Remove non-digits
            return cleanNumber.Length >= 7 && cleanNumber.Length <= 15;
        }
    }
}