using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.EmployeeCode);

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.HrUserId);

        modelBuilder.Entity<LeaveType>()
            .HasIndex(x => x.Name);

        modelBuilder.Entity<LeaveType>()
            .HasIndex(x => x.HrUserId);

        modelBuilder.Entity<Attendance>()
            .HasIndex(x => new
            {
                x.EmployeeId,
                x.AttendanceDate
            })
            .IsUnique();

        modelBuilder.Entity<LeaveRequest>()
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LeaveRequest>()
            .HasOne(x => x.LeaveType)
            .WithMany()
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // User configuration
        modelBuilder.Entity<User>()
            .Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(x => x.Email)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(x => x.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(x => x.Role)
            .HasMaxLength(50)
            .IsRequired();

        // Emails will not be allowed in the evening.
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Phone)
                .HasMaxLength(20);

            entity.Property(x => x.Subject)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Message)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            
        });

        // Payroll configuration
        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.EmployeeId, x.Month, x.Year })
                .IsUnique();

            entity.Property(x => x.BasicSalary)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.HRA)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.OtherAllowances)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Deductions)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Remarks)
                .HasMaxLength(500);

            // NetSalary and MonthName are computed C# properties – not stored in DB
            entity.Ignore(x => x.NetSalary);
            entity.Ignore(x => x.MonthName);
        });

    }


}