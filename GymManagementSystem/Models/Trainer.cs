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
        public string TrainerId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? ContactNumber { get; set; }
        public string? Specialty { get; set; }
        public string? Experience { get; set; }
        public string? Email { get; set; }
        public string? JoinDate { get; set; }
    }
}