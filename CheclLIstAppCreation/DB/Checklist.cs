using System;
using System.Collections.Generic;

namespace CheclLIstAppCreation.DB
{
    public partial class Checklist
    {
        public Checklist()
        {
            CompletedTasks = new HashSet<CompletedTask>();
        }

        public int ChecklistId { get; set; }
        public int? ShiftId { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime ChecklistDate { get; set; }
        public string Name { get; set; } = null!;

        public virtual Employee? Employee { get; set; }
        public virtual Shift? Shift { get; set; }
        public virtual ICollection<CompletedTask> CompletedTasks { get; set; }
    }
}
