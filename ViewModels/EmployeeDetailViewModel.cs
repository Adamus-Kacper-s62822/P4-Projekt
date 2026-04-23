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
        private double _remainingDays;

        public Employee CurrentEmployee
        {
            get => _currentEmployee;
            set { _currentEmployee = value; OnPropertyChanged(); _ = LoadDetails(); }
        }

        public double RemainingDays
        {
            get => _remainingDays;
            set { _remainingDays = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Leave> Leaves { get; } = new();
        public ICommand AddLeaveCommand { get; }
        public ICommand DeleteLeaveCommand { get; }
        public ICommand ExportReportCommand { get; }

        public EmployeeDetailViewModel(DatabaseService dbService)
        {
            _dbService = dbService;

            AddLeaveCommand = new Command(async () =>
                await Shell.Current.GoToAsync("AddLeavePage",
                new Dictionary<string, object> { { "Employee", CurrentEmployee } }));

            DeleteLeaveCommand = new Command<Leave>(async (l) => await OnDeleteLeave(l));
            ExportReportCommand = new Command(async () => await OnExportReport());
        }

        public async Task LoadDetails()
        {
            if (CurrentEmployee == null) return;

            RemainingDays = await _dbService.GetRemainingLeaveDays(CurrentEmployee.Id, DateTime.Now.Year);

            var history = await _dbService.GetEmployeeLeaves(CurrentEmployee.Id);
            Leaves.Clear();
            foreach (var leave in history) Leaves.Add(leave);
        }

        private async Task OnDeleteLeave(Leave leave)
        {
            if (leave == null) return;

            bool confirm = await Shell.Current.DisplayAlertAsync("Potwierdzenie",
                $"Czy usunąć wniosek ({leave.Days} dni)?", "Tak", "Anuluj");

            if (confirm)
            {
                await _dbService.DeleteLeaveAsync(leave.Id);
                await LoadDetails();
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
                sb.AppendLine($"Pracownik: {CurrentEmployee.FirstName} {CurrentEmployee.LastName}");
                sb.AppendLine($"Dostępny urlop: {RemainingDays} dni");
                sb.AppendLine("------------------------------------------");

                if (Leaves.Count == 0) sb.AppendLine("Brak wniosków.");
                else
                {
                    foreach (var l in Leaves)
                        sb.AppendLine($"{l.StartDate:dd.MM.yyyy} - {l.EndDate:dd.MM.yyyy} | Dni: {l.Days}");
                }

                string fileName = $"Raport_{CurrentEmployee.LastName}_{DateTime.Now:yyyyMMdd}.txt";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                await File.WriteAllTextAsync(filePath, sb.ToString());
                await Shell.Current.DisplayAlertAsync("Sukces", $"Zapisano: {fileName}", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Błąd", ex.Message, "OK");
            }
        }
    }
}