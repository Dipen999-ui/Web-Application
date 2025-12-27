using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using learnify.Models;

namespace learnify.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly AppDbContext _context;

        [ActivatorUtilitiesConstructor]
        public UserController(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        // =========================
        // GET ALL USERS (ADMIN)
        // =========================
        public IActionResult GetUser()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // =========================
        // REGISTER PAGE
        // =========================
        public IActionResult RegisterForm()
        {
            return View();
        }

        // =========================
        // LOGIN PAGE (USER + ADMIN)
        // =========================
        public ActionResult LoginForm()
        {
            return View();
        }

        // =========================
        // LOGIN LOGIC
        // =========================
        [HttpPost]
        public ActionResult Login([FromForm] User user)
        {
            // 🔐 ADMIN LOGIN (HARDCODED)
            if (user.Email == "admin@gmail.com" && user.Password == "admin123")
            {
#pragma warning disable
                _httpContext.HttpContext.Session.SetString("UserType", "Admin");
                _httpContext.HttpContext.Session.SetString("User", "Admin");
                _httpContext.HttpContext.Session.SetString("UserId", Guid.Empty.ToString());
#pragma warning restore

                TempData["Success"] = "Welcome Admin!";
                return RedirectToAction("Index", "Home");
            }

            // 👤 NORMAL USER LOGIN (DATABASE)
            var dbUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);

            if (dbUser == null)
            {
                TempData["Error"] = "No user found, try batman!";
                return RedirectToAction(nameof(LoginForm));
            }

            if (dbUser.Password != user.Password)
            {
                TempData["Error"] = "Username or password wrong.";
                return RedirectToAction(nameof(LoginForm));
            }

#pragma warning disable
            _httpContext.HttpContext.Session.SetString("UserType", dbUser.UserType);
            _httpContext.HttpContext.Session.SetString("User", dbUser.Username);
            _httpContext.HttpContext.Session.SetString("UserId", dbUser.UserId.ToString());
#pragma warning restore

            TempData["Success"] = "Login successful!";
            TempData["LoggedInUser"] = dbUser.Username;

            return RedirectToAction("Index", "Home");
        }

        // =========================
        // REGISTER USER
        // =========================
        [HttpPost]
        public IActionResult Register([FromForm] User user)
        {
            if (!string.IsNullOrEmpty(user.Username) &&
                !string.IsNullOrEmpty(user.FullName) &&
                !string.IsNullOrEmpty(user.Email) &&
                !string.IsNullOrEmpty(user.Password))
            {
                user.UserType = "User";
                user.CreatedAt = DateTime.UtcNow;

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "Registration successful!";
                return RedirectToAction(nameof(LoginForm));
            }

            TempData["Error"] = "Please fill up all sections.";
            return RedirectToAction(nameof(RegisterForm));
        }

        // =========================
        // ERROR PAGE
        // =========================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
