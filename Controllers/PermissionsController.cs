using Microsoft.AspNetCore.Mvc;

namespace PersonalKnowledgeHub.Controllers;

public class PermissionsController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}