using Projekt.Models;
using Projekt.Services;
using System.Windows.Input;

namespace Projekt.ViewModels
{
    [QueryProperty(nameof(CurrentEmployee), "Employee")]
    public class AddLeaveViewModel : BindableObject
    {
        private readonly DatabaseService _dbService;
        private Employee _currentEmployee;
        private DateTime _startDate = DateTime.Today;
        private DateTime _endDate = DateTime.Today;
        private double _calculatedDays;
        private double _remainingDays;
        private string _validationMessage;
        private bool _isSubmitEnabled;

        public Employee CurrentEmployee
        {
            get => _currentEmployee;
            set { _currentEmployee = value; OnPropertyChanged(); _ = InitializeData(); }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); _ = UpdateView(); }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); _ = UpdateView(); }
        }

        public double CalculatedDays
        {
            get => _calculatedDays;
            set { _calculatedDays = value; OnPropertyChanged(); }
        }

        public double RemainingDays
        {
            get => _remainingDays;
            set { _remainingDays = value; OnPropertyChanged(); }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set { _validationMessage = value; OnPropertyChanged(); }
        }

        public bool IsSubmitEnabled
        {
            get => _isSubmitEnabled;
            set
            {
                _isSubmitEnabled = value;
                OnPropertyChanged();
                ((Command)SubmitCommand).ChangeCanExecute();
            }
        }

        public ICommand SubmitCommand { get; }

        public AddLeaveViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            SubmitCommand = new Command(async () => await OnSubmit(), () => IsSubmitEnabled);
        }

        private async Task InitializeData()
        {
            if (CurrentEmployee == null) return;
            RemainingDays = await _dbService.GetRemainingLeaveDays(CurrentEmployee.Id, StartDate.Year);
            await UpdateView();
        }

        private async Task UpdateView()
        {
            if (CurrentEmployee == null) return;

            if (EndDate < StartDate)
            {
                CalculatedDays = 0;
                ValidationMessage = "Data końcowa nie może być przed początkową.";
                IsSubmitEnabled = false;
                return;
            }

            CalculatedDays = await _dbService.CalculateWorkingDays(StartDate, EndDate);

            if (CalculatedDays <= 0)
            {
                ValidationMessage = "Wybierz dni robocze.";
                IsSubmitEnabled = false;
            }
            else if (CalculatedDays > RemainingDays)
            {
                ValidationMessage = $"Uwaga: Przekroczono limit (Dostępne: {RemainingDays})";
                IsSubmitEnabled = true; // Pozwalamy zapisać, ale z ostrzeżeniem w Submit
            }
            else
            {
                ValidationMessage = string.Empty;
                IsSubmitEnabled = true;
            }
        }

        private async Task OnSubmit()
        {
            if (CalculatedDays > RemainingDays)
            {
                bool proceed = await Shell.Current.DisplayAlertAsync("Przekroczenie limitu",
                    $"Pracownikowi brakuje {CalculatedDays - RemainingDays} dni. Kontynuować?", "Tak", "Nie");
                if (!proceed) return;
            }

            var leave = new Leave
            {
                EmployeeId = CurrentEmployee.Id,
                LeaveTypeId = 1,
                StartDate = StartDate,
                EndDate = EndDate,
                Days = CalculatedDays,
                Year = StartDate.Year,
                Status = 2
            };

            await _dbService.SaveLeave(leave);
            await Shell.Current.GoToAsync("..");
        }
    }
}