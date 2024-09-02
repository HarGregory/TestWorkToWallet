using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TestWorkToWallet.DAL;

namespace TestWorkToWallet.Controllers
{
    public class LastMinuteViewerController : Controller
    {
        private readonly MessageRepository _messageRepository;

        private readonly IConfiguration _configuration;

        public LastMinuteViewerController(MessageRepository messageRepository,IConfiguration configuration)
        {
            _messageRepository = messageRepository;
            _configuration = configuration;

        }
        public IActionResult Index()
        {
            int interval = _configuration.GetValue<int>("MessageSettings:LastMinuteInterval");
            ViewData["Interval"] = interval;
            return View();
        }

        [HttpGet("api/lastminute")]
        public async Task<IActionResult> GetLastMinuteMessages()
        {
            int interval = _configuration.GetValue<int>("MessageSettings:LastMinuteInterval");

            DateTime endDate = DateTime.UtcNow;
            DateTime startDate = endDate.AddMinutes(-interval);

            var messages = await _messageRepository.GetMessagesAsync(startDate, endDate);

            return Ok(messages);
        }
    }
}
