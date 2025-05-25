using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string TrainerId { get; set; } = ""; // e.g., "TRN001"
        public string FullName { get; set; } = "";
        public string? ContactNumber { get; set; }
        public string? Specialty { get; set; }
    }
}