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
    public partial class AddEquipmentDialog : Window
    {
        public string LastError { get; set; }
        private readonly IEntityFactory _entityFactory;

        public AddEquipmentDialog()
        {
            InitializeComponent();
            _entityFactory = new EntityFactory();
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
            string quantityText = QuantityText.Text.Trim();

            // Get category from ComboBox
            string category = "";
            if (CategoryCombo.SelectedItem is System.Windows.Controls.ComboBoxItem selectedCategory)
            {
                category = selectedCategory.Content.ToString();
            }

            // Get condition from ComboBox
            string condition = "";
            if (ConditionCombo.SelectedItem is System.Windows.Controls.ComboBoxItem selectedCondition)
            {
                condition = selectedCondition.Content.ToString();
            }

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

            try
            {
                // Check if equipment with same name already exists
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var checkCmd = conn.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM Equipment WHERE Name = @name";
                    checkCmd.Parameters.AddWithValue("@name", name);
                    long exists = (long)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        var result = MessageBox.Show(
                            $"Equipment '{name}' already exists. Do you want to add it anyway?",
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
                }

                // Use Factory Pattern to create Equipment
                var equipment = _entityFactory.CreateEquipment(
                    EquipmentIdText.Text,
                    name,
                    quantity,
                    string.IsNullOrWhiteSpace(condition) ? null : condition,
                    string.IsNullOrWhiteSpace(category) ? null : category
                );

                // Use Factory Pattern to insert Equipment
                if (_entityFactory.InsertEquipment(equipment, out string errorMessage))
                {
                    MessageBox.Show("Equipment added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    LastError = errorMessage;
                    ShowError($"Failed to add equipment: {errorMessage}");
                }
            }
            catch (ArgumentException ex)
            {
                LastError = $"Validation error: {ex.Message}";
                ShowError($"Validation error: {ex.Message}");
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