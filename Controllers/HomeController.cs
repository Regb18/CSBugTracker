using CSBugTracker.Models;
using CSBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CSBugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBTCompanyService _companyService;
        public HomeController(IBTCompanyService companyService)
        {
            _companyService = companyService;
        }

        public IActionResult Index()
        {
           
            return View();
        }

        public async Task<IActionResult> PortoIndex(int? id)
        {
            Company? company = await _companyService.GetCompanyInfoAsync(id);

            return View(company);
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
    }
}