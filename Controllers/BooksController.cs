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

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] BookStatus? status)
    {
        var query = _context.Books.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        return await query.ToListAsync();
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { message = $"Boken med ID {id} hittades inte." });
        }

        return book;
    }

    
    [HttpPost]
    public async Task<ActionResult<Book>> CreateBook(Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, Book book)
    {
        if (id != book.Id)
        {
            return BadRequest(new { message = "ID i URL:en matchar inte boken i body." });
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
                return NotFound(new { message = $"Boken med ID {id} hittades inte." });
            }
            throw;
        }

        return NoContent();
    }

    
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatusAndRating(int id, [FromQuery] BookStatus status, [FromQuery] int? rating)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { message = $"Boken med ID {id} hittades inte." });
        }

        book.Status = status;
        if (rating.HasValue)
        {
            book.Rating = rating.Value;
        }

        await _context.SaveChangesAsync();

        return Ok(book);
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { message = $"Boken med ID {id} hittades inte." });
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