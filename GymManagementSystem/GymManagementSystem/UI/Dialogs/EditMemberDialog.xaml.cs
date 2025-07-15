using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Models;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class EditMemberDialog : Window
    {
        private Member currentMember;

        public EditMemberDialog(Member member)
        {
            InitializeComponent();
            currentMember = member;
            LoadMemberData();
        }

        private void LoadMemberData()
        {
            MemberIdText.Text = currentMember.MemberId;
            FullNameText.Text = currentMember.FullName;
            ContactText.Text = currentMember.ContactNumber ?? "";
            TrainerText.Text = currentMember.TrainerName ?? "";
            MedicalHistoryText.Text = currentMember.MedicalHistory ?? "";

            // Set subscription type
            if (currentMember.SubscriptionType == "Daily Payment")
                SubTypeCombo.SelectedIndex = 0;
            else if (currentMember.SubscriptionType == "Monthly Payment")
                SubTypeCombo.SelectedIndex = 1;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameText.Text.Trim();
            string contact = ContactText.Text.Trim();
            string trainer = TrainerText.Text.Trim();
            string medicalHistory = MedicalHistoryText.Text.Trim();

            string subType = "";
            if (SubTypeCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                subType = selectedItem.Content.ToString();
            }

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(contact) || string.IsNullOrWhiteSpace(subType))
            {
                MessageBox.Show("Please fill all required fields (marked with *).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new SqliteCommand(@"
                    UPDATE Members SET 
                        FullName = @fullName,
                        ContactNumber = @contact,
                        TrainerName = @trainer,
                        SubscriptionType = @subType,
                        MedicalHistory = @medical
                    WHERE Id = @id", conn);

                cmd.Parameters.AddWithValue("@id", currentMember.Id);
                cmd.Parameters.AddWithValue("@fullName", fullName);
                cmd.Parameters.AddWithValue("@contact", contact);
                cmd.Parameters.AddWithValue("@trainer", string.IsNullOrWhiteSpace(trainer) ? (object)DBNull.Value : trainer);
                cmd.Parameters.AddWithValue("@subType", subType);
                cmd.Parameters.AddWithValue("@medical", string.IsNullOrWhiteSpace(medicalHistory) ? (object)DBNull.Value : medicalHistory);

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("Member updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Failed to update member.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating member: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}