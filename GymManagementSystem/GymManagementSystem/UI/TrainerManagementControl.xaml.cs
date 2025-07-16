using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GymManagementSystem.Models;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;
using GymManagementSystem.UI.Dialogs;
using System.Data;
using System.Windows.Media.Effects;

namespace GymManagementSystem.UI
{
    public partial class TrainerManagementControl : UserControl
    {
        private List<Trainer> allTrainers = new List<Trainer>();
        private string searchPlaceholder = "Search trainers...";

        public TrainerManagementControl()
        {
            InitializeComponent();
            LoadTrainers();
            LoadSummaryData();
        }

        private void LoadTrainers()
        {
            try
            {
                allTrainers = GetAllTrainersFromDatabase();
                DisplayTrainers(allTrainers);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trainers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Trainer> GetAllTrainersFromDatabase()
        {
            var trainers = new List<Trainer>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            var cmd = new SqliteCommand("SELECT * FROM Trainers ORDER BY Id DESC", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                trainers.Add(new Trainer
                {
                    Id = reader.GetInt32("Id"),
                    TrainerId = reader.GetString("TrainerId"),
                    FullName = reader.GetString("FullName"),
                    ContactNumber = reader.IsDBNull("ContactNumber") ? null : reader.GetString("ContactNumber"),
                    Specialty = reader.IsDBNull("Specialty") ? null : reader.GetString("Specialty"),
                    Experience = reader.IsDBNull("Experience") ? null : reader.GetString("Experience"),
                    Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                    JoinDate = reader.IsDBNull("JoinDate") ? null : reader.GetString("JoinDate")
                });
            }

            return trainers;
        }

        private void DisplayTrainers(List<Trainer> trainers)
        {
            TrainersContainer.Children.Clear();

            foreach (var trainer in trainers)
            {
                var trainerCard = CreateTrainerCard(trainer);
                TrainersContainer.Children.Add(trainerCard);
            }
        }

        private Border CreateTrainerCard(Trainer trainer)
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

            // Trainer Info
            var infoPanel = new StackPanel { Orientation = Orientation.Vertical };

            // Trainer Name
            var nameText = new TextBlock
            {
                Text = trainer.FullName,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            // Trainer ID
            var idText = new TextBlock
            {
                Text = $"ID: {trainer.TrainerId}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                Margin = new Thickness(0, 0, 0, 2)
            };

            // Contact Number
            var contactText = new TextBlock
            {
                Text = $"Contact: {trainer.ContactNumber ?? "N/A"}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102))
            };

            // Add elements to the panel
            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(idText);
            infoPanel.Children.Add(contactText);

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
            viewButton.Click += (s, e) => ViewTrainer(trainer);

            var editButton = new Button
            {
                Content = "✏️",
                Background = new SolidColorBrush(Color.FromRgb(241, 196, 15)),
                Foreground = Brushes.White,
                Style = (Style)FindResource("ActionButton"),
                ToolTip = "Edit Trainer"
            };
            editButton.Click += (s, e) => EditTrainer(trainer);

            var deleteButton = new Button
            {
                Content = "🗑️",
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                Style = (Style)FindResource("ActionButton"),
                ToolTip = "Delete Trainer"
            };
            deleteButton.Click += (s, e) => DeleteTrainer(trainer);

            buttonPanel.Children.Add(viewButton);
            buttonPanel.Children.Add(editButton);
            buttonPanel.Children.Add(deleteButton);

            grid.Children.Add(infoPanel);
            grid.Children.Add(buttonPanel);
            Grid.SetColumn(buttonPanel, 1);

            card.Child = grid;
            return card;
        }

        private void LoadSummaryData()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                // Total Trainers
                var totalCmd = new SqliteCommand("SELECT COUNT(*) FROM Trainers", conn);
                TotalTrainersText.Text = totalCmd.ExecuteScalar().ToString();

                // Active Trainers (same as total for now)
                ActiveTrainersText.Text = TotalTrainersText.Text;

                // Unique Specializations
                var specializationsCmd = new SqliteCommand("SELECT COUNT(DISTINCT Specialty) FROM Trainers WHERE Specialty IS NOT NULL", conn);
                SpecializationsText.Text = specializationsCmd.ExecuteScalar().ToString();

                // New This Month (placeholder - you can implement based on join date if you add it)
                NewThisMonthText.Text = "0";
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
            var filteredTrainers = allTrainers.Where(t =>
                t.FullName.ToLower().Contains(searchText) ||
                t.TrainerId.ToLower().Contains(searchText) ||
                (t.ContactNumber?.ToLower().Contains(searchText) == true) ||
                (t.Specialty?.ToLower().Contains(searchText) == true)
            ).ToList();

            DisplayTrainers(filteredTrainers);
        }

        private void AddNewTrainer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddTrainerDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadTrainers();
                LoadSummaryData();
            }
        }

        private void ViewTrainer(Trainer trainer)
        {
            var dialog = new ViewTrainerDialog(trainer);
            dialog.ShowDialog();
        }

        private void EditTrainer(Trainer trainer)
        {
            var dialog = new EditTrainerDialog(trainer);
            if (dialog.ShowDialog() == true)
            {
                LoadTrainers();
                LoadSummaryData();
            }
        }

        private void DeleteTrainer(Trainer trainer)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete trainer '{trainer.FullName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var conn = DatabaseHelper.GetConnection();
                    conn.Open();

                    var cmd = new SqliteCommand("DELETE FROM Trainers WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", trainer.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Trainer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTrainers();
                        LoadSummaryData();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete trainer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting trainer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}