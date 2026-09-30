using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Domain.Enities;

namespace HRAttendanceMVC.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;

        public ContactService(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public async Task<int> CreateAsync(ContactMessage contactMessage)
        {
            contactMessage.CreatedAt = DateTime.Now;
            

            return await _contactRepository.AddAsync(contactMessage);
        }

        public async Task<List<ContactMessage>> GetAllAsync()
        {
            return await _contactRepository.GetAllAsync();
        }

        public async Task<ContactMessage?> GetByIdAsync(int id)
        {
            return await _contactRepository.GetByIdAsync(id);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            return await _contactRepository.MarkAsReadAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _contactRepository.DeleteAsync(id);
        }
    }
}