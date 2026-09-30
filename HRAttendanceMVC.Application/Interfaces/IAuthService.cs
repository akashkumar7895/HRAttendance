using HRAttendanceMVC.Application.DTOs;
using HRAttendanceMVC.Domain.Entities;

namespace HRAttendanceMVC.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(LoginDto loginDto);
        Task<(bool IsSuccess, string Message)> SignupAsync(SignupDto signupDto);
    }
}