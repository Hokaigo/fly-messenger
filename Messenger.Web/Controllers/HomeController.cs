using Messenger.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    [Authorize]
    public IActionResult Index()
    {
        return View();
    }
}
