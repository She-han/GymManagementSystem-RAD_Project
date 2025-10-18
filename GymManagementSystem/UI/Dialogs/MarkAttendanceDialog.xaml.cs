using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GymManagementSystem.Models;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;
using System.Data;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class MarkAttendanceDialog : Window
    {
        private List<Member> allMembers = new List<Member>();
        private Member selectedMember = null;
        private string searchPlaceholder = "Search by name or member ID...";
        private System.Windows.Threading.DispatcherTimer timer;

        public MarkAttendanceDialog()
        {
            InitializeComponent();
            LoadMembers();
            SetupTimeComboBoxes();
            SetupCurrentTimeDisplay();
        }

        private void LoadMembers()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new SqliteCommand("SELECT * FROM Members ORDER BY FullName", conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    allMembers.Add(new Member
                    {
                        Id = reader.GetInt32("Id"),
                        MemberId = reader.GetString("MemberId"),
                        FullName = reader.GetString("FullName"),
                        ContactNumber = reader.IsDBNull("ContactNumber") ? null : reader.GetString("ContactNumber")
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupTimeComboBoxes()
        {
            // Setup Hour ComboBox (0-23)
            for (int i = 0; i < 24; i++)
            {
                HourComboBox.Items.Add(i.ToString("D2"));
            }

            // Setup Minute ComboBox (0-59, every 5 minutes)
            for (int i = 0; i < 60; i += 5)
            {
                MinuteComboBox.Items.Add(i.ToString("D2"));
            }

            // Set default time to current time + 2 hours
            var defaultOutTime = DateTime.Now.AddHours(2);
            HourComboBox.SelectedItem = defaultOutTime.Hour.ToString("D2");
            MinuteComboBox.SelectedItem = (defaultOutTime.Minute / 5 * 5).ToString("D2");
        }

        private void SetupCurrentTimeDisplay()
        {
            // Update current time display
            UpdateCurrentTimeDisplay();

            // Setup timer to update time every second
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => UpdateCurrentTimeDisplay();
            timer.Start();
        }

        private void UpdateCurrentTimeDisplay()
        {
            CurrentDateTimeText.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
                m.MemberId.ToLower().Contains(searchText)
            ).ToList();

            DisplayMembers(filteredMembers);
        }

        private void DisplayMembers(List<Member> members)
        {
            MembersListBox.Items.Clear();

            foreach (var member in members)
            {
                var item = new ListBoxItem
                {
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = member.FullName,
                                FontWeight = FontWeights.Bold,
                                FontSize = 14,
                                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51))
                            },
                            new TextBlock
                            {
                                Text = $"ID: {member.MemberId}",
                                FontSize = 12,
                                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102))
                            }
                        }
                    },
                    Tag = member
                };

                MembersListBox.Items.Add(item);
            }
        }

        private void MembersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MembersListBox.SelectedItem is ListBoxItem selectedItem)
            {
                selectedMember = selectedItem.Tag as Member;

                if (selectedMember != null)
                {
                    SelectedMemberName.Text = selectedMember.FullName;
                    SelectedMemberId.Text = $"ID: {selectedMember.MemberId}";

                    SelectedMemberPanel.Visibility = Visibility.Visible;
                    TimeSelectionPanel.Visibility = Visibility.Visible;
                    MarkButton.IsEnabled = true;
                }
            }
        }

        private void MarkAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMember == null)
            {
                MessageBox.Show("Please select a member first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (HourComboBox.SelectedItem == null || MinuteComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select expected out time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                string currentTime = DateTime.Now.ToString("HH:mm:ss");

                // Create expected out time
                string expectedOutTime = $"{HourComboBox.SelectedItem}:{MinuteComboBox.SelectedItem}:00";

                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                // Check if member already has attendance for today
                var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Attendance WHERE MemberId = @memberId AND Date = @date", conn);
                checkCmd.Parameters.AddWithValue("@memberId", selectedMember.Id);
                checkCmd.Parameters.AddWithValue("@date", currentDate);

                long existingCount = (long)checkCmd.ExecuteScalar();

                if (existingCount > 0)
                {
                    var result = MessageBox.Show(
                        $"Member {selectedMember.FullName} already has attendance marked for today. Do you want to update it?",
                        "Attendance Already Exists",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }

                    // Update existing attendance
                    var updateCmd = new SqliteCommand(@"
                        UPDATE Attendance SET 
                            TimeIn = @timeIn,
                            TimeOut = @timeOut 
                        WHERE MemberId = @memberId AND Date = @date", conn);

                    updateCmd.Parameters.AddWithValue("@memberId", selectedMember.Id);
                    updateCmd.Parameters.AddWithValue("@date", currentDate);
                    updateCmd.Parameters.AddWithValue("@timeIn", currentTime);
                    updateCmd.Parameters.AddWithValue("@timeOut", expectedOutTime);

                    updateCmd.ExecuteNonQuery();

                    MessageBox.Show("Attendance updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Insert new attendance
                    var insertCmd = new SqliteCommand(@"
                        INSERT INTO Attendance (MemberId, Date, TimeIn, TimeOut) 
                        VALUES (@memberId, @date, @timeIn, @timeOut)", conn);

                    insertCmd.Parameters.AddWithValue("@memberId", selectedMember.Id);
                    insertCmd.Parameters.AddWithValue("@date", currentDate);
                    insertCmd.Parameters.AddWithValue("@timeIn", currentTime);
                    insertCmd.Parameters.AddWithValue("@timeOut", expectedOutTime);

                    insertCmd.ExecuteNonQuery();

                    MessageBox.Show("Attendance marked successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking attendance: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            timer?.Stop();
            base.OnClosed(e);
        }
    }
}