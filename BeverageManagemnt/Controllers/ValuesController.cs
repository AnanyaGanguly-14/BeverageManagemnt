using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MLC.Fasat.Api.Report.Controllers
{
    [Route("api/values")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private readonly ServiceBusQueueService _queueService;


        public ValuesController(ServiceBusQueueService queueService)
        {
            _queueService = queueService;
        }

        [HttpPost]
        [Route("send")]
        public async Task<IActionResult> Send()
        {
            await _queueService.SendMessageAsync("Hello from Service Bus!");
            return Ok("Message sent.");
        }


        [HttpPost]
        [Route("receive")]
        public async Task<IActionResult> Receive()
        {
            var message = await _queueService.ReceiveMessageAsync();
            return Ok($"Received: {message}");
        }

    }
}
