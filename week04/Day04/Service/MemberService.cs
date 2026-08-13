using Day03.Controllers;
using Day03.Data;
using Day03.Domain;
using Day03.DTO;
using Microsoft.EntityFrameworkCore;

namespace Day03.Services
{
    public class MemberService : IMemberService
    {
        private readonly LibraryDbContext _context;

        public MemberService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Member>> GetAllMembersAsync()
        {
            return await _context.Members
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Member?> GetMemberByIdAsync(int id)
        {
            return await _context.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Member> CreateMemberAsync(CreateMemberRequest request)
        {
            var member = new Member
            {
                FullName = request.FullName,
                Email = request.Email
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<Member?> UpdateMemberAsync(int id, UpdateMemberRequest request)
        {
            var existingMember = await _context.Members.FindAsync(id);
            if (existingMember == null) return null;

            existingMember.FullName = request.FullName;
            existingMember.Email = request.Email;

            await _context.SaveChangesAsync();
            return existingMember;
        }

        public async Task<bool> DeleteMemberAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return false;
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}