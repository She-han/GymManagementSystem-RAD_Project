using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Models;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;
using GymManagementSystem.Services;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class AddTrainerDialog : Window
    {
        public AddTrainerDialog()
        {
            InitializeComponent();
            TrainerIdText.Text = TrainerService.GetNextTrainerId();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string trainerId = TrainerIdText.Text.Trim();
            string fullName = FullNameText.Text.Trim();
            string contact = ContactText.Text.Trim();
            string email = EmailText.Text.Trim();
            string experience = ExperienceText.Text.Trim();

            string specialty = "";
            if (SpecialtyCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                specialty = selectedItem.Content.ToString();
            }

            // Validation
            if (string.IsNullOrWhiteSpace(trainerId) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(contact))
            {
                MessageBox.Show("Please fill all required fields (marked with *).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Basic email validation if provided
            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailText.Focus();
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                using var transaction = conn.BeginTransaction();
                try
                {
                    // Check if trainer ID already exists
                    var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Trainers WHERE TrainerId = @trainerId", conn);
                    checkCmd.Transaction = transaction;
                    checkCmd.Parameters.AddWithValue("@trainerId", trainerId);

                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Trainer ID already exists. Please use a different ID.", "Duplicate ID", MessageBoxButton.OK, MessageBoxImage.Warning);
                        transaction.Rollback();
                        return;
                    }

                    // Insert trainer with current date as JoinDate
                    var cmd = new SqliteCommand(@"
                        INSERT INTO Trainers (TrainerId, FullName, ContactNumber, Specialty, Experience, Email, JoinDate) 
                        VALUES (@trainerId, @fullName, @contact, @specialty, @experience, @email, @joinDate)", conn);

                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@trainerId", trainerId);
                    cmd.Parameters.AddWithValue("@fullName", fullName);
                    cmd.Parameters.AddWithValue("@contact", contact);
                    cmd.Parameters.AddWithValue("@specialty", string.IsNullOrWhiteSpace(specialty) ? (object)DBNull.Value : specialty);
                    cmd.Parameters.AddWithValue("@experience", string.IsNullOrWhiteSpace(experience) ? (object)DBNull.Value : experience);
                    cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@joinDate", DateTime.Now.ToString("yyyy-MM-dd")); // Auto-set current date

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        transaction.Commit();
                        MessageBox.Show("Trainer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                    }
                    else
                    {
                        transaction.Rollback();
                        MessageBox.Show("Failed to add trainer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Error adding trainer: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}