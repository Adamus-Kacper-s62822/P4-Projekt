using Projekt.Models;
using Projekt.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace Projekt.ViewModels
{
    public class EmployeeViewModel : BindableObject
    {
        private readonly DatabaseService _dbService;

        public ObservableCollection<Employee> Employees { get; set; } = new();

        private int _totalEmployees;
        public int TotalEmployees { get => _totalEmployees; set { _totalEmployees = value; OnPropertyChanged(); } }

        private int _employeesOnLeaveToday;
        public int EmployeesOnLeaveToday { get => _employeesOnLeaveToday; set { _employeesOnLeaveToday = value; OnPropertyChanged(); } }

        public ICommand GoToDetailsCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        public EmployeeViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            GoToDetailsCommand = new Command<Employee>(async (emp) => await GoToDetails(emp));
            DeleteEmployeeCommand = new Command<Employee>(async (emp) => await DeleteEmployee(emp));
            LoadData();
        }

        public async void LoadData()
        {
            var emps = await _dbService.GetAllEmployees();
            _allEmployees = emps.OrderBy(e => e.LastName).ToList();

            ApplyFilter();

            TotalEmployees = emps.Count;
            EmployeesOnLeaveToday = await _dbService.GetEmployeesOnLeaveTodayCount();
        }

        private async Task GoToDetails(Employee emp)
        {
            await Shell.Current.GoToAsync("EmployeeDetailPage", new Dictionary<string, object> { { "Employee", emp } });
        }

        private async Task DeleteEmployee(Employee emp)
        {
            bool confirm = await Shell.Current.DisplayAlertAsync("Potwierdzenie",
                $"Czy na pewno chcesz usunąć pracownika {emp.FirstName} {emp.LastName}?", "Tak", "Nie");

            if (confirm)
            {
                await _dbService.DeleteEmployeeAsync(emp.Id);
                LoadData();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        private List<Employee> _allEmployees = new();

        private void ApplyFilter()
        {
            Employees.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allEmployees
                : _allEmployees.Where(e =>
                    e.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.ReferenceNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var e in filtered)
                Employees.Add(e);
        }
    }
}
