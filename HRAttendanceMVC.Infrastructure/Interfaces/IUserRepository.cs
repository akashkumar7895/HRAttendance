using HRAttendanceMVC.Domain.Entities;

namespace HRAttendanceMVC.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);
        Task<bool> UserExistsAsync(string email);
        Task AddUserAsync(User user);
    }
}