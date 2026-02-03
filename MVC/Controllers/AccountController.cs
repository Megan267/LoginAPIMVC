using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Text;
using System.Text.Json;

namespace MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/register", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Registration failed. Username or email may already exist.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var client = _httpClientFactory.CreateClient("ApiClient");
            var jsonContent = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(); //gets data out
                var user = JsonDocument.Parse(responseBody);
                var username = user.RootElement.GetProperty("username").GetString();

                //Set the username in the session
                HttpContext.Session.SetString("Username", username); //NB: you must/can look into adding time out for session for assignment

                return RedirectToAction("Index", "Pets");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // --- LOGOUT ---
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();  // Clear the session
            return RedirectToAction("Index", "Home");
        }
    }
}
