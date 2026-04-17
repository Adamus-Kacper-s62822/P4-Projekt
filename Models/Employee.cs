using SQLite;

namespace Projekt.Models
{
    [Table("Employee")]
    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string FirstName { get; set; }

        [NotNull]
        public string LastName { get; set; }

        [Unique]
        public string ReferenceNumber { get; set; } // PESEL / ID

        public DateTime EmploymentDate { get; set; }

        [Indexed]
        public int EmploymentTypeId { get; set; }

        public int WorkingHoursCount { get; set; } = 40;

        // Właściwość pomocnicza (nie zapisywana w bazie), ułatwia wyświetlanie na listach
        [Ignore]
        public string FullName => $"{FirstName} {LastName}";
    }

    public class EmployeeDisplay
    {
        public Employee Employee { get; set; }
        public string EmploymentTypeName { get; set; }
        public double RemainingDays { get; set; }
    }
}
