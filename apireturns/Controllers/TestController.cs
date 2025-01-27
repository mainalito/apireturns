using apireturns.NewFolder;
using Microsoft.AspNetCore.Mvc;

namespace apireturns.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly ExcelProcessingService _service;

        public TestController(ExcelProcessingService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok("ExcelProcessingService is working!");
        }
    }

}
