using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PageTurner.Data;
using PageTurner.Models;

namespace PageTurner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _context;

    public BooksController(AppDbContext context)
    {
        _context = context;
    }

    // 1. GET: api/books (Get all books with optional search, filtering, and sorting)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
        [FromQuery] BookStatus? status,
        [FromQuery] string? search,
        [FromQuery] string? sortBy)
    {
        var query = _context.Books.AsQueryable();

        // Filter by status if provided
        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        // Free text search on Title or Author
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(searchLower) || 
                                     b.Author.ToLower().Contains(searchLower));
        }

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "rating" => query.OrderByDescending(b => b.Rating),
            "title" => query.OrderBy(b => b.Title),
            "author" => query.OrderBy(b => b.Author),
            _ => query.OrderByDescending(b => b.Id) // Default: newest first
        };

        return await query.ToListAsync();
    }

    // 2. GET: api/books/stats (Get reading list statistics)
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var totalBooks = await _context.Books.CountAsync();
        var finishedBooks = await _context.Books.CountAsync(b => b.Status == BookStatus.Finished);
        var readingBooks = await _context.Books.CountAsync(b => b.Status == BookStatus.Reading);
        var wantToReadBooks = await _context.Books.CountAsync(b => b.Status == BookStatus.WantToRead);

        var averageRating = await _context.Books
            .Where(b => b.Rating.HasValue)
            .AverageAsync(b => (double?)b.Rating) ?? 0;

        return Ok(new
        {
            TotalBooks = totalBooks,
            Finished = finishedBooks,
            CurrentlyReading = readingBooks,
            WantToRead = wantToReadBooks,
            AverageRating = Math.Round(averageRating, 1)
        });
    }

    // 3. GET: api/books/5 (Get a specific book by ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { message = $"Book with ID {id} was not found." });
        }

        return book;
    }

    // 4. POST: api/books (Create a new book)
    [HttpPost]
    public async Task<ActionResult<Book>> CreateBook(Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    // 5. PUT: api/books/5 (Update an entire book)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, Book book)
    {
        if (id != book.Id)
        {
            return BadRequest(new { message = "ID in URL does not match book ID in body." });
        }

        _context.Entry(book).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookExists(id))
            {
                return NotFound(new { message = $"Book with ID {id} was not found." });
            }
            throw;
        }

        return NoContent();
    }

    // 6. PATCH: api/books/5/status (Convenient status/rating update)
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatusAndRating(int id, [FromQuery] BookStatus status, [FromQuery] int? rating)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { message = $"Book with ID {id} was not found." });
        }

        book.Status = status;
        if (rating.HasValue)
        {
            book.Rating = rating.Value;
        }

        await _context.SaveChangesAsync();

        return Ok(book);
    }

    // 7. DELETE: api/books/5 (Delete a book)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { message = $"Book with ID {id} was not found." });
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BookExists(int id)
    {
        return _context.Books.Any(e => e.Id == id);
    }
}