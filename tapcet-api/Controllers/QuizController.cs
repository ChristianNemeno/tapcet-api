using Microsoft.AspNetCore.Mvc;

namespace tapcet_api.Controllers
{
    public class QuizController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
