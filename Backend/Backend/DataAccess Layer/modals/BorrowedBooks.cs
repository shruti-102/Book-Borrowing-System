namespace Backend.DataAccess_Layer.modals
{
    public class BorrowedBooks
    {
        public int BorrowId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }

        // Navigation properties for relationships
        public User User { get; set; }
        public Book Book { get; set; }
    }
}
