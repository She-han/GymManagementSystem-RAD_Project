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

namespace GymManagementSystem
{
    public partial class AdminLoginWindow : Window
    {
        public AdminLoginWindow()
        {
            InitializeComponent();
            DatabaseHelper.EnsureAdminsTableExists();
        }

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
                MessageBox.Show("Please fill all fields.");
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
                MessageBox.Show("Login successful!");
                // Replace with your dashboard window
                DashboardWindow dashboard = new DashboardWindow();
                dashboard.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid credentials.");
            }
           // MessageBox.Show(System.IO.Path.GetFullPath("gym.db"));
        }

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            string username = SignupUsernameTextBox.Text.Trim();
            string password = SignupPasswordBox.Password;
            string confirmPassword = SignupConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
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
                MessageBox.Show("Username already exists.");
                return;
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Admins (Username, PasswordHash) VALUES (@username, @hash)";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hashedPassword);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Signup successful! You can now login.");
            ShowLogin_Click(null, null);
        }
    }
}
