namespace Day03.Domain
{
    
        public class MemberPhone
        {
            public int Id { get; set; }
            public int MemberId { get; set; }
            public string PhoneNumber { get; set; } 
            public Member Member { get; set; }
           }
}
