﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GymManagementSystem.DAL;
using GymManagementSystem.UI.Dialogs;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.UI
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow(string username)
        {
            InitializeComponent();
            CurrentUserText.Text = username;
            LoadDashboardData();

            // Ensure dashboard is shown initially
            ShowDashboard();
        }

        private void LoadDashboardData()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                // Get actual member count
                var memberCmd = new SqliteCommand("SELECT COUNT(*) FROM Members", conn);
                TotalMembersText.Text = memberCmd.ExecuteScalar().ToString();

                // Get active memberships (same as total members for now)
                ActiveMembershipsText.Text = TotalMembersText.Text;

                // Get trainer count
                var trainerCmd = new SqliteCommand("SELECT COUNT(*) FROM Trainers", conn);
                TotalTrainersText.Text = trainerCmd.ExecuteScalar().ToString();

                // Get total income (placeholder - you can implement based on payments)
                TotalIncomeText.Text = "$8,400";

                // Add sample recent activity
                RecentActivityList.ItemsSource = new List<string>
                {
                    "🟢 New member registered (1 hour ago)",
                    "🟢 Payment received: $50 from member",
                    "🟢 Trainer added to system",
                    "🔴 Payment reminder sent",
                    "🟢 Equipment maintenance completed",
                };
            }
            catch (Exception ex)
            {
                // Sample data if database error
                TotalMembersText.Text = "0";
                ActiveMembershipsText.Text = "0";
                TotalTrainersText.Text = "0";
                TotalIncomeText.Text = "$0";

                RecentActivityList.ItemsSource = new List<string>
                {
                    "⚠️ Error loading data from database"
                };
            }
        }

        // Helper method to hide all content sections
        private void HideAllContent()
        {
            DashboardContent.Visibility = Visibility.Collapsed;
            MemberManagementContent.Visibility = Visibility.Collapsed;
            TrainerManagementContent.Visibility = Visibility.Collapsed;
            // Add other content sections here as you create them
            // PaymentManagementContent.Visibility = Visibility.Collapsed;
            // EquipmentManagementContent.Visibility = Visibility.Collapsed;
            // SettingsContent.Visibility = Visibility.Collapsed;
        }

        private void ShowDashboard()
        {
            HideAllContent();
            DashboardContent.Visibility = Visibility.Visible;
        }

        private void ShowMemberManagement()
        {
            HideAllContent();
            MemberManagementContent.Visibility = Visibility.Visible;
        }

        private void ShowTrainerManagement()
        {
            HideAllContent();
            TrainerManagementContent.Visibility = Visibility.Visible;
        }

        // Sidebar click handlers
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
            LoadDashboardData(); // Refresh dashboard data
        }

        private void Members_Click(object sender, RoutedEventArgs e)
        {
            ShowMemberManagement();
        }

        private void Trainers_Click(object sender, RoutedEventArgs e)
        {
            ShowTrainerManagement();
        }

        private void Payments_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement when PaymentManagementContent is created
            // HideAllContent();
            // PaymentManagementContent.Visibility = Visibility.Visible;
            MessageBox.Show("Payments page coming soon!");
        }

        private void Equipments_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement when EquipmentManagementContent is created
            // HideAllContent();
            // EquipmentManagementContent.Visibility = Visibility.Visible;
            MessageBox.Show("Equipments page coming soon!");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement when SettingsContent is created
            // HideAllContent();
            // SettingsContent.Visibility = Visibility.Visible;
            MessageBox.Show("Settings page coming soon!");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var login = new AdminLoginWindow();
                login.Show();
                this.Close();
            }
        }

        // Dashboard quick action handlers
        private void MarkAttendance_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MarkAttendanceDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void AddMemberButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddMemberDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                LoadDashboardData(); // Refresh dashboard data after adding member
            }
        }

        private void AddTrainerButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddTrainerDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                LoadDashboardData(); // Refresh dashboard data after adding trainer
            }
        }

        private void AddEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEquipmentDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }
    }
}