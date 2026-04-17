using SQLite;

namespace Projekt.Models
{
    [Table("EmploymentType")]
    public class EmploymentType
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        public bool HasLeaveRights { get; set; } = true;

        public int? LeaveLimit { get; set; }
    }
}
