namespace Alabaster.Models
{
    public class GalleryImage
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; } // Path to the image
        public DateTime UploadedDate { get; set; } = DateTime.Now;
    }
}