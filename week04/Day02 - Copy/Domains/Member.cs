namespace Day03.Domain
{
   


    public class Member
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; } 

        public ICollection<MemberPhone> MemberPhones { get; set; } = new List<MemberPhone>();
        public ICollection<LendingRecord> LendingRecords { get; set; } = new List<LendingRecord>();
    }
}
