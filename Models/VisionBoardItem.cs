using System;

namespace SosyalAjandam.Models
{
    public class VisionBoardItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        
        // "image", "sticker", "note", "tape"
        public string Type { get; set; } = "image";
        
        // Image URL or Text Content
        public string Content { get; set; } = string.Empty;
        
        // Style properties (stored as strings to be flexible or specific types)
        // Storing as double for interact.js compatibility
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }
        
        // Extra style data (e.g., color for notes)
        public string StyleData { get; set; } = string.Empty; 
    }
}
