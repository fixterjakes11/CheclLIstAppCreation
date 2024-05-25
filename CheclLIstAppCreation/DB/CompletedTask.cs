using System;
using System.Collections.Generic;

namespace CheclLIstAppCreation.DB
{
    public partial class CompletedTask
    {
        public int CompletedTaskId { get; set; }
        public int? TaskId { get; set; }
        public int? ChecklistId { get; set; }
        public string Status { get; set; } = null!;

        public virtual Checklist? Checklist { get; set; }
        public virtual Task? Task { get; set; }
    }
}
