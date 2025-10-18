using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.Models
{
    public class Attendance
    {
        public int MemberId { get; set; }
        public string? Date { get; set; }
        public string? TimeIn { get; set; }
        public string? TimeOut { get; set; }
    }
}
