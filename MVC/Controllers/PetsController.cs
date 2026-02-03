using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Text.Json;

namespace MVC.Controllers
{
    public class PetsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public PetsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            //Check if the user is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username"))) //This is very important
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/pets");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var pets = JsonSerializer.Deserialize<List<PetViewModel>>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(pets);
            }
            return View(new List<PetViewModel>());  // Return empty list on error

        }
    }        
}
