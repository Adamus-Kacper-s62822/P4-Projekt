using Projekt.Models;
using Projekt.Services;
using System.Collections.ObjectModel;
using System.Text;
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
            set
            {
                _currentEmployee = value;
                OnPropertyChanged();
                LoadDetails();
            }
        }

        private double _remainingDays;
        public double RemainingDays
        {
            get => _remainingDays;
            set { _remainingDays = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Leave> Leaves { get; set; } = new();
        public ICommand AddLeaveCommand { get; }
        public ICommand DeleteLeaveCommand { get; }
        public ICommand ExportReportCommand { get; }

        public EmployeeDetailViewModel(DatabaseService dbService)
        {
            _dbService = dbService;

            AddLeaveCommand = new Command(async () =>
                await Shell.Current.GoToAsync("AddLeavePage",
                new Dictionary<string, object> { { "Employee", CurrentEmployee } }));

            DeleteLeaveCommand = new Command<Leave>(async (leave) => await OnDeleteLeave(leave));
            ExportReportCommand = new Command(async () => await OnExportReport());
        }

        public async void LoadDetails()
        {
            if (CurrentEmployee == null) return;

            // Pobieramy pozostałe dni
            RemainingDays = await _dbService.GetRemainingLeaveDays(CurrentEmployee.Id, DateTime.Now.Year);

            // Pobieramy historię
            var history = await _dbService.GetEmployeeLeaves(CurrentEmployee.Id);
            Leaves.Clear();
            foreach (var leave in history)
                Leaves.Add(leave);
        }

        private async Task OnDeleteLeave(Leave leave)
        {
            if (leave == null) return;

            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Potwierdzenie",
                $"Czy na pewno usunąć wniosek urlopowy ({leave.Days} dni)?",
                "Tak", "Anuluj");

            if (confirm)
            {
                // Tutaj używamy Twojej nowej metody z DatabaseService
                await _dbService.DeleteLeaveAsync(leave.Id);

                // Odświeżamy widok
                LoadDetails();
            }
        }

        private async Task OnExportReport()
        {
            if (CurrentEmployee == null) return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("==========================================");
                sb.AppendLine("        KARTA URLOPOWA PRACOWNIKA         ");
                sb.AppendLine("==========================================");
                sb.AppendLine($"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm}");
                sb.AppendLine($"Pracownik: {CurrentEmployee.FirstName} {CurrentEmployee.LastName}");
                sb.AppendLine($"Numer ref: {CurrentEmployee.ReferenceNumber}");
                sb.AppendLine($"Pozostały urlop: {RemainingDays} dni");
                sb.AppendLine("------------------------------------------");
                sb.AppendLine("HISTORIA WNIOSKÓW:");
                sb.AppendLine("------------------------------------------");

                if (Leaves.Count == 0)
                {
                    sb.AppendLine("Brak zarejestrowanych wniosków.");
                }
                else
                {
                    foreach (var leave in Leaves)
                    {
                        sb.AppendLine($"{leave.StartDate:dd.MM.yyyy} - {leave.EndDate:dd.MM.yyyy} | Dni: {leave.Days}");
                    }
                }
                sb.AppendLine("------------------------------------------");
                sb.AppendLine("Podpis kadrowego: ........................");

                // Ścieżka do folderu "Moje Dokumenty"
                string fileName = $"Raport_Urlopowy_{CurrentEmployee.LastName}_{DateTime.Now:yyyyMMdd}.txt";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                // Zapis pliku
                await File.WriteAllTextAsync(filePath, sb.ToString());

                await Shell.Current.DisplayAlertAsync("Raport wygenerowany",
                    $"Plik został zapisany w folderze Dokumenty:\n{fileName}", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Błąd", $"Nie udało się wygenerować raportu: {ex.Message}", "OK");
            }
        }
    }
}