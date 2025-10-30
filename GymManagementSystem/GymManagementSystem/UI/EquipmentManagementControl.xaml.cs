using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;
using GymManagementSystem.UI.Dialogs;
using System.Data;
using System.Windows.Media.Effects;

namespace GymManagementSystem.UI
{
    public partial class EquipmentManagementControl : UserControl
    {
        private List<Equipment> allEquipment = new List<Equipment>();
        private string searchPlaceholder = "Search equipment...";

        public EquipmentManagementControl()
        {
            InitializeComponent();
            LoadEquipment();
            LoadSummaryData();
        }

        private void LoadEquipment()
        {
            try
            {
                allEquipment = GetAllEquipmentFromDatabase();
                DisplayEquipment(allEquipment);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading equipment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Equipment> GetAllEquipmentFromDatabase()
        {
            var equipment = new List<Equipment>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            var cmd = new SqliteCommand("SELECT * FROM Equipment ORDER BY Id DESC", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                equipment.Add(new Equipment
                {
                    Id = reader.GetInt32("Id"),
                    EquipmentId = reader.GetString("EquipmentId"),
                    Name = reader.GetString("Name"),
                    Quantity = reader.GetInt32("Quantity"),
                    Condition = reader.IsDBNull("Condition") ? null : reader.GetString("Condition"),
                    Category = reader.IsDBNull("Category") ? null : reader.GetString("Category")
                });
            }

            return equipment;
        }

        private void DisplayEquipment(List<Equipment> equipment)
        {
            EquipmentContainer.Children.Clear();

            foreach (var item in equipment)
            {
                var equipmentCard = CreateEquipmentCard(item);
                EquipmentContainer.Children.Add(equipmentCard);
            }
        }

        private Border CreateEquipmentCard(Equipment equipment)
        {
            var card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 10),
                Effect = (DropShadowEffect)FindResource("CardShadow")
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Equipment Info
            var infoPanel = new StackPanel { Orientation = Orientation.Vertical };

            var nameText = new TextBlock
            {
                Text = equipment.Name,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var idText = new TextBlock
            {
                Text = $"ID: {equipment.EquipmentId}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                Margin = new Thickness(0, 0, 0, 2)
            };

            var categoryText = new TextBlock
            {
                Text = $"Category: {equipment.Category ?? "N/A"}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                Margin = new Thickness(0, 0, 0, 2)
            };

            var quantityText = new TextBlock
            {
                Text = $"Quantity: {equipment.Quantity}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                Margin = new Thickness(0, 0, 0, 2)
            };

            var conditionText = new TextBlock
            {
                Text = $"Condition: {equipment.Condition ?? "N/A"}",
                FontSize = 14,
                Foreground = GetConditionColor(equipment.Condition),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 2)
            };

            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(idText);
            infoPanel.Children.Add(categoryText);
            infoPanel.Children.Add(quantityText);
            infoPanel.Children.Add(conditionText);

            // Action Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var viewButton = new Button
            {
                Content = "👁️",
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Foreground = Brushes.White,
                Style = (Style)FindResource("ActionButton"),
                ToolTip = "View Details"
            };
            viewButton.Click += (s, e) => ViewEquipment(equipment);

            var editButton = new Button
            {
                Content = "✏️",
                Background = new SolidColorBrush(Color.FromRgb(241, 196, 15)),
                Foreground = Brushes.White,
                Style = (Style)FindResource("ActionButton"),
                ToolTip = "Edit Equipment"
            };
            editButton.Click += (s, e) => EditEquipment(equipment);

            var deleteButton = new Button
            {
                Content = "🗑️",
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                Style = (Style)FindResource("ActionButton"),
                ToolTip = "Delete Equipment"
            };
            deleteButton.Click += (s, e) => DeleteEquipment(equipment);

            buttonPanel.Children.Add(viewButton);
            buttonPanel.Children.Add(editButton);
            buttonPanel.Children.Add(deleteButton);

            grid.Children.Add(infoPanel);
            grid.Children.Add(buttonPanel);
            Grid.SetColumn(buttonPanel, 1);

            card.Child = grid;
            return card;
        }

        private SolidColorBrush GetConditionColor(string? condition)
        {
            if (string.IsNullOrEmpty(condition))
                return new SolidColorBrush(Color.FromRgb(102, 102, 102));

            string conditionLower = condition.ToLower();
            if (conditionLower.Contains("good") || conditionLower.Contains("excellent"))
                return new SolidColorBrush(Color.FromRgb(40, 167, 69)); // Green
            else if (conditionLower.Contains("fair"))
                return new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Yellow/Orange
            else if (conditionLower.Contains("poor") || conditionLower.Contains("needs"))
                return new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Red
            else
                return new SolidColorBrush(Color.FromRgb(102, 102, 102));
        }

        private void LoadSummaryData()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                // Total Equipment Types
                var totalCmd = new SqliteCommand("SELECT COUNT(*) FROM Equipment", conn);
                TotalEquipmentText.Text = totalCmd.ExecuteScalar().ToString();

                // Total Items (sum of all quantities)
                var itemsCmd = new SqliteCommand("SELECT COALESCE(SUM(Quantity), 0) FROM Equipment", conn);
                TotalItemsText.Text = itemsCmd.ExecuteScalar().ToString();

                // Maintenance Needed
                var maintenanceCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Equipment WHERE Condition LIKE '%Needs%' OR Condition LIKE '%Poor%'", conn);
                MaintenanceNeededText.Text = maintenanceCmd.ExecuteScalar().ToString();

                // Good Condition
                var goodCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Equipment WHERE Condition LIKE '%Good%' OR Condition LIKE '%Excellent%'", conn);
                GoodConditionText.Text = goodCmd.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading summary data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == searchPlaceholder)
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = searchPlaceholder;
                SearchTextBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text == searchPlaceholder) return;

            var searchText = SearchTextBox.Text.ToLower();
            var filteredEquipment = allEquipment.Where(eq =>
                eq.Name.ToLower().Contains(searchText) ||
                eq.EquipmentId.ToLower().Contains(searchText) ||
                (eq.Category?.ToLower().Contains(searchText) == true) ||
                (eq.Condition?.ToLower().Contains(searchText) == true)
            ).ToList();

            DisplayEquipment(filteredEquipment);
        }

        private void AddNewEquipment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEquipmentDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadEquipment();
                LoadSummaryData();
            }
        }

        private void ViewEquipment(Equipment equipment)
        {
            var dialog = new ViewEquipmentDialog(equipment);
            dialog.ShowDialog();
        }

        private void EditEquipment(Equipment equipment)
        {
            var dialog = new EditEquipmentDialog(equipment);
            if (dialog.ShowDialog() == true)
            {
                LoadEquipment();
                LoadSummaryData();
            }
        }

        private void DeleteEquipment(Equipment equipment)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete equipment '{equipment.Name}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var conn = DatabaseHelper.GetConnection();
                    conn.Open();

                    var cmd = new SqliteCommand("DELETE FROM Equipment WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", equipment.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Equipment deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadEquipment();
                        LoadSummaryData();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete equipment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting equipment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
