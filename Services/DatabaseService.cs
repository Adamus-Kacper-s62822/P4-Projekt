using Projekt.Models;
using SQLite;
//using Microsoft.Maui.Storage;

namespace Projekt.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _db;
        private readonly string _dbPath;

        //public DatabaseService() => _dbPath = Path.Combine(FileSystem.AppDataDirectory, "db.db3");
        public DatabaseService() => _dbPath = Path.Combine("./db.sqlite");

        private async Task Init()
        {
            if (_db is not null) return;

            _db = new SQLiteAsyncConnection(_dbPath);

            // Automatyczne tworzenie tabel na podstawie klas
            await _db.CreateTableAsync<Employee>();
            await _db.CreateTableAsync<EmploymentType>();
            await _db.CreateTableAsync<LeaveType>();
            await _db.CreateTableAsync<LeaveLimit>();
            await _db.CreateTableAsync<Leave>();
            await _db.CreateTableAsync<Holiday>();

            if (await _db.Table<EmploymentType>().CountAsync() == 0)
            {
                await _db.InsertAsync(new EmploymentType { Name = "Umowa o pracę (26 dni)", LeaveLimit = 26 });
                await _db.InsertAsync(new EmploymentType { Name = "Umowa o pracę (20 dni)", LeaveLimit = 20 });
                await _db.InsertAsync(new EmploymentType { Name = "B2B / Brak urlopu", HasLeaveRights = false });
            }

            if (await _db.Table<LeaveType>().CountAsync() == 0)
            {
                await _db.InsertAsync(new LeaveType { Name = "Wypoczynkowy" });
                await _db.InsertAsync(new LeaveType { Name = "Chorobowy" });
                await _db.InsertAsync(new LeaveType { Name = "Na żądanie" });
            }
        }

        // --- METODY DLA PRACOWNIKA ---

        public async Task<List<Employee>> GetAllEmployees()
        {
            await Init();
            return await _db.Table<Employee>().ToListAsync();
        }

        public async Task<int> SaveEmployee(Employee employee)
        {
            await Init();
            int result;

            if (employee.Id != 0)
            {
                result = await _db.UpdateAsync(employee);
            }
            else
            {
                result = await _db.InsertAsync(employee);

                var empType = await _db.Table<EmploymentType>()
                                       .FirstOrDefaultAsync(t => t.Id == employee.EmploymentTypeId);

                if (empType != null && empType.HasLeaveRights)
                {
                    await _db.InsertAsync(new LeaveLimit
                    {
                        EmployeeId = employee.Id,
                        LeaveTypeId = 1,
                        Year = DateTime.Now.Year,
                        DaysLimit = empType.LeaveLimit ?? 0
                    });
                }
            }
            return result;
        }

        public async Task<int> DeleteEmployee(Employee employee)
        {
            await Init();
            return await _db.DeleteAsync(employee);
        }

        public async Task<List<EmployeeDisplay>> GetEmployeesForDisplay()
        {
            await Init();
            var employees = await _db.Table<Employee>().ToListAsync();
            var types = await _db.Table<EmploymentType>().ToListAsync();

            var displayList = new List<EmployeeDisplay>();

            foreach (var emp in employees)
            {
                var typeName = types.FirstOrDefault(t => t.Id == emp.EmploymentTypeId)?.Name ?? "Nieznany";
                var remaining = await GetRemainingLeaveDays(emp.Id, DateTime.Now.Year);

                displayList.Add(new EmployeeDisplay
                {
                    Employee = emp,
                    EmploymentTypeName = typeName,
                    RemainingDays = remaining
                });
            }

            return displayList;
        }

        public async Task<List<Leave>> GetEmployeeLeaves(int employeeId)
        {
            await Init();
            return await _db.Table<Leave>()
                            .Where(l => l.EmployeeId == employeeId)
                            .OrderByDescending(l => l.StartDate)
                            .ToListAsync();
        }

        // --- METODY DLA TYPÓW ZATRUDNIENIA ---

        public async Task<List<EmploymentType>> GetEmploymentTypes()
        {
            await Init();
            return await _db.Table<EmploymentType>().ToListAsync();
        }

        // --- METODY DLA LIMITÓW URLOPU PRACOWNIKA ---

        public async Task CreateLeaveLimitForEmployee(Employee emp, int year)
        {
            await Init();

            var empType = await _db.Table<EmploymentType>()
                                   .Where(t => t.Id == emp.EmploymentTypeId)
                                   .FirstOrDefaultAsync();

            if (empType != null && empType.HasLeaveRights)
            {
                var limit = new LeaveLimit
                {
                    EmployeeId = emp.Id,
                    LeaveTypeId = 1,
                    Year = year,
                    DaysLimit = empType.LeaveLimit ?? 0,
                    CarryOverDays = 0
                };
                await _db.InsertAsync(limit);
            }
        }

        // --- METODY POMOCNICZE ---

        public async Task<double> CalculateWorkingDays(DateTime start, DateTime end)
        {
            await Init();

            var holidays = await _db.Table<Holiday>().ToListAsync();
            var holidayDates = holidays.Select(h => h.HolidayDate.Date).ToList();

            double days = 0;
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday &&
                    date.DayOfWeek != DayOfWeek.Sunday &&
                    !holidayDates.Contains(date))
                {
                    days++;
                }
            }
            return days;
        }

        public async Task<double> GetRemainingLeaveDays(int employeeId, int year)
        {
            await Init();
            var limitRecord = await _db.Table<LeaveLimit>()
                                       .Where(l => l.EmployeeId == employeeId && l.Year == year)
                                       .FirstOrDefaultAsync();

            if (limitRecord == null) return 0;


            var approvedLeaves = await _db.Table<Leave>()
                                          .Where(l => l.EmployeeId == employeeId
                                                   && l.Year == year
                                                   && l.Status == 2)
                                          .ToListAsync();

            double usedDays = approvedLeaves.Sum(l => l.Days);
            return (limitRecord.DaysLimit + limitRecord.CarryOverDays) - usedDays;
        }

        public async Task SaveLeave(Leave leave)
        {
            await Init();
            await _db.InsertAsync(leave);
        }
    }
}
