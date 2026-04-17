using SQLite;

namespace Projekt.Models
{
    [Table("Holiday")]
    public class Holiday
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string HolidayName { get; set; }

        [Unique]
        public DateTime HolidayDate { get; set; }
    }
}
