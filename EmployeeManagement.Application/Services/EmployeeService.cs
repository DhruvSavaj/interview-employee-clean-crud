using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Doamin.Entities;
using EmployeeManagement.Doamin.Repository;

namespace EmployeeManagement.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<EmployeeDto?> GetByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            return employee is null ? null : ToDto(employee);
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            return employees.Select(ToDto);
        }

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
        {
            var employee = new Employee
            {
                Name = dto.Name,
                Email = dto.Email,
                Salary = dto.Salary,
                HireDate = dto.HireDate,
                IsActive = true,
            };

            employee.Id = await _employeeRepository.AddAsync(employee);
            return ToDto(employee);
        }
        public async Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var existing = await _employeeRepository.GetByIdAsync(id);
            if (existing is null) return false;

            existing.Salary = dto.Salary;
            existing.IsActive = dto.IsActive;

            return await _employeeRepository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _employeeRepository.DeleteAsync(id);
        }

        private static EmployeeDto ToDto(Employee e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            Salary = e.Salary,
            HireDate = e.HireDate,
            IsActive = e.IsActive,
        };
    }
}
