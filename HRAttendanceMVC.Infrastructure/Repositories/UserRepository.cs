using HRAttendanceMVC.Domain.Entities;
using HRAttendanceMVC.Infrastructure.Data;
using HRAttendanceMVC.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace HRAttendanceMVC.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAndPasswordAsync(
            string email,
            string password)
        {
            // Email se user find karo
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return null;
            }

            // BCrypt password verify
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                password,
                user.PasswordHash
            );

            if (!isPasswordValid)
            {
                return null;
            }

            return user;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}