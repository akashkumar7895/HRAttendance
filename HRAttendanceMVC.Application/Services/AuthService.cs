using HRAttendanceMVC.Application.DTOs;
using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Domain.Entities;
using HRAttendanceMVC.Infrastructure.Interfaces;


namespace HRAttendanceMVC.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> LoginAsync(LoginDto loginDto)
        {
            return await _userRepository.GetByEmailAndPasswordAsync(
                loginDto.Email,
                loginDto.Password
            );
        }

        public async Task<(bool IsSuccess, string Message)> SignupAsync(
            SignupDto signupDto)
        {
            if (await _userRepository.UserExistsAsync(signupDto.Email))
            {
                return (false, "This email is already registered.");
            }

            // Password ko BCrypt se hash karo
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(
                signupDto.Password
            );

            var newUser = new User
            {
                Name = signupDto.Name,
                Email = signupDto.Email,
                PasswordHash = passwordHash,
                Role = string.IsNullOrWhiteSpace(signupDto.Role)
                    ? "HR"
                    : signupDto.Role
            };

            await _userRepository.AddUserAsync(newUser);

            return (true, "Account Successfully registered!");
        }
    }
}