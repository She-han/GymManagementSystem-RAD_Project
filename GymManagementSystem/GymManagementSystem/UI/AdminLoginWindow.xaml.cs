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
using GymManagementSystem.DAL;
using GymManagementSystem.Utils;
using Microsoft.Data.Sqlite;
using GymManagementSystem.UI;
using System.Windows.Media.Animation;

namespace GymManagementSystem
{
    public partial class AdminLoginWindow : Window
    {
        public AdminLoginWindow()
        {
            InitializeComponent();
            DatabaseHelper.EnsureAdminsTableExists();
        }

        // Show modern notification (call this instead of MessageBox.Show)
        public async void ShowNotification(string message, string type = "info")
        {
            // Set icon and color based on type
            switch (type)
            {
                case "success":
                    NotificationIcon.Text = "✔️";
                    NotificationPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2ec4b6"));
                    break;
                case "error":
                    NotificationIcon.Text = "⚠️";
                    NotificationPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFe74c3c"));
                    break;
                default:
                    NotificationIcon.Text = "ℹ️";
                    NotificationPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF232526"));
                    break;
            }

            NotificationText.Text = message;
            NotificationPanel.Visibility = Visibility.Visible;

            // Fade in
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            NotificationPanel.BeginAnimation(OpacityProperty, fadeIn);

            // Wait 2.2 seconds
            await Task.Delay(2200);

            // Fade out
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, e) => { NotificationPanel.Visibility = Visibility.Collapsed; };
            NotificationPanel.BeginAnimation(OpacityProperty, fadeOut);
        }

        // The rest of your code (event handlers)...
        private void ShowSignup_Click(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            SignupGrid.Visibility = Visibility.Visible;
        }

        private void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            SignupGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowNotification("Please fill all fields.", "error");
                return;
            }

            string hashedPassword = PasswordHelper.HashPassword(password);

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Admins WHERE Username=@username AND PasswordHash=@hash";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hashedPassword);

            long count = (long)cmd.ExecuteScalar();
            if (count == 1)
            {
                ShowNotification("Login successful!", "success");
                // Delay navigation for user to see message
                Task.Delay(800).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        var dashboard = new UI.DashboardWindow();
                        dashboard.Show();
                        this.Close();
                    });
                });
            }
            else
            {
                ShowNotification("Invalid credentials.", "error");
            }
        }

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            string username = SignupUsernameTextBox.Text.Trim();
            string password = SignupPasswordBox.Password;
            string confirmPassword = SignupConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowNotification("Please fill all fields.", "error");
                return;
            }
            if (password != confirmPassword)
            {
                ShowNotification("Passwords do not match.", "error");
                return;
            }

            string hashedPassword = PasswordHelper.HashPassword(password);

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Admins WHERE Username=@username";
            checkCmd.Parameters.AddWithValue("@username", username);
            long exists = (long)checkCmd.ExecuteScalar();
            if (exists > 0)
            {
                ShowNotification("Username already exists.", "error");
                return;
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Admins (Username, PasswordHash) VALUES (@username, @hash)";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hashedPassword);
            int rows = cmd.ExecuteNonQuery();

            if (rows > 0)
            {
                ShowNotification("Signup successful! You can now login.", "success");
                Task.Delay(1200).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ShowLogin_Click(null, null);
                    });
                });
            }
            else
            {
                ShowNotification("Signup failed. Please try again.", "error");
            }
        }
    }
}
