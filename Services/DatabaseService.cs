using Projekt.Models;
using SQLite;

namespace Projekt.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _db;
        private readonly string _dbPath;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public DatabaseService()
        {
            //_dbPath = Path.Combine(FileSystem.AppDataDirectory, "db.sqlite");
            _dbPath = "./db.sqlite";
        }

        private async Task Init()
        {
            if (_db is not null) return;

            await _semaphore.WaitAsync();
            try
            {
                if (_db is not null) return;

                _db = new SQLiteAsyncConnection(_dbPath);
                var conn = _db.GetConnection();
                conn.CreateTable<Employee>();
                conn.CreateTable<EmploymentType>();
                conn.CreateTable<LeaveType>();
                conn.CreateTable<LeaveLimit>();
                conn.CreateTable<Leave>();
                conn.CreateTable<Holiday>();

                await SeedInitialData();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SeedInitialData()
        {
            if (await _db.Table<EmploymentType>().CountAsync() == 0)
            {
                await _db.InsertAllAsync(new List<EmploymentType>
                {
                    new() { Name = "Umowa o pracę (26 dni)", LeaveLimit = 26, HasLeaveRights = true },
                    new() { Name = "Umowa o pracę (20 dni)", LeaveLimit = 20, HasLeaveRights = true },
                    new() { Name = "B2B / Brak urlopu", HasLeaveRights = false, LeaveLimit = 0 }
                });
            }

            if (await _db.Table<LeaveType>().CountAsync() == 0)
            {
                await _db.InsertAllAsync(new List<LeaveType>
                {
                    new() { Name = "Wypoczynkowy" },
                    new() { Name = "Chorobowy" },
                    new() { Name = "Na żądanie" }
                });
            }

            await SeedHolidays();
        }

        public async Task<List<Employee>> GetAllEmployees()
        {
            await Init();
            return await _db.Table<Employee>().OrderBy(e => e.LastName).ToListAsync();
        }

        public async Task<int> SaveEmployee(Employee employee)
        {
            await Init();

            if (employee.Id != 0)
                return await _db.UpdateAsync(employee);

            int result = await _db.InsertAsync(employee);
            await EnsureLeaveLimit(employee);
            return result;
        }

        private async Task EnsureLeaveLimit(Employee employee)
        {
            var type = await _db.Table<EmploymentType>().FirstOrDefaultAsync(t => t.Id == employee.EmploymentTypeId);
            if (type is not { HasLeaveRights: true }) return;

            int year = DateTime.Now.Year;
            var existing = await _db.Table<LeaveLimit>()
                .FirstOrDefaultAsync(l => l.EmployeeId == employee.Id && l.Year == year);

            if (existing == null)
            {
                await _db.InsertAsync(new LeaveLimit
                {
                    EmployeeId = employee.Id,
                    LeaveTypeId = 1,
                    Year = year,
                    DaysLimit = type.LeaveLimit ?? 0
                });
            }
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            await Init();
            await _db.RunInTransactionAsync(tran =>
            {
                tran.Table<Leave>().Where(l => l.EmployeeId == id).Delete();
                tran.Table<LeaveLimit>().Where(l => l.EmployeeId == id).Delete();
                tran.Table<Employee>().Where(e => e.Id == id).Delete();
            });
        }

        public async Task<int> GetEmployeesOnLeaveTodayCount()
        {
            await Init();
            var today = DateTime.Today;
            return await _db.Table<Leave>()
                .CountAsync(l => l.StartDate <= today && l.EndDate >= today && l.Status == 2);
        }

        public async Task<List<Leave>> GetEmployeeLeaves(int employeeId)
        {
            await Init();
            return await _db.Table<Leave>()
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();
        }

        public async Task SaveLeave(Leave leave)
        {
            await Init();
            leave.Status = 2;
            await _db.InsertAsync(leave);
        }

        public async Task DeleteLeaveAsync(int id)
        {
            await Init();
            await _db.Table<Leave>().Where(l => l.Id == id).DeleteAsync();
        }

        public async Task<double> GetRemainingLeaveDays(int employeeId, int year)
        {
            await Init();

            var limit = await _db.Table<LeaveLimit>()
                .FirstOrDefaultAsync(l => l.EmployeeId == employeeId && l.Year == year);

            if (limit == null) return 0;

            var leaves = await _db.Table<Leave>()
                .Where(l => l.EmployeeId == employeeId && l.Year == year && l.Status == 2)
                .ToListAsync();

            return (limit.DaysLimit + limit.CarryOverDays) - leaves.Sum(l => l.Days);
        }

        public async Task<List<EmploymentType>> GetEmploymentTypes()
        {
            await Init();
            return await _db.Table<EmploymentType>().ToListAsync();
        }

        public async Task<double> CalculateWorkingDays(DateTime start, DateTime end)
        {
            await Init();
            var holidays = (await _db.Table<Holiday>().ToListAsync())
                .Select(h => h.HolidayDate.Date).ToHashSet();

            int count = 0;
            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday && !holidays.Contains(d))
                    count++;
            }
            return count;
        }

        private async Task SeedHolidays()
        {
            if (await _db.Table<Holiday>().CountAsync() > 0) return;

            int y = DateTime.Now.Year;
            await _db.InsertAllAsync(new List<Holiday>
            {
                new() { HolidayName = "Nowy Rok", HolidayDate = new DateTime(y, 1, 1) },
                new() { HolidayName = "Trzech Króli", HolidayDate = new DateTime(y, 1, 6) },
                new() { HolidayName = "Majówka", HolidayDate = new DateTime(y, 5, 1) },
                new() { HolidayName = "Konstytucja 3 Maja", HolidayDate = new DateTime(y, 5, 3) },
                new() { HolidayName = "Wniebowzięcie", HolidayDate = new DateTime(y, 8, 15) },
                new() { HolidayName = "Wszystkich Świętych", HolidayDate = new DateTime(y, 11, 1) },
                new() { HolidayName = "Niepodległości", HolidayDate = new DateTime(y, 11, 11) },
                new() { HolidayName = "Boże Narodzenie", HolidayDate = new DateTime(y, 12, 25) },
                new() { HolidayName = "Boże Narodzenie d.2", HolidayDate = new DateTime(y, 12, 26) }
            });
        }
    }
}