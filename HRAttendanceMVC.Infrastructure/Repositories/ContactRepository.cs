using HRAttendanceMVC.Application.Interfaces;
using HRAttendanceMVC.Domain.Enities;
using HRAttendanceMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRAttendanceMVC.Infrastructure.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly AppDbContext _context;

        public ContactRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(ContactMessage contact)
        {
            _context.ContactMessages.Add(contact);

            await _context.SaveChangesAsync();

            return contact.Id;
        }

        public async Task<List<ContactMessage>> GetAllAsync()
        {
            return await _context.ContactMessages
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<ContactMessage?> GetByIdAsync(int id)
        {
            return await _context.ContactMessages
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var contact = await _context.ContactMessages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (contact == null)
                return false;

            

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var contact = await _context.ContactMessages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (contact == null)
                return false;

            _context.ContactMessages.Remove(contact);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}