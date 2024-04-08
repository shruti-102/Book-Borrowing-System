using Backend.DataAccess_Layer;
using Backend.DataAccess_Layer.modals;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Presentation_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookBorrowingController : ControllerBase
    {
        readonly IDataAccess _dataAccess;

        public BookBorrowingController(IDataAccess dataAccess,IConfiguration configuration)
        {
            this._dataAccess = dataAccess;
        }

        [HttpGet("GetBooks")]
        public IActionResult GetBooks() {
            var result = _dataAccess.GetBooks();
            return Ok(result);
        }

        [HttpGet("id")]
        public ActionResult<Book> GetBookById(int id)
        {
            try
            {
                var book = _dataAccess.GetBookById(id);

                if (book != null)
                {
                    return Ok(book);
                }
                else
                {
                    return NotFound(); // If the book with the specified ID is not found
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("LoginUser")]
        public IActionResult LoginUser([FromBody] User user)
        {
            Console.WriteLine($"Received user data: {user.Username}, {user.Password}");
            var token = _dataAccess.IsUserPresent(user.Username, user.Password);
            if (token == "") token = "invalid";
            return Ok(token);
        }

        [HttpPost("AddBook")]
        public IActionResult AddBook([FromBody] Book book)
        {
            try
            {
                var addedBook=_dataAccess.AddBook(book);

                if (addedBook != null)
                {
                    var user = _dataAccess.GetUserByUsername(addedBook.Username);
                    if (user != null)
                    {
                        user.TokensAvailable += 1;
                        _dataAccess.UpdateUser(user);
                    }

                    return Ok(addedBook);
                }
                else
                {
                    return BadRequest("Failed to add the book");
                }
                //return Ok(book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error:{ex.Message}");
            }
        }

        [HttpPost("BorrowBook")]
        public IActionResult BorrowBook([FromBody] BorrowBookRequest borrowRequest)
        {
            try
            {
                var book = _dataAccess.GetBookById(borrowRequest.BookId);
                var borrower = _dataAccess.GetUserByUsername(borrowRequest.BorrowerUsername);

                if (book != null && borrower != null && book.OwnerId != borrower.Id)
                {
                    if (borrower.TokensAvailable > 0)
                    {
                        // Deduct token from the borrower
                        borrower.TokensAvailable -= 1;

                        // Update the book's owner to the borrower
                        book.OwnerId = borrower.Id;

                        // Update the book in the database
                        _dataAccess.UpdateBook(book);

                        // Update the borrower's tokens in the database
                        _dataAccess.UpdateUser(borrower);

                        return Ok(new { message = "Book borrowed successfully" });
                    }
                    else
                    {
                        return StatusCode(403, new { message = "Not enough tokens to borrow the book" });
                    }
                }
                else
                {
                    return BadRequest("Invalid request");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
