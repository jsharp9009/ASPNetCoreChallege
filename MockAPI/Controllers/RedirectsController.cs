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
        /// <summary>
        /// Returns url redirects for consumption by challenge site
        /// </summary>
        /// <returns>IEnumerable of Redirect Objects</returns>
        [HttpGet]
        public ContentResult Get()
        {
            //Read all text from the file
            var file = IO.File.ReadAllText("Assets/redirects.json");
            //return the string as the content
            return Content(file, "application/json");
        }
    }
}
