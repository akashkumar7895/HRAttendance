using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Interfaces
{
    public interface IContactRepository
    {
        Task<int> AddAsync(ContactMessage contact);

        Task<List<ContactMessage>> GetAllAsync();

        Task<ContactMessage?> GetByIdAsync(int id);

        Task<bool> MarkAsReadAsync(int id);

        Task<bool> DeleteAsync(int id);
    }
}