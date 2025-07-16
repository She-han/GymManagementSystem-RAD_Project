using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string EquipmentId { get; set; } = ""; // e.g., "EQP001"
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string? Condition { get; set; }
        public string? Category { get; set; }
    }
}