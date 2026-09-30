using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces
{
    public interface IContactService
    {
        Task<int> CreateAsync(ContactMessage contactMessage);

        Task<List<ContactMessage>> GetAllAsync();

        Task<ContactMessage?> GetByIdAsync(int id);

        Task<bool> MarkAsReadAsync(int id);

        Task<bool> DeleteAsync(int id);
    }
}