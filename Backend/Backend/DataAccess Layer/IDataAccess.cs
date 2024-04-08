using Backend.DataAccess_Layer.modals;

namespace Backend.DataAccess_Layer
{
    public interface IDataAccess
    {
        List<Book> GetBooks();

        Book GetBookById(int id);

        string IsUserPresent(string username, string password);

        void AddBook(Book book);
    }
}
