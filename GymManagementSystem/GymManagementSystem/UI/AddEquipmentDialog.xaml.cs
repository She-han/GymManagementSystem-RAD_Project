using System.Windows;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.UI
{
    public partial class AddEquipmentDialog : Window
    {
        public string LastError { get; set; }
        public AddEquipmentDialog()
        {
            InitializeComponent();
            EquipmentIdText.Text = EquipmentService.GetNextEquipmentId();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string name = NameText.Text.Trim();
            string condition = ConditionText.Text.Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(QuantityText.Text))
            {
                LastError = "Please fill all required fields.";
                DialogResult = false;
                return;
            }
            if (!int.TryParse(QuantityText.Text.Trim(), out int quantity) || quantity < 1)
            {
                LastError = "Quantity must be a positive integer.";
                DialogResult = false;
                return;
            }
            var equipment = new Equipment
            {
                EquipmentId = EquipmentIdText.Text,
                Name = name,
                Quantity = quantity,
                Condition = condition
            };
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Equipment (EquipmentId, Name, Quantity, Condition) VALUES (@id, @name, @qty, @cond)";
                cmd.Parameters.AddWithValue("@id", equipment.EquipmentId);
                cmd.Parameters.AddWithValue("@name", equipment.Name);
                cmd.Parameters.AddWithValue("@qty", equipment.Quantity);
                cmd.Parameters.AddWithValue("@cond", equipment.Condition ?? "");
                cmd.ExecuteNonQuery();
                DialogResult = true;
            }
            catch
            {
                LastError = "Failed to add equipment.";
                DialogResult = false;
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}