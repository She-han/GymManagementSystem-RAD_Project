using System;
using System.Collections.Generic;
using System.Globalization;
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
    public partial class ViewMemberDialog : Window
    {
        private Member currentMember;
        private List<Attendance> attendanceRecords = new List<Attendance>();

        public ViewMemberDialog(Member member)
        {
            InitializeComponent();
            currentMember = member;
            LoadMemberData();
            LoadAttendanceData();
        }

        private void LoadMemberData()
        {
            MemberIdText.Text = currentMember.MemberId;
            FullNameText.Text = currentMember.FullName;
            ContactText.Text = currentMember.ContactNumber ?? "N/A";
            TrainerText.Text = currentMember.TrainerName ?? "No trainer assigned";
            JoinDateText.Text = currentMember.JoinDate ?? "N/A";
            SubscriptionText.Text = currentMember.SubscriptionType ?? "N/A";
            MedicalHistoryText.Text = currentMember.MedicalHistory ?? "No medical history recorded";
        }

        private void LoadAttendanceData()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new SqliteCommand(@"
                    SELECT * FROM Attendance 
                    WHERE MemberId = @memberId 
                    ORDER BY Date DESC, TimeIn DESC", conn);
                cmd.Parameters.AddWithValue("@memberId", currentMember.Id);

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    attendanceRecords.Add(new Attendance
                    {
                        
                        MemberId = reader.GetInt32("MemberId"),
                        Date = reader.GetString("Date"),
                        TimeIn = reader.IsDBNull("TimeIn") ? null : reader.GetString("TimeIn"),
                        TimeOut = reader.IsDBNull("TimeOut") ? null : reader.GetString("TimeOut")
                    });
                }

                DisplayAttendanceRecords();
                UpdateAttendanceSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayAttendanceRecords()
        {
            AttendanceContainer.Children.Clear();

            if (attendanceRecords.Count == 0)
            {
                NoAttendanceMessage.Visibility = Visibility.Visible;
                return;
            }

            NoAttendanceMessage.Visibility = Visibility.Collapsed;

            foreach (var attendance in attendanceRecords)
            {
                var attendanceCard = CreateAttendanceCard(attendance);
                AttendanceContainer.Children.Add(attendanceCard);
            }
        }

        private Border CreateAttendanceCard(Attendance attendance)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Date
            var dateText = new TextBlock
            {
                Text = FormatDate(attendance.Date),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(dateText, 0);

            // Time In
            var timeInPanel = new StackPanel { Orientation = Orientation.Horizontal };
            timeInPanel.Children.Add(new TextBlock
            {
                Text = "In: ",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                Margin = new Thickness(0, 0, 5, 0)
            });
            timeInPanel.Children.Add(new TextBlock
            {
                Text = attendance.TimeIn ?? "N/A",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(40, 167, 69))
            });
            Grid.SetRow(timeInPanel, 1);

            // Time Out
            var timeOutPanel = new StackPanel { Orientation = Orientation.Horizontal };
            timeOutPanel.Children.Add(new TextBlock
            {
                Text = "Out: ",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                Margin = new Thickness(0, 0, 5, 0)
            });
            timeOutPanel.Children.Add(new TextBlock
            {
                Text = attendance.TimeOut ?? "N/A",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69))
            });
            Grid.SetRow(timeOutPanel, 2);

            grid.Children.Add(dateText);
            grid.Children.Add(timeInPanel);
            grid.Children.Add(timeOutPanel);

            card.Child = grid;
            return card;
        }

        private void UpdateAttendanceSummary()
        {
            // Check if the controls exist before setting their text
            if (TotalDaysText != null)
            {
                TotalDaysText.Text = attendanceRecords.Count.ToString();
            }

            if (ThisMonthText != null)
            {
                // Calculate this month's attendance
                var thisMonth = DateTime.Now.ToString("yyyy-MM");
                var thisMonthCount = attendanceRecords.Count(a => a.Date.StartsWith(thisMonth));
                ThisMonthText.Text = thisMonthCount.ToString();
            }
        }

        private string FormatDate(string dateString)
        {
            try
            {
                if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    return date.ToString("MMM dd, yyyy");
                }
                return dateString;
            }
            catch
            {
                return dateString;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}