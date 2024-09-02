using Microsoft.AspNetCore.Mvc;

namespace TestWorkToWallet.Controllers
{
    public class RealTimeViewerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
