using SQLite;

namespace Projekt.Models
{
    [Table("Leave")]
    public class Leave
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int EmployeeId { get; set; }

        [Indexed]
        public int LeaveTypeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public double Days { get; set; }

        public int Year { get; set; }

        public int Status { get; set; } = 1;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }
}
