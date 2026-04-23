using Projekt.Models;
using Projekt.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Projekt.ViewModels
{
    public class AddEmployeeViewModel : BindableObject
    {
        private readonly DatabaseService _dbService;

        private string _firstName;
        private string _lastName;
        private string _referenceNumber;
        private EmploymentType _selectedType;

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string ReferenceNumber
        {
            get => _referenceNumber;
            set { _referenceNumber = value; OnPropertyChanged(); }
        }

        public DateTime EmploymentDate { get; set; } = DateTime.Now;

        public EmploymentType SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); }
        }

        public ObservableCollection<EmploymentType> EmploymentTypes { get; } = new();
        public ICommand SaveCommand { get; }

        public AddEmployeeViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            SaveCommand = new Command(async () => await OnSave());

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var types = await _dbService.GetEmploymentTypes();
            foreach (var type in types)
                EmploymentTypes.Add(type);
        }

        private async Task OnSave()
        {
            if (string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                SelectedType == null)
            {
                await Shell.Current.DisplayAlertAsync("Błąd", "Wypełnij wymagane pola (Imię, Nazwisko, Typ umowy).", "OK");
                return;
            }

            var allEmployees = await _dbService.GetAllEmployees();
            if (allEmployees.Any(e => e.ReferenceNumber == ReferenceNumber))
            {
                await Shell.Current.DisplayAlertAsync("Błąd",
                    $"Numer referencyjny '{ReferenceNumber}' jest już przypisany do innego pracownika.", "OK");
                return;
            }

            var newEmp = new Employee
            {
                FirstName = FirstName,
                LastName = LastName,
                ReferenceNumber = ReferenceNumber ?? string.Empty,
                EmploymentDate = EmploymentDate,
                EmploymentTypeId = SelectedType.Id,
                WorkingHoursCount = 40
            };

            try
            {
                await _dbService.SaveEmployee(newEmp);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Błąd zapisu", ex.Message, "OK");
            }
        }
    }
}