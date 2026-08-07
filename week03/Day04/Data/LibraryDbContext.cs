using Day04.Domain;
using Microsoft.EntityFrameworkCore;

namespace Day04.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<MemberPhone> MemberPhones { get; set; }
        public DbSet<LendingRecord> LendingRecords { get; set; }
    }
}
