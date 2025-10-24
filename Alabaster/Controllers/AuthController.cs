using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Alabaster.Services;
using System;
using System.Threading.Tasks;

using FirebaseAdminAuth = FirebaseAdmin.Auth.FirebaseAuth;

namespace Alabaster.Controllers
{
    // === Helper class to store user data (optional but cleaner) ===
    public class FirebaseUser
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
    // ==============================================================

    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly FirebaseAuthService _authService;
        private readonly FirebaseClient _dbClient;

        public AuthController(FirebaseAuthService authService)
        {
            _authService = authService;
            _dbClient = new FirebaseClient("https://alabaster-8cfcd-default-rtdb.firebaseio.com/");
        }

        [HttpGet("Register")] public IActionResult Register() => View();
        [HttpGet("Login")] public IActionResult Login() => View();
        [HttpGet("ForgotPassword")] public IActionResult ForgotPassword() => View();

        // ---------------- Registration ----------------
        [HttpPost("Register")]
        public async Task<IActionResult> Register(string name, string email, string password) // <<-- ADDED 'name'
        {
            try
            {
                // Strong password validation
                if (string.IsNullOrWhiteSpace(password) ||
                    password.Length < 6 ||
                    !System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]") ||
                    !System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]") ||
                    !System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]") ||
                    !System.Text.RegularExpressions.Regex.IsMatch(password, @"[\W_]"))
                {
                    ViewBag.Error = "Password must be at least 6 characters and include uppercase, lowercase, number, and special character.";
                    return View();
                }

                var result = await _authService.Register(email, password);
                string uid = result.User.LocalId;
                
                // === NEW: Save user name to Realtime Database ===
                var userProfile = new FirebaseUser { Email = email, Name = name };
                await _dbClient
                    .Child("users") // Create a 'users' node
                    .Child(uid)     // Use the Firebase UID as the key
                    .PutAsync(userProfile);
                // ===============================================

                TempData["Success"] = "Registration successful! Please login to continue.";
                return RedirectToAction("Login"); // Redirect to login page
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (msg.Contains("EMAIL_EXISTS")) ViewBag.Error = "Email already exists.";
                else ViewBag.Error = "Registration failed. " + msg;
                return View();
            }
        }

        // ---------------- Login ----------------
        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var result = await _authService.Login(email, password);
                string uid = result.User.LocalId;

                // === NEW: Retrieve user name from Realtime Database ===
                var userProfile = await _dbClient
                    .Child("users")
                    .Child(uid)
                    .OnceSingleAsync<FirebaseUser>();

                string userName = userProfile?.Name ?? email; // Default to email if name isn't found
                // =====================================================

                HttpContext.Session.SetString("FirebaseToken", result.FirebaseToken);
                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("UserId", uid);
                HttpContext.Session.SetString("UserName", userName); // <<-- STORING NAME IN SESSION

                var isAdmin = await _dbClient
                    .Child("admins")
                    .Child(uid)
                    .OnceSingleAsync<bool?>() ?? false;

                HttpContext.Session.SetString("IsAdmin", isAdmin ? "true" : "false");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (msg.Contains("EMAIL_NOT_FOUND") || msg.Contains("INVALID_LOGIN_CREDENTIALS"))
                    ViewBag.Error = "Account not found.";
                else if (msg.Contains("INVALID_PASSWORD"))
                    ViewBag.Error = "Incorrect password.";
                else
                    ViewBag.Error = "Login failed. " + msg;

                return View();
            }
        }

        // ---------------- Forgot Password ----------------
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Please enter an email address.";
                return View();
            }

            try
            {
                await _authService.SendPasswordResetEmail(email);
                ViewBag.Message = "If an account exists with that email, a reset link has been sent.";
            }
            catch (Exception ex)
            {
                string msg = ex.Message ?? "";
                if (msg.Contains("EMAIL_NOT_FOUND"))
                    ViewBag.Error = "No account found with that email.";
                else
                    ViewBag.Error = "Failed to send reset link. " + msg;
            }

            return View();
        }

        // ---------------- Google Login ----------------
        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] string idToken)
        {
            if (string.IsNullOrEmpty(idToken))
                return Json(new { success = false, error = "No token provided." });

            try
            {
                var decodedToken = await FirebaseAdminAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

                string uid = decodedToken.Uid;
                string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;
                // === NEW: Extract name from Google Claims ===
                string name = decodedToken.Claims.ContainsKey("name") ? decodedToken.Claims["name"].ToString() : email;
                // ============================================

                // === OPTIONAL: Store/Update Google User Data in DB ===
                var userProfile = new FirebaseUser { Email = email, Name = name };
                await _dbClient
                    .Child("users") 
                    .Child(uid)     
                    .PatchAsync(userProfile); // Use Patch to avoid overwriting other potential fields
                // =====================================================

                HttpContext.Session.SetString("FirebaseToken", idToken);
                HttpContext.Session.SetString("UserEmail", email ?? "");
                HttpContext.Session.SetString("UserId", uid);
                HttpContext.Session.SetString("UserName", name ?? email ?? "User"); // <<-- STORING NAME IN SESSION

                var isAdmin = await _dbClient
                    .Child("admins")
                    .Child(uid)
                    .OnceSingleAsync<bool?>() ?? false;

                HttpContext.Session.SetString("IsAdmin", isAdmin ? "true" : "false");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Token verification failed: " + ex.Message });
            }
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}