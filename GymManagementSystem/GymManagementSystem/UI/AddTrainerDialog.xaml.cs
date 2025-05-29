using System.Windows;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.UI
{
    public partial class AddTrainerDialog : Window
    {
        public string LastError { get; set; }
        public AddTrainerDialog()
        {
            InitializeComponent();
            TrainerIdText.Text = TrainerService.GetNextTrainerId();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameText.Text.Trim();
            string contact = ContactText.Text.Trim();
            string specialty = SpecialtyText.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(contact))
            {
                LastError = "Please fill all required fields.";
                DialogResult = false;
                return;
            }
            var trainer = new Trainer
            {
                TrainerId = TrainerIdText.Text,
                FullName = fullName,
                ContactNumber = contact,
                Specialty = specialty
            };
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Trainers (TrainerId, FullName, ContactNumber, Specialty) VALUES (@id, @name, @contact, @spec)";
                cmd.Parameters.AddWithValue("@id", trainer.TrainerId);
                cmd.Parameters.AddWithValue("@name", trainer.FullName);
                cmd.Parameters.AddWithValue("@contact", trainer.ContactNumber ?? "");
                cmd.Parameters.AddWithValue("@spec", trainer.Specialty ?? "");
                cmd.ExecuteNonQuery();
                DialogResult = true;
            }
            catch
            {
                LastError = "Failed to add trainer.";
                DialogResult = false;
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}