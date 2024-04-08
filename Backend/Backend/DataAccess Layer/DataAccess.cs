using Backend.DataAccess_Layer.modals;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.DataAccess_Layer
{
    public class DataAccess : IDataAccess
    {
        private readonly IConfiguration configuration;
        private readonly string dbconnection;

        public DataAccess(IConfiguration configuration)
        {
            this.configuration = configuration;
            dbconnection = this.configuration["ConnectionStrings:DB"];
        }

        public List<Book> GetBooks()
        {
            var booksAvailable = new List<Book>();

            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                using (SqlCommand command = new SqlCommand("SELECT * FROM Books WHERE Is_Available = 1", connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var book = new Book
                            {
                                Id = (int)reader["BookID"],
                                Name = (string)reader["Name"],
                                Rating = (decimal)reader["Rating"],
                                Author = (string)reader["Author"],
                                Genre = (string)reader["Genre"],
                                IsBookAvailable = (int)reader["Is_Available"],
                                Description = (string)reader["Description"],
                                //LentByUserId = reader["LentBy_ID"] as int?,
                                CurrentlyBorrowedByUserId = reader["BorrowedBy_ID"] as int?,
                                AddedBy = (int)reader["Added_By"]
                            };

                            booksAvailable.Add(book);
                        }
                    }
                }
            }

            return booksAvailable;
        }

        public Book GetBookById(int id)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();

                string query = "SELECT * FROM Books WHERE BookID=@id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                //connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var book = new Book
                    {
                        Id = (int)reader["BookID"],
                        Name = (string)reader["Name"],
                        Rating = (decimal)reader["Rating"],
                        Author = (string)reader["Author"],
                        Genre = (string)reader["Genre"],
                        IsBookAvailable = (int)reader["Is_Available"],
                        Description = (string)reader["Description"],
                        //LentByUserId = reader["LentBy_ID"] as int?,
                        CurrentlyBorrowedByUserId = reader["BorrowedBy_ID"] as int?,
                        AddedBy = (int)reader["Added_By"]
                    };
                    return book;
                }
                return null;
            }
            
        }
        

        public List<Book> GetBorrowedBooks()
        {
            var bookBorrowed= new List<Book>();

            return bookBorrowed;
        }

        public string IsUserPresent(string username, string password)
        {
            User user = new();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection,
                };

                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username='" + username + "' AND Password='" + password + "';";
                command.CommandText = query;
                int count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    connection.Close();
                    return "";
                }
                query = "Select * FROM Users WHERE Username='" + username + "'AND Password='" + password + "';";
                command.CommandText = query;

                SqlDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    user.Id = (int)reader["UserID"];
                    user.Name= (string)reader["Name"];
                    user.Username = (string)reader["Username"];
                    user.Password= (string)reader["Password"];
                    user.TokensAvailable = (int)reader["Tokens_Available"];
                }

                string key = "MNU66iBl3T5rh6H52i69abcdefghijklmnopqrstuvwx";

                string duration = "60";
                var symmetrickey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(symmetrickey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim("Id",user.Id.ToString()),
                    new Claim("Name",user.Name),
                    new Claim("Username",user.Username),
                    new Claim("Password",user.Password),
                    new Claim("Tokens_Available",user.TokensAvailable.ToString())
                };

                var jwtToken = new JwtSecurityToken(
                    issuer: "localhost",
                    audience: "localhost",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Int32.Parse(duration)),
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(jwtToken);
                
            }
            return "";
        }

        public void AddBook(Book book)
        {
            using(SqlConnection connection=new SqlConnection(dbconnection))
            {
                connection.Open();

                string query= "INSERT INTO Books (Name, Rating, Author, Genre, Is_Available, Description, Added_By) VALUES (@Name, @Rating, @Author, @Genre, @Is_Available, @Description, @Added_By)";
                SqlCommand command=new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Name", book.Name);
                command.Parameters.AddWithValue("@Rating", book.Rating);
                command.Parameters.AddWithValue("@Author", book.Author);
                command.Parameters.AddWithValue("@Genre", book.Genre);
                command.Parameters.AddWithValue("@Is_Available", book.IsBookAvailable);
                command.Parameters.AddWithValue("@Description", book.Description);
                command.Parameters.AddWithValue("@Added_By", book.AddedBy);

                command.ExecuteNonQuery();
            }
        }

        public User GetUserById(int userId)
        {
            // Assuming you have a DbSet<User> in your DbContext named 'Users'
            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);

            return user;
        }

        public bool BorrowBook(int bookId, int userId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();

                var book = GetBookById(bookId);
                var borrower = GetUserById(userId);

                if (book != null && borrower != null && book.OwnerId != borrower.Id && book.IsBookAvailable == 1)
                {
                    if (borrower.TokensAvailable > 0)
                    {
                        // Deduct token from the borrower
                        borrower.TokensAvailable -= 1;

                        // Update the book's owner to the borrower
                        book.AddedBy = borrower.Id;

                        // Set the book as not available
                        book.IsBookAvailable = 0;

                        // Update the book in the database
                        UpdateBook(book);

                        // Update the borrower's tokens in the database
                        UpdateUser(borrower);

                        return true;
                    }
                }
                return false;
            }
        }

        public bool ReturnBook(int bookId, int userId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();

                var book = GetBookById(bookId);
                var borrower = GetUserById(userId);

                if (book != null && borrower != null && book.OwnerId == borrower.Id && book.IsBookAvailable == 0)
                {
                    // Increment token for the user who is returning the book
                    borrower.TokensAvailable += 1;

                    // Update the book's owner to null (book is now available)
                    book.OwnerId = null;

                    // Set the book as available
                    book.IsBookAvailable = 1;

                    // Update the book in the database
                    UpdateBook(book);

                    // Update the borrower's tokens in the database
                    UpdateUser(borrower);

                    return true;
                }
                return false;
            }
        }
    }
}
