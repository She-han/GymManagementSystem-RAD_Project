using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class AddTrainerDialog : Window
    {
        public string LastError { get; set; }

        public AddTrainerDialog()
        {
            InitializeComponent();
            TrainerIdText.Text = TrainerService.GetNextTrainerId();
        }

        // Event handler for number-only input validation for contact
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
            string specialty = SpecialtyText.Text.Trim();

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

            // Validate contact number format
            if (!IsValidPhoneNumber(contact))
            {
                ShowError("Please enter a valid contact number (7-15 digits).");
                ContactText.Focus();
                ContactText.SelectAll();
                return;
            }

            var trainer = new Trainer
            {
                TrainerId = TrainerIdText.Text,
                FullName = fullName,
                ContactNumber = contact,
                Specialty = string.IsNullOrWhiteSpace(specialty) ? null : specialty
            };

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                // Check if trainer with same name already exists
                var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Trainers WHERE FullName = @name";
                checkCmd.Parameters.AddWithValue("@name", trainer.FullName);
                long exists = (long)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    var result = MessageBox.Show(
                        $"A trainer named '{trainer.FullName}' already exists. Do you want to add anyway?",
                        "Duplicate Trainer",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        FullNameText.Focus();
                        FullNameText.SelectAll();
                        return;
                    }
                }

                // Check if contact number already exists
                checkCmd.CommandText = "SELECT COUNT(*) FROM Trainers WHERE ContactNumber = @contact";
                checkCmd.Parameters.Clear();
                checkCmd.Parameters.AddWithValue("@contact", trainer.ContactNumber);
                exists = (long)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    ShowError("This contact number is already registered to another trainer.");
                    ContactText.Focus();
                    ContactText.SelectAll();
                    return;
                }

                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Trainers (TrainerId, FullName, ContactNumber, Specialty) VALUES (@id, @name, @contact, @spec)";
                cmd.Parameters.AddWithValue("@id", trainer.TrainerId);
                cmd.Parameters.AddWithValue("@name", trainer.FullName);
                cmd.Parameters.AddWithValue("@contact", trainer.ContactNumber);
                cmd.Parameters.AddWithValue("@spec", trainer.Specialty ?? (object)DBNull.Value);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Trainer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    LastError = "Failed to add trainer - no rows affected.";
                    ShowError("Failed to add trainer. Please try again.");
                }
            }
            catch (SqliteException ex)
            {
                LastError = $"Database error: {ex.Message}";
                ShowError($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                LastError = $"Failed to add trainer: {ex.Message}";
                ShowError($"An unexpected error occurred: {ex.Message}");
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