using CheclLIstAppCreation.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static CheclLIstAppCreation.Veiw.CloseShiftChecklisWindow;

namespace CheclLIstAppCreation.View
{
    /// <summary>
    /// Логика взаимодействия для OpenShiftChecklistWindow.xaml
    /// </summary>
    /// <summary>
    /// Логика взаимодействия для OpenShiftChecklistWindow.xaml
    /// </summary>
    public partial class OpenShiftChecklistWindow : Window
    {
        public class TaskViewModel
        {
            public int TaskID { get; set; }
            public string TaskDescription { get; set; }
            public string Status { get; set; }
        }

        private Employee _selectedEmployee;
        private ObservableCollection<TaskViewModel> _tasks;
        private readonly ChekListCreateContext _context;

        public OpenShiftChecklistWindow(Employee selectedEmployee)
        {
            InitializeComponent();
            _selectedEmployee = selectedEmployee;
            _context = new ChekListCreateContext();
            LoadTasks();
            BindEmployeeDetails();
        }

        private void LoadTasks()
        {
            var tasks = _context.Tasks
                                .Where(t => t.TaskName == "Открытие смены")
                                .Select(t => new TaskViewModel
                                {
                                    TaskID = t.TaskId,
                                    TaskDescription = t.TaskDescription,
                                    Status = "Нет"
                                }).ToList();

            _tasks = new ObservableCollection<TaskViewModel>(tasks);
            TasksItemsControl.ItemsSource = _tasks;
        }

        private void BindEmployeeDetails()
        {
            EmployeeNameTextBlock.Text = _selectedEmployee.FullName;
            EmployeeRoleTextBlock.Text = _selectedEmployee.Role;

            var lastShift = _context.Shifts
                                    .Where(s => s.EmployeeId == _selectedEmployee.EmployeeId && s.EndTime != null)
                                    .OrderByDescending(s => s.EndTime)
                                    .FirstOrDefault();

            LastShiftEndTimeTextBlock.Text = lastShift?.EndTime?.ToString() ?? "Никогда";
        }


        private void OpenShiftButton_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new ChekListCreateContext())
            {
                var newShift = new Shift
                {
                    EmployeeId = _selectedEmployee.EmployeeId,
                    StartTime = DateTime.Now
                };

                context.Shifts.Add(newShift);
                context.SaveChanges();

                var newChecklist = new Checklist
                {
                    ShiftId = newShift.ShiftId,
                    ChecklistDate = DateTime.Now,
                    EmployeeId = _selectedEmployee.EmployeeId,
                    Name = "Открытие смены"
                };

                context.Checklists.Add(newChecklist);
                context.SaveChanges();

                foreach (var task in _tasks)
                {
                    var completedTask = new CompletedTask
                    {
                        TaskId = task.TaskID,
                        ChecklistId = newChecklist.ChecklistId,
                        Status = task.Status
                    };

                    context.CompletedTasks.Add(completedTask);
                }

                context.SaveChanges();
                MessageBox.Show("Смена успешно открыта.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; 
                this.Close(); 
            }
        }
    }

}
