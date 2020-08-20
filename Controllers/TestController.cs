using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceBusAPI.Controllers
{
    [RoutePrefix("TestController")]
    public class TestController : ApiController
    {
        [HttpGet]
        [Route("GET")]
        
        public IHttpActionResult Get()
        {
            return Ok("Test");
        }
    }
}
