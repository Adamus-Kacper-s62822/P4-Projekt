using Projekt.Models;
using Projekt.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Projekt.ViewModels
{
    [QueryProperty(nameof(CurrentEmployee), "Employee")]
    public class EmployeeDetailViewModel : BindableObject
    {
        private readonly DatabaseService _dbService;

        private Employee _currentEmployee;
        public Employee CurrentEmployee
        {
            get => _currentEmployee;
            set { _currentEmployee = value; OnPropertyChanged(); LoadDetails(); }
        }

        private double _remainingDays;
        public double RemainingDays
        {
            get => _remainingDays;
            set { _remainingDays = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Leave> Leaves { get; set; } = new();
        public ICommand AddLeaveCommand { get; }

        public EmployeeDetailViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            AddLeaveCommand = new Command(async () => await Shell.Current.GoToAsync("AddLeavePage",
                new Dictionary<string, object> { { "Employee", CurrentEmployee } }));
        }

        public async void LoadDetails()
        {
            if (CurrentEmployee == null) return;

            RemainingDays = await _dbService.GetRemainingLeaveDays(CurrentEmployee.Id, DateTime.Now.Year);

            var history = await _dbService.GetEmployeeLeaves(CurrentEmployee.Id);
            Leaves.Clear();
            foreach (var leave in history) Leaves.Add(leave);
        }
    }
}
