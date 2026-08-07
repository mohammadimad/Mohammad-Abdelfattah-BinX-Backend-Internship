using Day03.Domain;
using Day03.Domains.DTO;

public interface IMemberService
{
    Task<IEnumerable<Member>> GetAllMembersAsync();
    Task<Member?> GetMemberByIdAsync(int id);

    Task<Member> CreateMemberAsync(CreateMemberRequest request);

    Task<Member?> UpdateMemberAsync(int id, UpdateMemberRequest request);

    Task<bool> DeleteMemberAsync(int id);
}