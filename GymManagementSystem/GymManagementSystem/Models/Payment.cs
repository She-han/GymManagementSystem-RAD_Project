using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string? PaymentId { get; set; } // e.g., "PAY001"
        public int MemberId { get; set; }
        public double Amount { get; set; }
        public string? Date { get; set; }
        public string Status { get; set; }

    }
}