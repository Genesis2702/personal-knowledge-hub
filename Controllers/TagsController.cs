using Microsoft.AspNetCore.Mvc;

namespace PersonalKnowledgeHub.Controllers
{
    public class TagsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
