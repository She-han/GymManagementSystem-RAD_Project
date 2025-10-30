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
        private readonly IEntityFactory _entityFactory;

        public AddTrainerDialog()
        {
            InitializeComponent();
            _entityFactory = new EntityFactory();
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
                // Use Factory Pattern to create Trainer
                var trainer = _entityFactory.CreateTrainer(
                    trainerId,
                    fullName,
                    contact,
                    string.IsNullOrWhiteSpace(specialty) ? null : specialty,
                    string.IsNullOrWhiteSpace(experience) ? null : experience,
                    string.IsNullOrWhiteSpace(email) ? null : email
                );

                // Use Factory Pattern to insert Trainer
                if (_entityFactory.InsertTrainer(trainer, out string errorMessage))
                {
                    MessageBox.Show("Trainer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show($"Failed to add trainer: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Validation error: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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