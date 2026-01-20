using System.ComponentModel.DataAnnotations;

namespace SosyalAjandam.Models
{
    public enum ModuleType
    {
        Wishlist,
        VisionBoard,
        Movie,
        Book
    }

    public class LifeItem
    {
        public int Id { get; set; }
        
        [Required]
        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }

        public ModuleType Type { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty; // Author, Director, Link, ImageURL

        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
