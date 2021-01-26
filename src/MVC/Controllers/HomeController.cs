using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVC.Models;
using System.Text;
using MVC.Services;
using IdentityModel.Client;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenService _tokenService;

        public HomeController(ILogger<HomeController> logger,
        ITokenService tokenService)
        {
            _logger = logger;
            _tokenService= tokenService;
        }

        public IActionResult Index()
        {
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

        [Authorize]
        public async Task<IActionResult> Weather(){
            var data = new List<WeatherData>();
            var apiUrl = "https://localhost:5002";

            using (var client = new HttpClient())
            {
                var tokenResponse = await _tokenService.GetToken("api.scope1");
                client.SetBearerToken(tokenResponse.AccessToken);

                var result = client.GetAsync(apiUrl + "/weatherforecast").Result;

                if (result.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions();
                        options.PropertyNameCaseInsensitive = true;
                        options.Converters.Add(new JsonStringEnumConverter());

                    var model = Encoding.UTF8.GetString(result.Content.ReadAsByteArrayAsync().Result);
                    data = JsonSerializer.Deserialize<List<WeatherData>>(model, options);
                    return View(data);
                }
                else
                {
                    throw new Exception("error");
                }
            }
        }
    }
}
