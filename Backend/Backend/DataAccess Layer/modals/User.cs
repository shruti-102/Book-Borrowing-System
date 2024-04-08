namespace Backend.DataAccess_Layer.modals
{
    public class User
    {
        public int Id { get; set; }
        public string ?Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int TokensAvailable { get; set; }

        // Navigation properties for relationships
        //public ICollection<Book> BooksBorrowed { get; set; }
        //public ICollection<Book> BooksLent { get; set; }
    }
}
