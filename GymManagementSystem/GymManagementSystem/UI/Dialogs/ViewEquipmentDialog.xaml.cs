using System;
using System.Windows;
using System.Windows.Media;
using GymManagementSystem.Models;
using GymManagementSystem.DAL;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class ViewEquipmentDialog : Window
    {
        private Equipment currentEquipment;

        public ViewEquipmentDialog(Equipment equipment)
        {
            InitializeComponent();
            currentEquipment = equipment;
            LoadEquipmentData();
        }

        private void LoadEquipmentData()
        {
            EquipmentIdText.Text = currentEquipment.EquipmentId;
            NameText.Text = currentEquipment.Name;
            CategoryText.Text = currentEquipment.Category ?? "N/A";
            QuantityText.Text = currentEquipment.Quantity.ToString();
            ConditionText.Text = currentEquipment.Condition ?? "N/A";

            // Update status summary
            AvailableUnitsText.Text = currentEquipment.Quantity.ToString();
            
            // Set status indicator color based on condition
            string condition = currentEquipment.Condition?.ToLower() ?? "unknown";
            if (condition.Contains("good") || condition.Contains("excellent"))
            {
                StatusIndicatorText.Text = "Good";
                StatusIndicatorText.Foreground = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // Green
            }
            else if (condition.Contains("fair") || condition.Contains("needs"))
            {
                StatusIndicatorText.Text = "Fair";
                StatusIndicatorText.Foreground = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Yellow/Orange
            }
            else if (condition.Contains("poor") || condition.Contains("damaged"))
            {
                StatusIndicatorText.Text = "Poor";
                StatusIndicatorText.Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Red
            }
            else
            {
                StatusIndicatorText.Text = currentEquipment.Condition ?? "Unknown";
                StatusIndicatorText.Foreground = new SolidColorBrush(Color.FromRgb(46, 196, 182)); // Teal
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
