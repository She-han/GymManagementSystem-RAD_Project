using System.Windows;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using GymManagementSystem.DAL;
using Microsoft.Data.Sqlite;

namespace GymManagementSystem.UI
{
    public partial class AddMemberDialog : Window
    {
        public string LastError { get; set; }
        public AddMemberDialog()
        {
            InitializeComponent();
            MemberIdText.Text = MemberService.GetNextMemberId();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameText.Text.Trim();
            string contact = ContactText.Text.Trim();
            string trainer = TrainerText.Text.Trim();
            string subType = SubTypeCombo.SelectedItem as string ?? "";
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(contact) || string.IsNullOrWhiteSpace(subType))
            {
                LastError = "Please fill all required fields.";
                DialogResult = false;
                return;
            }
            var member = new Member
            {
                MemberId = MemberIdText.Text,
                FullName = fullName,
                ContactNumber = contact,
                TrainerName = trainer,
                SubscriptionType = subType,
                JoinDate = System.DateTime.Now.ToString("yyyy-MM-dd")
            };
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Members (MemberId, FullName, TrainerName, JoinDate, SubscriptionType, ContactNumber) VALUES (@id, @name, @trainer, @date, @type, @contact)";
                cmd.Parameters.AddWithValue("@id", member.MemberId);
                cmd.Parameters.AddWithValue("@name", member.FullName);
                cmd.Parameters.AddWithValue("@trainer", member.TrainerName ?? "");
                cmd.Parameters.AddWithValue("@date", member.JoinDate);
                cmd.Parameters.AddWithValue("@type", member.SubscriptionType ?? "");
                cmd.Parameters.AddWithValue("@contact", member.ContactNumber ?? "");
                cmd.ExecuteNonQuery();
                DialogResult = true;
            }
            catch
            {
                LastError = "Failed to add member.";
                DialogResult = false;
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}