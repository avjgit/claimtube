using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace VideoAi.Controllers
{
    [Produces("application/json")]
    [Route("api/policies")]
    public class PoliciesController : Controller
    {
        // GET: api/Policies
        [HttpGet]
        public IEnumerable<Policy> Get()
        {
            return new[]
            {
                new Policy
                {
                    PolicyType = "MOD",
                    Number = "3X20110002030",
                    Description = "Ford Focus, AA1234"
                },
                new Policy
                {
                    PolicyType = "HH",
                    Number = "3H10110002125",
                    Description = "E. Birznieka-Upīša iela 21"
                }
            };
        }
    }
}
