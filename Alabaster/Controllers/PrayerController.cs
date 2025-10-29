using Microsoft.AspNetCore.Mvc;
using Alabaster.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace Alabaster.Controllers
{
    public class PrayerController : Controller
    {
        private readonly FirebaseClient _firebase;

        public PrayerController()
        {
            _firebase = new FirebaseClient("https://alabaster-8cfcd-default-rtdb.firebaseio.com/");
        }

        // User: Prayer Request Form
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("FirebaseToken") == null)
                return RedirectToAction("Login", "Auth");

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var prayers = await _firebase.Child("prayers")
                                         .OnceAsync<PrayerRequest>();

            // Get latest prayer from this user (if any)
            var latestPrayer = prayers
                .Select(p => p.Object)
                .Where(p => p.UserEmail == userEmail)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            return View(latestPrayer); // Pass the latest prayer as model
        }


        // User: Submit Prayer Request
        [HttpPost]
        public async Task<IActionResult> Submit(string name, string request)
        {
            if (HttpContext.Session.GetString("FirebaseToken") == null)
                return RedirectToAction("Login", "Auth");

            var prayer = new PrayerRequest
            {
                UserId = HttpContext.Session.GetString("UserId"),
                UserEmail = HttpContext.Session.GetString("UserEmail"),
                Name = name,
                Request = request
            };

            await _firebase.Child("prayers").Child(prayer.Id).PutAsync(prayer);

            TempData["Success"] = "Your prayer request has been sent 🙏";
            return RedirectToAction("Index");
        }

        // Admin: View All Prayer Requests
        public async Task<IActionResult> AdminView()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var prayers = await _firebase.Child("prayers").OnceAsync<PrayerRequest>();
            var prayerList = prayers.Select(p => p.Object).OrderByDescending(p => p.CreatedAt).ToList();

            return View(prayerList);
        }

        [HttpPost]
        public async Task<IActionResult> RespondToPrayer(string id, string response)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            // Fetch the existing prayer request
            var prayerSnapshot = await _firebase.Child("prayers").Child(id).OnceSingleAsync<PrayerRequest>();

            if (prayerSnapshot == null)
                return NotFound();

            // Update with admin response
            prayerSnapshot.Response = response;
            prayerSnapshot.RespondedAt = DateTime.UtcNow;

            await _firebase.Child("prayers").Child(id).PutAsync(prayerSnapshot);

            TempData["Success"] = "Response sent successfully 🙌";
            return RedirectToAction("AdminView");
        }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeletePrayerRequest(string id)
{
    if (string.IsNullOrEmpty(id))
    {
        TempData["Error"] = "Invalid request ID";
        return RedirectToAction("AdminView");
    }

    try
    {
        await _firebase
            .Child("prayers")  // ✅ Correct Firebase node
            .Child(id)         // ✅ Delete item by Firebase key
            .DeleteAsync();

        TempData["Success"] = "Prayer request deleted successfully ✅";
    }
    catch (Exception ex)
    {
        TempData["Error"] = "Error deleting request: " + ex.Message;
    }

    return RedirectToAction("AdminView");
}




    }
}
