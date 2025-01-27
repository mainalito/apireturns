using apireturns.Models;
using apireturns.NewFolder;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace apireturns.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ExcelProcessingService _excelProcessingService;

        public ValuesController(ExcelProcessingService excelProcessingService)
        {
            _excelProcessingService = excelProcessingService;
        }
        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost]
        public async Task<IActionResult> UploadAndProcess(
          [FromForm] IFormFile file,
          [FromForm] string return_submission,
          [FromForm] string return_template_id)
        {
            try
            {
                // Parse JSON into a DTO
                var returnSubmissionValue = JsonConvert.DeserializeObject<ReturnSubmissionDto>(return_submission);
                Console.WriteLine(JsonConvert.SerializeObject(returnSubmissionValue, Formatting.Indented));
                if (returnSubmissionValue != null) {
                    var result = await _excelProcessingService.ProcessExcelFile(file, returnSubmissionValue, return_template_id);
                    Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

                    if (!result.Success)
                    {
                        return BadRequest(result);
                    }
                    return Ok(result);
                }
                // Process Excel File
                return BadRequest("Invalid Data Submission");

              
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error processing file: {ex.Message}"
                });
            }
        }


    // GET api/<ValuesController>/5
    [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

     

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
