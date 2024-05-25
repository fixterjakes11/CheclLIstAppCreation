
using CheclLIstAppCreation.DB;
using Microsoft.EntityFrameworkCore;
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

namespace CheclLIstAppCreation.Veiw
{
    /// <summary>
    /// Логика взаимодействия для CloseShiftChecklistWindow.xaml
    /// </summary>
    public partial class CloseShiftChecklisWindow : Window
    {
        public class TaskViewModel
        {
            public int TaskID { get; set; }
            public string TaskDescription { get; set; }
            public string Status { get; set; }
        }

        private Employee _selectedEmployee;
        private Shift _shift;
        private ObservableCollection<TaskViewModel> _tasks;
        private readonly ChekListCreateContext _context;
        public CloseShiftChecklisWindow(Employee selectedEmployee)
        {
            InitializeComponent();
            _selectedEmployee = selectedEmployee;
            _context = new ChekListCreateContext();
            LoadShiftAndTasks();
            BindEmployeeDetails();
        }

        private void LoadShiftAndTasks()
        {
            using (var context = new ChekListCreateContext())
            {
                _shift = context.Shifts.FirstOrDefault(s => s.EmployeeId == _selectedEmployee.EmployeeId && s.EndTime == null);
                if (_shift != null)
                {
                    var tasks = context.Tasks.Where(t => t.TaskName == "Закрытие смены").Select(t => new TaskViewModel
                    {
                        TaskID = t.TaskId,
                        TaskDescription = t.TaskDescription,
                        Status = "Нет"
                    }).ToList();
                    _tasks = new ObservableCollection<TaskViewModel>(tasks);
                    TasksItemsControl.ItemsSource = _tasks;
                }
                else
                {
                    MessageBox.Show("Не найдена активная смена для выбранного сотрудника.");
                    Close();
                }
            }
        }

        private void BindEmployeeDetails()
        {
            EmployeeNameTextBlock.Text = _selectedEmployee.FullName;
            EmployeeRoleTextBlock.Text = _selectedEmployee.Role;
            if (_shift != null)
            {
                ShiftStartTimeTextBlock.Text = _shift.StartTime.ToString();
            }
        }

        private void CloseShiftButton_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new ChekListCreateContext())
            {
                var activeShift = context.Shifts.FirstOrDefault(s => s.EmployeeId == _selectedEmployee.EmployeeId && s.EndTime == null);
                if (activeShift != null)
                {
                    activeShift.EndTime = DateTime.Now;
                    context.SaveChanges();

                    var checklist = new Checklist
                    {
                        ShiftId = _shift.ShiftId,
                        ChecklistDate = DateTime.Now,
                        EmployeeId = _selectedEmployee.EmployeeId,
                        Name = "Закрытие смены"
                    };
                    context.Checklists.Add(checklist);
                    context.SaveChanges();

                    foreach (var task in _tasks)
                    {
                        var completedTask = new CompletedTask
                        {
                            TaskId = task.TaskID,
                            ChecklistId = checklist.ChecklistId,
                            Status = task.Status
                        };
                        context.CompletedTasks.Add(completedTask);
                    }
                    context.SaveChanges();
                    MessageBox.Show("Смена успешно закрыта.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; 
                    this.Close(); 
                }
                else
                {
                    MessageBox.Show("Не найдено открытых смен для данного работника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
    }
}

