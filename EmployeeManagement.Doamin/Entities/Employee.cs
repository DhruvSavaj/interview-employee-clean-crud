using EmployeeManagement.Doamin.Common;

namespace EmployeeManagement.Doamin.Entities
{
    public sealed class Employee : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
    }
}
