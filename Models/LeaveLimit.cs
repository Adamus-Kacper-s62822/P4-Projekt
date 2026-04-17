using SQLite;

namespace Projekt.Models
{
    [Table("LeaveLimit")]
    public class LeaveLimit
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int EmployeeId { get; set; }

        [Indexed]
        public int LeaveTypeId { get; set; }

        public int Year { get; set; }

        public int DaysLimit { get; set; }

        public int CarryOverDays { get; set; } = 0; // Urlop zaległy z poprzedniego roku
    }
}
