using System.Windows;
using GymManagementSystem.Models;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class ViewTrainerDialog : Window
    {
        public ViewTrainerDialog(Trainer trainer)
        {
            InitializeComponent();
            LoadTrainerData(trainer);
        }

        private void LoadTrainerData(Trainer trainer)
        {
            TrainerIdText.Text = trainer.TrainerId;
            FullNameText.Text = trainer.FullName;
            ContactText.Text = trainer.ContactNumber ?? "N/A";
            EmailText.Text = trainer.Email ?? "N/A";
            SpecialtyText.Text = trainer.Specialty ?? "No specialty specified";
            ExperienceText.Text = trainer.Experience ?? "No experience information";
            JoinDateText.Text = trainer.JoinDate ?? "N/A";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}