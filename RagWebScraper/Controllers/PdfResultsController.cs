using Microsoft.AspNetCore.Mvc;

namespace RagWebScraper.Controllers
{
    [ApiController]
    [Route("api/pdf")]
    public class PdfResultsController : ControllerBase
    {
        private readonly PdfResultStore _store;

        public PdfResultsController(PdfResultStore store)
        {
            _store = store;
        }

        [HttpGet("results")]
        public IActionResult GetResults()
        {
            var results = _store.GetAll();
            return Ok(results);
        }
    }
}
