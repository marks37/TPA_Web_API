using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TPA_Web_API.Models;

namespace TPA_Web_API.Controllers
{
    public class MobileController : ApiController
    {
        private DBLayer dblayer = new DBLayer();

        [Route("api/users/{userID}/resources")]
        [HttpGet]
        public IHttpActionResult GetResources(int userID)
        {
            List<Mobile_Channel> accounts = dblayer.GetAssignedMobile_Channels(userID);

            if (accounts.Count() <= 0 || accounts == null)
            {
                return ResponseMessage(
                   Request.CreateResponse(HttpStatusCode.NotFound, new { errormessage = "No records found", success = 0 })
                   );
            }

            return Ok(new { data = accounts, success = 1, errormessage = "" });
        }

        [Route("api/users/{userID}/resourcestest")]
        [HttpGet]
        public List<Mobile_Channel> GetResourcesTest(int userID)
        {
            List<Mobile_Channel> accounts = dblayer.GetAssignedMobile_Channels(userID);

            if (accounts.Count() <= 0 || accounts == null)
            {
                //return ResponseMessage(
                //   Request.CreateResponse(HttpStatusCode.NotFound, new { errormessage = "No records found", success = 0 })
                //   );
            }

            return accounts;
        }

        public IHttpActionResult Post(JObject jobject)
        {

            return Ok();
        }

        [Route("api/login")]
        [HttpPost]
        public IHttpActionResult UserLogin(UserLogin login)
        {
            UserProfile userProfile = dblayer.LoginUser(login.Username, login.Password);
            if (userProfile == null)
            {
                return ResponseMessage(
                       Request.CreateResponse(HttpStatusCode.NotFound, new { errormessage = "No records found", success = 0 })
                       );
            }
            return Ok(new { data = userProfile, success = 1, errormessage = "" });
        }

    }
}
