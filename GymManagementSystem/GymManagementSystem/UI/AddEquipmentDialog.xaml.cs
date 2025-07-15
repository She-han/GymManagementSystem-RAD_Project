using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
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

        // Event handler for number-only input validation for quantity
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits for quantity
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string name = NameText.Text.Trim();
            string condition = ConditionText.Text.Trim();
            string quantityText = QuantityText.Text.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Equipment Name is required.");
                NameText.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(quantityText))
            {
                ShowError("Quantity is required.");
                QuantityText.Focus();
                return;
            }

            if (!int.TryParse(quantityText, out int quantity) || quantity < 1)
            {
                ShowError("Quantity must be a positive integer (1 or greater).");
                QuantityText.Focus();
                QuantityText.SelectAll();
                return;
            }

            if (quantity > 10000)
            {
                ShowError("Quantity cannot exceed 10,000 units.");
                QuantityText.Focus();
                QuantityText.SelectAll();
                return;
            }

            var equipment = new Equipment
            {
                EquipmentId = EquipmentIdText.Text,
                Name = name,
                Quantity = quantity,
                Condition = string.IsNullOrWhiteSpace(condition) ? null : condition
            };

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                // Check if equipment with same name already exists
                var checkCmd = conn.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Equipment WHERE Name = @name";
                checkCmd.Parameters.AddWithValue("@name", equipment.Name);
                long exists = (long)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    var result = MessageBox.Show(
                        $"Equipment '{equipment.Name}' already exists. Do you want to add it anyway?",
                        "Duplicate Equipment",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        NameText.Focus();
                        NameText.SelectAll();
                        return;
                    }
                }

                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Equipment (EquipmentId, Name, Quantity, Condition) VALUES (@id, @name, @qty, @cond)";
                cmd.Parameters.AddWithValue("@id", equipment.EquipmentId);
                cmd.Parameters.AddWithValue("@name", equipment.Name);
                cmd.Parameters.AddWithValue("@qty", equipment.Quantity);
                cmd.Parameters.AddWithValue("@cond", equipment.Condition ?? (object)DBNull.Value);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Equipment added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    LastError = "Failed to add equipment - no rows affected.";
                    ShowError("Failed to add equipment. Please try again.");
                }
            }
            catch (SqliteException ex)
            {
                LastError = $"Database error: {ex.Message}";
                ShowError($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                LastError = $"Failed to add equipment: {ex.Message}";
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
    }
}