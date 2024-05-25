using System;
using System.Collections.Generic;

namespace CheclLIstAppCreation.DB
{
    public partial class Employee
    {
        public Employee()
        {
            Checklists = new HashSet<Checklist>();
            Shifts = new HashSet<Shift>();
        }

        public int EmployeeId { get; set; }
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? ContactInfo { get; set; }

        public virtual ICollection<Checklist> Checklists { get; set; }
        public virtual ICollection<Shift> Shifts { get; set; }
    }
}
