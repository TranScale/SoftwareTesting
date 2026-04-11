using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2,
    Leave = 3
}

public class Attendance
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    public string? Note { get; set; }
}