using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using IO = System.IO;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MockAPI.FakeAPI
{
    [Route("api/redirects")]
    [ApiController]
    public class RedirectsController : ControllerBase
    {
        // GET: api/redirects
        [HttpGet]
        public ContentResult Get()
        {
            var file = IO.File.ReadAllText("Assets/redirects.json");
            return Content(file, "application/json");
        }
    }
}
