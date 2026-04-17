using Projekt.Models;
using Projekt.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Projekt.ViewModels
{
    [QueryProperty(nameof(CurrentEmployee), "Employee")]
    public class AddLeaveViewModel : BindableObject
    {
        private readonly DatabaseService _dbService;

        private Employee _currentEmployee;
        public Employee CurrentEmployee { get; set; } // Z przekazania parametrów

        private DateTime _startDate = DateTime.Now;
        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); UpdateCalculatedDays(); }
        }

        private DateTime _endDate = DateTime.Now;
        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); UpdateCalculatedDays(); }
        }

        private double _calculatedDays;
        public double CalculatedDays
        {
            get => _calculatedDays;
            set { _calculatedDays = value; OnPropertyChanged(); }
        }

        public ICommand SubmitCommand { get; }

        public AddLeaveViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            SubmitCommand = new Command(async () => await Submit());
            UpdateCalculatedDays();
        }

        private async void UpdateCalculatedDays()
        {
            if (EndDate < StartDate) { CalculatedDays = 0; return; }
            CalculatedDays = await _dbService.CalculateWorkingDays(StartDate, EndDate);
        }

        private async Task Submit()
        {
            if (CurrentEmployee == null)
            {
                await Shell.Current.DisplayAlertAsync("Błąd", "Nie znaleziono pracownika!", "OK");
                return;
            }

            if (CalculatedDays <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Błąd", "Wybierz poprawne daty (dni robocze > 0)", "OK");
                return;
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
