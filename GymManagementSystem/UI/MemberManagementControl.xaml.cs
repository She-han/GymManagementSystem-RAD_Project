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
    public partial class MemberManagementControl : UserControl
    {
        private List<Member> allMembers = new List<Member>();
        private string searchPlaceholder = "Search members...";

        public MemberManagementControl()
        {
            InitializeComponent();
            LoadMembers();
            LoadSummaryData();
        }

        private void LoadMembers()
        {
            try
            {
                allMembers = GetAllMembersFromDatabase();
                DisplayMembers(allMembers);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Member> GetAllMembersFromDatabase()
        {
            var members = new List<Member>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            var cmd = new SqliteCommand("SELECT * FROM Members ORDER BY Id DESC", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                members.Add(new Member
                {
                    Id = reader.GetInt32("Id"),
                    MemberId = reader.GetString("MemberId"),
                    FullName = reader.GetString("FullName"),
                    TrainerName = reader.IsDBNull("TrainerName") ? null : reader.GetString("TrainerName"),
                    JoinDate = reader.IsDBNull("JoinDate") ? null : reader.GetString("JoinDate"),
                    SubscriptionType = reader.IsDBNull("SubscriptionType") ? null : reader.GetString("SubscriptionType"),
                    ContactNumber = reader.IsDBNull("ContactNumber") ? null : reader.GetString("ContactNumber"),
                    MedicalHistory = reader.IsDBNull("MedicalHistory") ? null : reader.GetString("MedicalHistory")
                });
            }

            return members;
        }

        private void DisplayMembers(List<Member> members)
        {
            MembersContainer.Children.Clear();

            foreach (var member in members)
            {
                var memberCard = CreateMemberCard(member);
                MembersContainer.Children.Add(memberCard);
            }
        }

        private Border CreateMemberCard(Member member)
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

            // Member Info
            var infoPanel = new StackPanel { Orientation = Orientation.Vertical };

            var nameText = new TextBlock
            {
                Text = member.FullName,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var idText = new TextBlock
            {
                Text = $"ID: {member.MemberId}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                Margin = new Thickness(0, 0, 0, 2)
            };

            var contactText = new TextBlock
            {
                Text = $"Contact: {member.ContactNumber ?? "N/A"}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                Margin = new Thickness(0, 0, 0, 2)
            };

    

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
            viewButton.Click += (s, e) => ViewMember(member);

            var editButton = new Button
            {
                Content = "✏️",
                Background = new SolidColorBrush(Color.FromRgb(241, 196, 15)),
                Foreground = Brushes.White,
                Style = (Style)FindResource("ActionButton"),
                ToolTip = "Edit Member"
            };
            editButton.Click += (s, e) => EditMember(member);

            var deleteButton = new Button
            {
                Content = "🗑️",
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                Style = (Style)FindResource("ActionButton"),
                ToolTip = "Delete Member"
            };
            deleteButton.Click += (s, e) => DeleteMember(member);

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

                // Active Members
                var activeCmd = new SqliteCommand("SELECT COUNT(*) FROM Members", conn);
                ActiveMembersText.Text = activeCmd.ExecuteScalar().ToString();

                // New Members This Month
                var newCmd = new SqliteCommand("SELECT COUNT(*) FROM Members WHERE JoinDate >= date('now', 'start of month')", conn);
                NewMembersText.Text = newCmd.ExecuteScalar().ToString();

                // Expiring Soon (placeholder - you can implement based on your business logic)
                ExpiringSoonText.Text = "5";

                // Total Revenue (placeholder - you can implement based on payments)
                TotalRevenueText.Text = "$12,450";
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
            var filteredMembers = allMembers.Where(m =>
                m.FullName.ToLower().Contains(searchText) ||
                m.MemberId.ToLower().Contains(searchText) ||
                (m.ContactNumber?.ToLower().Contains(searchText) == true)
            ).ToList();

            DisplayMembers(filteredMembers);
        }

        private void AddNewMember_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddMemberDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadMembers();
                LoadSummaryData();
            }
        }

        private void ViewMember(Member member)
        {
            var dialog = new ViewMemberDialog(member);
            dialog.ShowDialog();
        }

        private void EditMember(Member member)
        {
            var dialog = new EditMemberDialog(member);
            if (dialog.ShowDialog() == true)
            {
                LoadMembers();
                LoadSummaryData();
            }
        }

        private void DeleteMember(Member member)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete member '{member.FullName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var conn = DatabaseHelper.GetConnection();
                    conn.Open();

                    var cmd = new SqliteCommand("DELETE FROM Members WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", member.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Member deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadMembers();
                        LoadSummaryData();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete member.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting member: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}