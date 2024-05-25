using System;
using System.Collections.Generic;

namespace CheclLIstAppCreation.DB
{
    public partial class Task
    {
        public Task()
        {
            CompletedTasks = new HashSet<CompletedTask>();
        }

        public int TaskId { get; set; }
        public string TaskName { get; set; } = null!;
        public string? TaskDescription { get; set; }

        public virtual ICollection<CompletedTask> CompletedTasks { get; set; }
    }
}
