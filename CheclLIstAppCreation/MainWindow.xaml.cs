using CheclLIstAppCreation.DB;
using CheclLIstAppCreation.Veiw;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using CheclLIstAppCreation.View;

namespace CheclLIstAppCreation
{

    public partial class MainWindow : Window
    {
        private ChekListCreateContext _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = new ChekListCreateContext();
            LoadEmployees();
            LoadChecklists();
        }

        private void LoadEmployees()
        {
            var employees = _context.Employees.ToList();
            EmployeeComboBox.ItemsSource = employees;
        }

        private void LoadChecklists()
        {
            var checklists = _context.Checklists.Include("Shift.Employee").OrderByDescending(c => c.ChecklistDate).ToList();
            ChecklistsDataGrid.ItemsSource = checklists;
        }

        private void CloseShiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem != null)
            {
                var selectedEmployee = (Employee)EmployeeComboBox.SelectedItem;

                var activeShift = _context.Shifts
               .Where(s => s.EmployeeId == selectedEmployee.EmployeeId && s.EndTime == null)
                   .OrderByDescending(s => s.StartTime)
                   .FirstOrDefault();

                if (activeShift == null)
                {
                    MessageBox.Show("Не найдено открытых смен для данного работника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var closeShiftWindow = new CloseShiftChecklisWindow(selectedEmployee);

                // ShowDialog возвращает Nullable<bool>
                bool? dialogResult = closeShiftWindow.ShowDialog();

                if (dialogResult == true)
                {
                    LoadChecklists(); 
                }

            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите работника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void OpenShiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem != null)
            {
                var selectedEmployee = (Employee)EmployeeComboBox.SelectedItem;

                // Проверка на наличие открытой смены
                var activeShift = _context.Shifts
                                          .Where(s => s.EmployeeId == selectedEmployee.EmployeeId && s.EndTime == null)
                                          .FirstOrDefault();

                if (activeShift != null)
                {
                    MessageBox.Show("У данного работника уже есть открытая смена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var openShiftWindow = new OpenShiftChecklistWindow(selectedEmployee);

                bool? dialogResult = openShiftWindow.ShowDialog();

                if (dialogResult == true)
                {
                    LoadChecklists();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите работника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateCloseShiftCheckListDocument_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = CreateCloseChecklistDocument();
                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Печать чек-листа закрытия смены");
            }
        }

        private void CreateOpenShiftChecklistDocument_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = CreateOpenChecklistDocument();
                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Печать чек-листа открытия смены");
            }
        }

        private void PrintDocument(FlowDocument document)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = document;
                pd.PrintDocument(idpSource.DocumentPaginator, "Печать чек-листа");
            }
        }
        private FlowDocument CreateCloseChecklistDocument()
        {
            // Создание документа
            FlowDocument doc = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
                FontSize = 14,
                LineHeight = 21, 
                PageWidth = 793.7,  
                PageHeight = 1122.5, 
                PagePadding = new Thickness(56.7, 56.7, 28.35, 56.7) 
            };

            // Настройки для текста
            Style paragraphStyle = new Style(typeof(Paragraph))
            {
                Setters =
                {
                    // Отступы между параграфами
                    new Setter(Paragraph.MarginProperty, new Thickness(0, 0, 0, 10)), 
                    // Выравнивание текста по левому краю
                    new Setter(Paragraph.TextAlignmentProperty, TextAlignment.Left) 
                }
            };
            doc.Resources.Add(typeof(Paragraph), paragraphStyle);

            // Заголовок
            Paragraph title = new Paragraph(new Bold(new Run("Чек лист")))
            {
                FontSize = 24,
                TextAlignment = TextAlignment.Center
            };
            doc.Blocks.Add(title);

            Paragraph subtitle = new Paragraph(new Bold(new Run("для закрытия смены")))
            {
                FontSize = 18,
                TextAlignment = TextAlignment.Center
            };
            doc.Blocks.Add(subtitle);

            // Информация о работнике
            Paragraph employeeInfo = new Paragraph();
            employeeInfo.Inlines.Add(new Bold(new Run("Имя работника: ______________\n"))); 
            employeeInfo.Inlines.Add(new Bold(new Run("Дата: ______________\n"))); 
            employeeInfo.Inlines.Add(new Bold(new Run("Должность: ______________\n")));
            doc.Blocks.Add(employeeInfo);

            // Таблица задач
            System.Windows.Documents.Table taskTable = new System.Windows.Documents.Table
            {
                CellSpacing = 0,
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1),
                TextAlignment = TextAlignment.Left
            };

            // Настройка столбцов таблицы
            taskTable.Columns.Add(new TableColumn() { Width = new GridLength(3, GridUnitType.Star) });
            taskTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup trg = new TableRowGroup();
            taskTable.RowGroups.Add(trg);

            // Заголовок таблицы
            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Задача"))) { TextAlignment = TextAlignment.Center })
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1)
            });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Ответ"))) { TextAlignment = TextAlignment.Center })
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1)
            });
            trg.Rows.Add(headerRow);

            // Добавление задач в таблицу
            var tasks = _context.Tasks.Where(t => t.TaskName == "Закрытие смены").ToList();
            foreach (var task in tasks)
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(task.TaskDescription)) { TextAlignment = TextAlignment.Left })
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 0, 1)
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run("да/нет")) { TextAlignment = TextAlignment.Center })
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 0, 1)
                });
                trg.Rows.Add(row);
            }

            // Добавляем границы для всей таблицы
            foreach (var row in trg.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    cell.BorderBrush = System.Windows.Media.Brushes.Black;
                    cell.BorderThickness = new Thickness(0, 0, 0, 1);
                }
            }

            taskTable.TextAlignment = TextAlignment.Left;
            doc.Blocks.Add(taskTable);

            // Поля подписи
            Paragraph signature = new Paragraph
            {
                TextAlignment = TextAlignment.Left
            };
            signature.Inlines.Add(new Bold(new Run("Подпись: _____________\n")));
            signature.Inlines.Add(new Bold(new Run("Расшифровка подписи: ____________________________")));
            doc.Blocks.Add(signature);

            return doc;
        }
        private FlowDocument CreateOpenChecklistDocument()
        {
            // Создание документа
            FlowDocument doc = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
                FontSize = 14,
                LineHeight = 21,
                PageWidth = 793.7,  
                PageHeight = 1122.5,
                PagePadding = new Thickness(56.7, 56.7, 28.35, 56.7) 
            };

            // Настройки для текста
            Style paragraphStyle = new Style(typeof(Paragraph))
            {
                Setters =
                {
                    new Setter(Paragraph.MarginProperty, new Thickness(0, 0, 0, 10)),
                    new Setter(Paragraph.TextAlignmentProperty, TextAlignment.Left) 
                }
            };
            doc.Resources.Add(typeof(Paragraph), paragraphStyle);

            // Заголовок
            Paragraph title = new Paragraph(new Bold(new Run("Чек лист")))
            {
                FontSize = 24,
                TextAlignment = TextAlignment.Center
            };
            doc.Blocks.Add(title);

            Paragraph subtitle = new Paragraph(new Bold(new Run("для открытия смены")))
            {
                FontSize = 18,
                TextAlignment = TextAlignment.Center
            };
            doc.Blocks.Add(subtitle);

            // Информация о работнике
            Paragraph employeeInfo = new Paragraph();
            employeeInfo.Inlines.Add(new Bold(new Run("Имя работника: ______________\n"))); 
            employeeInfo.Inlines.Add(new Bold(new Run("Дата: ______________\n"))); 
            employeeInfo.Inlines.Add(new Bold(new Run("Должность: ______________\n"))); 
            doc.Blocks.Add(employeeInfo);

            // Таблица задач
            System.Windows.Documents.Table taskTable = new System.Windows.Documents.Table
            {
                CellSpacing = 0,
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1),
                TextAlignment = TextAlignment.Left
            };

            // Настройка столбцов таблицы
            taskTable.Columns.Add(new TableColumn() { Width = new GridLength(3, GridUnitType.Star) });
            taskTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup trg = new TableRowGroup();
            taskTable.RowGroups.Add(trg);

            // Заголовок таблицы
            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Задача"))) { TextAlignment = TextAlignment.Center })
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1)
            });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Ответ"))) { TextAlignment = TextAlignment.Center })
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1)
            });
            trg.Rows.Add(headerRow);

            // Добавление задач в таблицу
            var tasks = _context.Tasks.Where(t => t.TaskName == "Открытие смены").ToList();
            foreach (var task in tasks)
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(task.TaskDescription)) { TextAlignment = TextAlignment.Left })
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 0, 1)
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run("да/нет")) { TextAlignment = TextAlignment.Center })
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 0, 1)
                });
                trg.Rows.Add(row);
            }

            // Добавляем границы для всей таблицы
            foreach (var row in trg.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    cell.BorderBrush = System.Windows.Media.Brushes.Black;
                    cell.BorderThickness = new Thickness(0, 0, 0, 1);
                }
            }

            taskTable.TextAlignment = TextAlignment.Left; 
            doc.Blocks.Add(taskTable);

            // Поля подписи
            Paragraph signature = new Paragraph
            {
                TextAlignment = TextAlignment.Left
            };
            signature.Inlines.Add(new Bold(new Run("Подпись: _____________\n")));
            signature.Inlines.Add(new Bold(new Run("Расшифровка подписи: ____________________________")));
            doc.Blocks.Add(signature);

            return doc;
        }

        private void PrintSelectedChecklistsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChecklistsDataGrid.SelectedItem is Checklist selectedChecklist)
            {
                FlowDocument doc = CreateCopletedChecklistDocument(selectedChecklist);
                PrintDocument(doc);
            }
            else
            {
                MessageBox.Show("Выберите чек-лист для печати.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CreateCopletedChecklistDocument(Checklist checklist)
        {
            // Создание документа
            FlowDocument doc = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman"),
                FontSize = 14,
                LineHeight = 21, 
                PageWidth = 793.7,  
                PageHeight = 1122.5,
                PagePadding = new Thickness(56.7, 56.7, 28.35, 56.7) 
            };

            // Настройки для текста
            Style paragraphStyle = new Style(typeof(Paragraph))
            {
                Setters =
                {
                    new Setter(Paragraph.MarginProperty, new Thickness(0, 0, 0, 10)), 
                    new Setter(Paragraph.TextAlignmentProperty, TextAlignment.Left) 
                }
            };
            doc.Resources.Add(typeof(Paragraph), paragraphStyle);

            // Заголовок
            Paragraph title = new Paragraph(new Bold(new Run("Чек лист")))
            {
                FontSize = 24,
                TextAlignment = TextAlignment.Center
            };
            doc.Blocks.Add(title);

            Paragraph subtitle = new Paragraph(new Bold(new Run(checklist.Name)))
            {
                FontSize = 18,
                TextAlignment = TextAlignment.Center
            };
            doc.Blocks.Add(subtitle);

            // Информация о работнике
            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == checklist.EmployeeId);
            Paragraph employeeInfo = new Paragraph();
            employeeInfo.Inlines.Add(new Bold(new Run($"Имя работника: {employee?.FullName.ToString()}\n")));
            employeeInfo.Inlines.Add(new Bold(new Run($"Дата: {checklist.ChecklistDate.ToShortDateString()}\n")));
            employeeInfo.Inlines.Add(new Bold(new Run($"Должность: {employee?.Role.ToString()}\n")));
            doc.Blocks.Add(employeeInfo);

            // Таблица задач
            System.Windows.Documents.Table taskTable = new System.Windows.Documents.Table
            {
                CellSpacing = 0,
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1),
                TextAlignment = TextAlignment.Left
            };

            // Настройка столбцов таблицы
            taskTable.Columns.Add(new TableColumn() { Width = new GridLength(3, GridUnitType.Star) });
            taskTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup trg = new TableRowGroup();
            taskTable.RowGroups.Add(trg);

            // Заголовок таблицы
            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Задача"))) { TextAlignment = TextAlignment.Center })
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1)
            });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Ответ"))) { TextAlignment = TextAlignment.Center })
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1)
            });
            trg.Rows.Add(headerRow);

            // Добавление задач в таблицу
            var tasks = _context.CompletedTasks.Include(ct => ct.Task)
                                               .Where(ct => ct.ChecklistId == checklist.ChecklistId)
                                               .ToList();

            foreach (var completedTask in tasks)
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(completedTask.Task.TaskDescription)) { TextAlignment = TextAlignment.Left })
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 0, 1)
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(completedTask.Status)) { TextAlignment = TextAlignment.Center })
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 0, 1)
                });
                trg.Rows.Add(row);
            }

            taskTable.TextAlignment = TextAlignment.Left; 
            doc.Blocks.Add(taskTable);

            // Поля подписи
            Paragraph signature = new Paragraph
            {
                TextAlignment = TextAlignment.Left
            };
            signature.Inlines.Add(new Bold(new Run("Подпись: ____________________\n")));
            signature.Inlines.Add(new Bold(new Run("Расшифровка подписи: ____________________________")));
            doc.Blocks.Add(signature);

            return doc;
        }


    }
}
    

