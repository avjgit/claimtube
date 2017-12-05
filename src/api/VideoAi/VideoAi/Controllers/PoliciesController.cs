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
                    PolicyType = "kasko",
                    Number = "3X20110002030",
                    Description = "Volvo V50, JE7758"
                },
                new Policy
                {
                    PolicyType = "home",
                    Number = "3H10110002125",
                    Description = "E. Birznieka-Upīša iela 21"
                }
            };
        }
    }
}
