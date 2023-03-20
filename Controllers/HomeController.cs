using CSBugTracker.Extensions;
using CSBugTracker.Models;
using CSBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CSBugTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IBTCompanyService _companyService;
        public HomeController(IBTCompanyService companyService)
        {
            _companyService = companyService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
           
            return View();
        }

        public async Task<IActionResult> PortoIndex()
        {
            int companyId = User.Identity!.GetCompanyId();

            Company? company = await _companyService.GetCompanyInfoAsync(companyId);

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