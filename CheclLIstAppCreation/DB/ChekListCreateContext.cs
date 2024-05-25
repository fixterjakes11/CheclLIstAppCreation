using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CheclLIstAppCreation.DB
{
    public partial class ChekListCreateContext : DbContext
    {
        public ChekListCreateContext()
        {
        }

        public ChekListCreateContext(DbContextOptions<ChekListCreateContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Checklist> Checklists { get; set; } = null!;
        public virtual DbSet<CompletedTask> CompletedTasks { get; set; } = null!;
        public virtual DbSet<Employee> Employees { get; set; } = null!;
        public virtual DbSet<Shift> Shifts { get; set; } = null!;
        public virtual DbSet<Task> Tasks { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=localhost;Database=ChekListCreate;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Checklist>(entity =>
            {
                entity.Property(e => e.ChecklistId).HasColumnName("ChecklistID");

                entity.Property(e => e.ChecklistDate).HasColumnType("datetime");

                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.ShiftId).HasColumnName("ShiftID");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Checklists)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__Checklist__Emplo__47DBAE45");

                entity.HasOne(d => d.Shift)
                    .WithMany(p => p.Checklists)
                    .HasForeignKey(d => d.ShiftId)
                    .HasConstraintName("FK__Checklist__Shift__46E78A0C");
            });

            modelBuilder.Entity<CompletedTask>(entity =>
            {
                entity.Property(e => e.CompletedTaskId).HasColumnName("CompletedTaskID");

                entity.Property(e => e.ChecklistId).HasColumnName("ChecklistID");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.HasOne(d => d.Checklist)
                    .WithMany(p => p.CompletedTasks)
                    .HasForeignKey(d => d.ChecklistId)
                    .HasConstraintName("FK__Completed__Check__4D94879B");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.CompletedTasks)
                    .HasForeignKey(d => d.TaskId)
                    .HasConstraintName("FK__Completed__TaskI__4CA06362");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

                entity.Property(e => e.ContactInfo).HasMaxLength(100);

                entity.Property(e => e.FullName).HasMaxLength(100);

                entity.Property(e => e.Role).HasMaxLength(50);
            });

            modelBuilder.Entity<Shift>(entity =>
            {
                entity.Property(e => e.ShiftId).HasColumnName("ShiftID");

                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Shifts)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__Shifts__Employee__440B1D61");
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.Property(e => e.TaskDescription).HasMaxLength(250);

                entity.Property(e => e.TaskName).HasMaxLength(100);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
