using System.ComponentModel.DataAnnotations;

namespace PageTurner.Models;

public class Book
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Author { get; set; } = string.Empty;

    [StringLength(50)]
    public string Genre { get; set; } = string.Empty;

    [Range(1, 5)]
    public int? Rating { get; set; }

    public BookStatus Status { get; set; } = BookStatus.WantToRead;
}