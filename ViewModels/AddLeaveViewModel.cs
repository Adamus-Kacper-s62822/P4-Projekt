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
        public Employee CurrentEmployee
        {
            get => _currentEmployee;
            set
            {
                _currentEmployee = value;
                OnPropertyChanged();

                LoadEmployeeData();
                UpdateCalculatedDays();
            }
        }

        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); UpdateCalculatedDays(); }
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); UpdateCalculatedDays(); }
        }

        private double _calculatedDays;
        public double CalculatedDays
        {
            get => _calculatedDays;
            set { _calculatedDays = value; OnPropertyChanged(); CheckLimits(); }
        }

        private double _remainingDays;
        public double RemainingDays
        {
            get => _remainingDays;
            set { _remainingDays = value; OnPropertyChanged(); CheckLimits(); }
        }

        private string _validationMessage;
        public string ValidationMessage
        {
            get => _validationMessage;
            set { _validationMessage = value; OnPropertyChanged(); }
        }

        private bool _isSubmitEnabled;
        public bool IsSubmitEnabled
        {
            get => _isSubmitEnabled;
            set { _isSubmitEnabled = value; OnPropertyChanged(); }
        }

        public ICommand SubmitCommand { get; }

        public AddLeaveViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            SubmitCommand = new Command(async () => await Submit(), () => IsSubmitEnabled);

            // Reagowanie na zmianę IsSubmitEnabled
            PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(IsSubmitEnabled))
                    ((Command)SubmitCommand).ChangeCanExecute();
            };
        }

        private async void LoadEmployeeData()
        {
            if (CurrentEmployee != null)
            {
                RemainingDays = await _dbService.GetRemainingLeaveDays(CurrentEmployee.Id, StartDate.Year);
            }
        }

        private async void UpdateCalculatedDays()
        {
            if (EndDate < StartDate)
            {
                CalculatedDays = 0;
                return;
            }
            CalculatedDays = await _dbService.CalculateWorkingDays(StartDate, EndDate);
        }

        private void CheckLimits()
        {
            if (CalculatedDays <= 0)
            {
                ValidationMessage = "Wybierz poprawne daty robocze.";
                IsSubmitEnabled = false;
            }
            else if (CalculatedDays > RemainingDays)
            {
                ValidationMessage = $"Błąd: Przekroczono limit! (Dostępne: {RemainingDays})";
                IsSubmitEnabled = true;
            }
            else
            {
                ValidationMessage = string.Empty;
                IsSubmitEnabled = true;
            }
        }

        private async Task Submit()
        {
            if (CalculatedDays <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Błąd", "Nie można zatwierdzić wniosku o zerowym wymiarze dni roboczych.", "OK");
                return;
            }

            if (CalculatedDays > RemainingDays)
            {
                bool proceed = await Shell.Current.DisplayAlertAsync("Przekroczenie limitu",
                    $"Pracownikowi brakuje {CalculatedDays - RemainingDays} dni. Czy mimo to zapisać wniosek?", "Tak", "Nie");

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