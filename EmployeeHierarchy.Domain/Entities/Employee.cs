namespace EmployeeHierarchy.Domain.Entities;

 public class Employee
    {
        public int EmployeeId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int PositionId { get; set; }

        public int? ManagerEmployeeId { get; set; }

        public bool CreateUser { get; set; } = false;

        public string? Username { get; set; }

        public DateTime CreateTime { get; set; }
    }