using Microsoft.AspNetCore.Mvc;

namespace PrimeCalcApi.WebService.Controllers
{
    [Route("[controller]")]
    public class StatusController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok($"Healthy");
        }
    }
}