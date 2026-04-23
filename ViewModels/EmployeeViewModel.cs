using Projekt.Models;
using Projekt.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Projekt.ViewModels
{
    public class EmployeeViewModel : BindableObject
    {
        private readonly DatabaseService _dbService;
        private List<Employee> _allEmployees = new();
        private string _searchText;
        private int _totalEmployees;
        private int _employeesOnLeaveToday;

        public ObservableCollection<Employee> Employees { get; } = new();

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public int TotalEmployees
        {
            get => _totalEmployees;
            set { _totalEmployees = value; OnPropertyChanged(); }
        }

        public int EmployeesOnLeaveToday
        {
            get => _employeesOnLeaveToday;
            set { _employeesOnLeaveToday = value; OnPropertyChanged(); }
        }

        public ICommand GoToDetailsCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        public EmployeeViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            GoToDetailsCommand = new Command<Employee>(async (e) => await GoToDetails(e));
            DeleteEmployeeCommand = new Command<Employee>(async (e) => await OnDeleteEmployee(e));

            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            _allEmployees = await _dbService.GetAllEmployees();
            TotalEmployees = _allEmployees.Count;
            EmployeesOnLeaveToday = await _dbService.GetEmployeesOnLeaveTodayCount();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            Employees.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? _allEmployees
                : _allEmployees.Where(e =>
                    e.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.ReferenceNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var e in filtered) Employees.Add(e);
        }

        private async Task GoToDetails(Employee emp)
        {
            if (emp == null) return;
            await Shell.Current.GoToAsync("EmployeeDetailPage", new Dictionary<string, object> { { "Employee", emp } });
        }

        private async Task OnDeleteEmployee(Employee emp)
        {
            if (emp == null) return;

            bool confirm = await Shell.Current.DisplayAlertAsync("Potwierdzenie",
                $"Usunąć pracownika {emp.FirstName} {emp.LastName}?", "Tak", "Nie");

            if (confirm)
            {
                await _dbService.DeleteEmployeeAsync(emp.Id);
                await InitializeAsync();
            }
        }
    }
}