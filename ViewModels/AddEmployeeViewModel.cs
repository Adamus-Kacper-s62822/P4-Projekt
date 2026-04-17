using Projekt.Models;
using Projekt.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace Projekt.ViewModels
{
    public class AddEmployeeViewModel : BindableObject
    {
        private readonly DatabaseService _dbService;

        // Pola formularza
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime EmploymentDate { get; set; } = DateTime.Now;

        private EmploymentType _selectedType;
        public EmploymentType SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); }
        }

        public ObservableCollection<EmploymentType> EmploymentTypes { get; set; } = new();
        public ICommand SaveCommand { get; }

        public AddEmployeeViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            SaveCommand = new Command(async () => await Save());
            _ = LoadTypes();
        }

        private async Task LoadTypes()
        {
            var types = await _dbService.GetEmploymentTypes();
            foreach (var t in types) EmploymentTypes.Add(t);
        }

        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || SelectedType == null) return;

            var newEmp = new Employee
            {
                FirstName = FirstName,
                LastName = LastName,
                ReferenceNumber = ReferenceNumber,
                EmploymentDate = EmploymentDate,
                EmploymentTypeId = SelectedType.Id,
                WorkingHoursCount = 40
            };

            await _dbService.SaveEmployee(newEmp);
            await Shell.Current.GoToAsync("..");
        }
    }
}
