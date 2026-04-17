using Projekt.Models;
using Projekt.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace Projekt.ViewModels
{
    public class EmployeeViewModel : BindableObject
    {
        private readonly DatabaseService _dbService;
        public ObservableCollection<Employee> Employees { get; set; } = new();

        public ICommand AddEmployeeCommand { get; }

        public EmployeeViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            AddEmployeeCommand = new Command(async () => await AddRandomEmployee());
            _ = LoadEmployees();
        }

        public async Task LoadEmployees()
        {
            var list = await _dbService.GetAllEmployees();
            Employees.Clear();
            foreach (var emp in list) Employees.Add(emp);
        }

        private async Task AddRandomEmployee()
        {
            var newEmp = new Employee
            {
                FirstName = "Jan",
                LastName = "Kowalski",
                ReferenceNumber = Guid.NewGuid().ToString().Substring(0, 8),
                EmploymentDate = DateTime.Now
            };

            await _dbService.SaveEmployee(newEmp);
            Employees.Add(newEmp);
        }
    }
}
