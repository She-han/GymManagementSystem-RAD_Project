using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using GymManagementSystem.Services;

namespace GymManagementSystem.UI
{
    public partial class ReportManagementControl : UserControl
    {
        public ReportManagementControl()
        {
            InitializeComponent();
            // Set default end date to today
            EndDatePicker.SelectedDate = DateTime.Now;
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
        }

        private void MemberReport_Click(object sender, MouseButtonEventArgs e)
        {
            // Visual feedback - handled by button click
        }

        private void EquipmentReport_Click(object sender, MouseButtonEventArgs e)
        {
            // Visual feedback - handled by button click
        }

        private void TrainerReport_Click(object sender, MouseButtonEventArgs e)
        {
            // Visual feedback - handled by button click
        }

        private void PaymentReport_Click(object sender, MouseButtonEventArgs e)
        {
            // Visual feedback - handled by button click
        }

        private void CompleteReport_Click(object sender, MouseButtonEventArgs e)
        {
            // Visual feedback - handled by button click
        }

        private void GenerateMemberReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport("Member", PdfReportGenerator.GenerateMemberReport);
        }

        private void GenerateEquipmentReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport("Equipment", PdfReportGenerator.GenerateEquipmentReport);
        }

        private void GenerateTrainerReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport("Trainer", PdfReportGenerator.GenerateTrainerReport);
        }

        private void GeneratePaymentReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport("Payment", PdfReportGenerator.GeneratePaymentReport);
        }

        private void GenerateCompleteReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport("Complete", PdfReportGenerator.GenerateAllReports);
        }

        private void GenerateReport(string reportType, Func<DateTime?, DateTime?, string, string> generateFunction)
        {
            try
            {
                // Validate date range
                if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
                {
                    if (StartDatePicker.SelectedDate.Value > EndDatePicker.SelectedDate.Value)
                    {
                        MessageBox.Show("Start date cannot be later than end date!", "Invalid Date Range", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Open SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = $"Save {reportType} Report",
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"{reportType}Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    DefaultExt = "pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string savePath = saveFileDialog.FileName;

                    // Show processing message
                    Mouse.OverrideCursor = Cursors.Wait;

                    // Generate report
                    DateTime? startDate = StartDatePicker.SelectedDate;
                    DateTime? endDate = EndDatePicker.SelectedDate;

                    string result = generateFunction(startDate, endDate, savePath);

                    Mouse.OverrideCursor = null;

                    // Show success message with option to open
                    var messageResult = MessageBox.Show(
                        $"{reportType} report has been generated successfully!\n\nLocation: {result}\n\nWould you like to open the report now?",
                        "Report Generated",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (messageResult == MessageBoxResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = result,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Could not open the file: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
