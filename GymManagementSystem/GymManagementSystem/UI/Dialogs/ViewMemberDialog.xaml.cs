using System.Windows;
using GymManagementSystem.Models;

namespace GymManagementSystem.UI.Dialogs
{
    public partial class ViewMemberDialog : Window
    {
        public ViewMemberDialog(Member member)
        {
            InitializeComponent();
            LoadMemberData(member);
        }

        private void LoadMemberData(Member member)
        {
            MemberIdText.Text = member.MemberId;
            FullNameText.Text = member.FullName;
            ContactText.Text = member.ContactNumber ?? "N/A";
            TrainerText.Text = member.TrainerName ?? "No trainer assigned";
            JoinDateText.Text = member.JoinDate ?? "N/A";
            SubscriptionText.Text = member.SubscriptionType ?? "N/A";
            MedicalHistoryText.Text = member.MedicalHistory ?? "No medical history recorded";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}