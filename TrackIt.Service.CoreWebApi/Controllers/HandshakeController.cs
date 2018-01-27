using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TrackIt.Service.CoreWebApi.Controllers
{
    [Route("api/[controller]")]
    public class HandshakeController : BaseController
    {
        [HttpGet]
        [Authorize]
        [Route("auth")]
        public IActionResult AuthorizedGet()
        {
            return Ok($"Hello {GetNameClaimValue()} ({GetEmailClaimValue()}, {GetIdClaimValue()})");
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(true);
        }
    }
}
