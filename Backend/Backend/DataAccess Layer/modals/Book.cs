namespace Backend.DataAccess_Layer.modals
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Rating { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public int IsBookAvailable { get; set; }
        public string Description { get; set; }

        // Foreign key references
       // public int? LentByUserId { get; set; }
        public int? CurrentlyBorrowedByUserId { get; set; }

        // Navigation properties for relationships
        public User? LentByUser { get; set; }
        public User? CurrentlyBorrowedByUser { get; set; }

        public int AddedBy { get; set; }
    }
}
