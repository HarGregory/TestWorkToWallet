using Microsoft.AspNetCore.Mvc;
using TestWorkToWallet.DAL;
using TestWorkToWallet.Models;

namespace TestWorkToWallet.Controllers
{
    public class SenderController : Controller
    {
        private readonly MessageRepository _messageRepository;
        private readonly ILogger<MessageRepository> _logger;

        public SenderController(MessageRepository messageRepository, ILogger<MessageRepository> logger)
        {
            _messageRepository = messageRepository;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("api/send")]
        public IActionResult SendMessage([FromBody] Message message)
        {
            try
            {
                message.Timestamp = DateTime.UtcNow;

                bool isSuccess = _messageRepository.AddMessage(message);

                if (isSuccess)
                {
                    return Ok(); // HTTP 200
                }
                else
                {
                    _logger.LogError($"Failed to add message");
                    return BadRequest("Failed to add message."); // HTTP 400
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while sending message: {ex.Message}");
                return StatusCode(500, "Internal server error"); // HTTP 500
            }
        }
    }
}
