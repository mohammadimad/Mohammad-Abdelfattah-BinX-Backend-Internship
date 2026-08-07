namespace Day03.Domain
{
    public class LendingRecord
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime LendDate { get; set; } 
        public DateTime? ReturnDate { get; set; } 

        public Book Book { get; set; }
        public Member Member { get; set; }
    }
}
