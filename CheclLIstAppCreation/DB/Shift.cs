using System;
using System.Collections.Generic;

namespace CheclLIstAppCreation.DB
{
    public partial class Shift
    {
        public Shift()
        {
            Checklists = new HashSet<Checklist>();
        }

        public int ShiftId { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public virtual Employee? Employee { get; set; }
        public virtual ICollection<Checklist> Checklists { get; set; }
    }
}
