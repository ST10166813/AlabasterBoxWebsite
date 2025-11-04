using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Alabaster.Models;
using Alabaster.Services; // Make sure this matches your folder path
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Alabaster.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Store keys securely in appsettings.json for production
        private const string YouTubeApiKey = "AIzaSyBHLkEHH9kmnS-B-BFiZaBUjZNugn4dHiM"; // Replace if needed
        private const string YouTubeApiKey_Secondary = "AIzaSyBGEPb4ifXD94dJ8T7BUKOvHNU22LhC2aY"; /////////// added fallback key
        private const string YouTubeChannelId = "UCFqkqWkSvJ_hm0m2whwGZWQ";             // Your channel ID
        //  private const string YouTubeChannelId = "UCBdi3O3UDeD5gleDqi5gx3g";             // Your channel ID

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            string? videoId = null;

            // 1) Try primary API key
            try
            {
                var yt = new YouTubeHelper(YouTubeApiKey, YouTubeChannelId);
                videoId = await yt.GetLatestOrLiveAsync();
                _logger.LogInformation("Fetched YouTube video ID with primary key: {VideoId}", videoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch YouTube video with primary key."); /////////// log primary failure
                videoId = null;
            }

            // 2) If primary produced no result, try secondary/fallback key
            if (string.IsNullOrEmpty(videoId))
            {
                try
                {
                    var ytFallback = new YouTubeHelper(YouTubeApiKey_Secondary, YouTubeChannelId); /////////// using fallback key
                    videoId = await ytFallback.GetLatestOrLiveAsync();
                    _logger.LogInformation("Fetched YouTube video ID with secondary key: {VideoId}", videoId); /////////// log fallback success
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Failed to fetch YouTube video with secondary key."); /////////// log fallback failure
                    videoId = null;
                }
            }

            ViewBag.YouTubeVideoId = videoId;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}
