using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Models;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class EditEquipmentDialog : Window
    {
        private Equipment currentEquipment;

        public EditEquipmentDialog(Equipment equipment)
        {
            InitializeComponent();
            currentEquipment = equipment;
            LoadEquipmentData();
        }

        private void LoadEquipmentData()
        {
            EquipmentIdText.Text = currentEquipment.EquipmentId;
            NameText.Text = currentEquipment.Name;
            QuantityText.Text = currentEquipment.Quantity.ToString();

            // Set category
            string category = currentEquipment.Category ?? "";
            for (int i = 0; i < CategoryCombo.Items.Count; i++)
            {
                if ((CategoryCombo.Items[i] as ComboBoxItem)?.Content.ToString() == category)
                {
                    CategoryCombo.SelectedIndex = i;
                    break;
                }
            }

            // Set condition
            string condition = currentEquipment.Condition ?? "";
            for (int i = 0; i < ConditionCombo.Items.Count; i++)
            {
                if ((ConditionCombo.Items[i] as ComboBoxItem)?.Content.ToString() == condition)
                {
                    ConditionCombo.SelectedIndex = i;
                    break;
                }
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            string name = NameText.Text.Trim();
            string quantityStr = QuantityText.Text.Trim();

            string category = "";
            if (CategoryCombo.SelectedItem is ComboBoxItem selectedCategory)
            {
                category = selectedCategory.Content.ToString();
            }

            string condition = "";
            if (ConditionCombo.SelectedItem is ComboBoxItem selectedCondition)
            {
                condition = selectedCondition.Content.ToString();
            }

            // Validation
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(quantityStr))
            {
                MessageBox.Show("Please fill all required fields (marked with *).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(quantityStr, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Please enter a valid quantity (non-negative number).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new SqliteCommand(@"
                    UPDATE Equipment SET 
                        Name = @name,
                        Category = @category,
                        Quantity = @quantity,
                        Condition = @condition
                    WHERE Id = @id", conn);

                cmd.Parameters.AddWithValue("@id", currentEquipment.Id);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@category", string.IsNullOrWhiteSpace(category) ? (object)DBNull.Value : category);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@condition", string.IsNullOrWhiteSpace(condition) ? (object)DBNull.Value : condition);

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("Equipment updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Failed to update equipment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating equipment: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
