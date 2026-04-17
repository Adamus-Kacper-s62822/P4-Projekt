using SQLite;

namespace Projekt.Models
{
    [Table("LeaveType")]
    public class LeaveType
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
