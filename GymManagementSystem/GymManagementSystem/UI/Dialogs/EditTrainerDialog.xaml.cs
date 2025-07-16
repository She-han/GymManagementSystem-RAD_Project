using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Models;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class EditTrainerDialog : Window
    {
        private Trainer currentTrainer;

        public EditTrainerDialog(Trainer trainer)
        {
            InitializeComponent();
            currentTrainer = trainer;
            LoadTrainerData();
        }

        private void LoadTrainerData()
        {
            TrainerIdText.Text = currentTrainer.TrainerId;
            FullNameText.Text = currentTrainer.FullName;
            ContactText.Text = currentTrainer.ContactNumber ?? "";
            EmailText.Text = currentTrainer.Email ?? "";
            ExperienceText.Text = currentTrainer.Experience ?? "";
            

            // Set specialty
            if (!string.IsNullOrWhiteSpace(currentTrainer.Specialty))
            {
                foreach (ComboBoxItem item in SpecialtyCombo.Items)
                {
                    if (item.Content.ToString() == currentTrainer.Specialty)
                    {
                        SpecialtyCombo.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
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
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(contact))
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
                    var cmd = new SqliteCommand(@"
                        UPDATE Trainers SET 
                            FullName = @fullName,
                            ContactNumber = @contact,
                            Email = @email,
                            Specialty = @specialty,
                            Experience = @experience
                        WHERE Id = @id", conn);

                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@id", currentTrainer.Id);
                    cmd.Parameters.AddWithValue("@fullName", fullName);
                    cmd.Parameters.AddWithValue("@contact", contact);
                    cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@specialty", string.IsNullOrWhiteSpace(specialty) ? (object)DBNull.Value : specialty);
                    cmd.Parameters.AddWithValue("@experience", string.IsNullOrWhiteSpace(experience) ? (object)DBNull.Value : experience);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        transaction.Commit();
                        MessageBox.Show("Trainer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                    }
                    else
                    {
                        transaction.Rollback();
                        MessageBox.Show("Failed to update trainer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Error updating trainer: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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