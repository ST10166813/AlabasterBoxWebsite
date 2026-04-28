using Microsoft.AspNetCore.Mvc;
using Alabaster.Models;

namespace Alabaster.Controllers
{
    public class GalleryController : Controller
    {
        public IActionResult Index()
        {
            // Dummy data - replace with database logic later
            var images = new List<GalleryImage>
            {
                new GalleryImage { Id = 1, Title = "Church Service", ImageUrl = "/images/three.jpeg" },
                new GalleryImage { Id = 2, Title = "Outreach Program", ImageUrl = "/images/four.jpeg" },
                new GalleryImage { Id = 3, Title = "Prayer Meeting", ImageUrl = "/images/imgfour.jpeg" },
                new GalleryImage { Id = 4, Title = "Prayer Meeting", ImageUrl = "/images/five.jpeg" }, 
                new GalleryImage { Id = 5, Title = "Prayer Meeting", ImageUrl = "/images/four.jpeg" },
                new GalleryImage { Id = 6, Title = "Prayer Meeting", ImageUrl = "/images/imgfive.jpeg" },   
                new GalleryImage { Id = 7, Title = "Prayer Meeting", ImageUrl = "/images/open6.jpeg" },   
                new GalleryImage { Id = 8, Title = "Prayer Meeting", ImageUrl = "/images/siza.jpeg" },      
                new GalleryImage { Id = 9, Title = "Prayer Meeting", ImageUrl = "/images/siza2.jpeg" }, 
                new GalleryImage { Id = 10, Title = "Prayer Meeting", ImageUrl = "/images/siza3.jpeg" },   
                new GalleryImage { Id = 11, Title = "Prayer Meeting", ImageUrl = "/images/siza4.jpeg" },
                new GalleryImage { Id = 12, Title = "Prayer Meeting", ImageUrl = "/images/farm1.jpeg" },
                new GalleryImage { Id = 13, Title = "Prayer Meeting", ImageUrl = "/images/farm2.jpeg" },
                new GalleryImage { Id = 14, Title = "Prayer Meeting", ImageUrl = "/images/farm3.jpeg" },
                new GalleryImage { Id = 15, Title = "Prayer Meeting", ImageUrl = "/images/farm4.jpeg" },
                new GalleryImage { Id = 16, Title = "Prayer Meeting", ImageUrl = "/images/farm5.jpeg" },
                new GalleryImage { Id = 17, Title = "Prayer Meeting", ImageUrl = "/images/farm6.jpeg" },
                new GalleryImage { Id = 18, Title = "Prayer Meeting", ImageUrl = "/images/farm7.jpeg" },
                new GalleryImage { Id = 19, Title = "Prayer Meeting", ImageUrl = "/images/farm8.jpeg" },
                new GalleryImage { Id = 20, Title = "Prayer Meeting", ImageUrl = "/images/farm9.jpeg" },
                new GalleryImage { Id = 19, Title = "Prayer Meeting", ImageUrl = "/images/open1.jpeg" },
                new GalleryImage { Id = 20, Title = "Prayer Meeting", ImageUrl = "/images/open2.jpeg" },
                new GalleryImage { Id = 21, Title = "Prayer Meeting", ImageUrl = "/images/open3.jpeg" },
                new GalleryImage { Id = 22, Title = "Prayer Meeting", ImageUrl = "/images/open4.jpeg" },
                new GalleryImage { Id = 23, Title = "Prayer Meeting", ImageUrl = "/images/open5.jpeg" },
                new GalleryImage { Id = 24, Title = "Prayer Meeting", ImageUrl = "/images/open7.jpeg" },
            };

            return View(images);
        }
    }
}