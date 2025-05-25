using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string MemberId { get; set; } = ""; // e.g., "MEM001"
        public string FullName { get; set; } = "";
        public string? TrainerName { get; set; }
        public string? JoinDate { get; set; }
        public string? SubscriptionType { get; set; } // "Daily Payment" or "Monthly Payment"
        public string? ContactNumber { get; set; }
        public string? MedicalHistory { get; set; }
    }
}
