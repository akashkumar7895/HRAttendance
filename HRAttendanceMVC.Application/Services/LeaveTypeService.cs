using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Interfaces;
using HRAttendanceMVC.Application.Interfaces;

namespace HRAttendanceMVC.Application.Services;

public class LeaveTypeService : ILeaveTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public LeaveTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LeaveType>> GetAllLeaveTypesAsync()
    {
        return await _unitOfWork.LeaveTypes.GetAllOrderedByNameAsync();
    }

    public async Task<LeaveType?> GetLeaveTypeByIdAsync(int id)
    {
        return await _unitOfWork.LeaveTypes.GetByIdAsync(id);
    }

    public async Task<ServiceResult<LeaveType>> CreateLeaveTypeAsync(LeaveType leaveType)
    {
        if (string.IsNullOrWhiteSpace(leaveType.Name))
        {
            return ServiceResult<LeaveType>.Failed("Leave type name is required.", nameof(leaveType.Name));
        }

        var isUnique = await _unitOfWork.LeaveTypes.IsNameUniqueAsync(leaveType.Name.Trim());
        if (!isUnique)
        {
            return ServiceResult<LeaveType>.Failed("Leave type already exists.", nameof(leaveType.Name));
        }

        leaveType.Name = leaveType.Name.Trim();
        await _unitOfWork.LeaveTypes.AddAsync(leaveType);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<LeaveType>.Ok(leaveType, "Leave type created successfully.");
    }

    public async Task<ServiceResult<LeaveType>> UpdateLeaveTypeAsync(int id, LeaveType leaveType)
    {
        if (id != leaveType.Id)
        {
            return ServiceResult<LeaveType>.Failed("Leave type ID mismatch.");
        }

        var existing = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResult<LeaveType>.Failed("Leave type not found.");
        }

        var isUnique = await _unitOfWork.LeaveTypes.IsNameUniqueAsync(leaveType.Name.Trim(), id);
        if (!isUnique)
        {
            return ServiceResult<LeaveType>.Failed("Leave type already exists.", nameof(leaveType.Name));
        }

        existing.Name = leaveType.Name.Trim();
        existing.TotalDays = leaveType.TotalDays;

        _unitOfWork.LeaveTypes.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<LeaveType>.Ok(existing, "Leave type updated successfully.");
    }

    public async Task<ServiceResult> DeleteLeaveTypeAsync(int id)
    {
        var existing = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResult.Failed("Leave type not found.");
        }

        var hasRequests = await _unitOfWork.LeaveTypes.HasLeaveRequestsAsync(id);
        if (hasRequests)
        {
            return ServiceResult.Failed("Leave type cannot be deleted because leave requests exist.");
        }

        _unitOfWork.LeaveTypes.Delete(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok("Leave type deleted successfully.");
    }

    public async Task<int> GetTotalLeaveTypesCountAsync()
    {
        return await _unitOfWork.LeaveTypes.CountAsync();
    }
}
