using System;
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

namespace GymManagementSystem.UI
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
            // TODO: Replace with actual username from login
            CurrentUserText.Text = "Admin";
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            // TODO: Replace with real database queries.

            // Sample/fake data for UI demonstration:
            TotalMembersText.Text = "108";
            ActiveMembershipsText.Text = "96";
            TotalIncomeText.Text = "$8,400";
            TotalTrainersText.Text = "7";

            // Add sample recent activity
            RecentActivityList.ItemsSource = new List<string>
            {
                "🟢 John Doe renewed membership (1 hour ago)",
                "🟢 Payment received: $50 from Jane Smith",
                "🟢 New member registered: Alex Lee",
                "🔴 Payment overdue: Mark Brown",
                "🟢 Trainer Sarah added HIIT session (yesterday)",
            };
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            // Already on dashboard
        }

        private void Members_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open Members management window/page
            MessageBox.Show("Members page coming soon!");
        }

        private void Trainers_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open Trainers management window/page
            MessageBox.Show("Trainers page coming soon!");
        }

        private void Payments_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open Payments management window/page
            MessageBox.Show("Payments page coming soon!");
        }

        private void Equipments_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Equipments page coming soon!");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open Settings window/page
            MessageBox.Show("Settings page coming soon!");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Logout logic
            var login = new AdminLoginWindow();
            login.Show();
            this.Close();
        }

        private void AddMemberButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddMemberDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void AddTrainerButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddTrainerDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void AddEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEquipmentDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

    }
}
