using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Repositories;

public class PayrollRepository : Repository<Payroll>, IPayrollRepository
{
    public PayrollRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Payroll>> GetAllWithEmployeeAsync(int? hrUserId = null)
    {
        var query = DbSet.Include(p => p.Employee).AsQueryable();

        if (hrUserId.HasValue)
            query = query.Where(p => p.HrUserId == hrUserId.Value || (p.Employee != null && p.Employee.HrUserId == hrUserId.Value));

        return await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync();
    }

    public async Task<Payroll?> GetByIdWithEmployeeAsync(int id)
    {
        return await DbSet
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IReadOnlyList<Payroll>> GetByEmployeeAsync(int employeeId)
    {
        return await DbSet
            .Include(p => p.Employee)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync();
    }

    public async Task<bool> ExistsForEmployeeAndMonthAsync(int employeeId, int month, int year, int? excludeId = null)
    {
        return await DbSet.AnyAsync(p =>
            p.EmployeeId == employeeId &&
            p.Month == month &&
            p.Year == year &&
            (excludeId == null || p.Id != excludeId.Value));
    }
}
