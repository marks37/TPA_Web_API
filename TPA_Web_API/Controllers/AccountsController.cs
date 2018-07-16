using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TPA_Web_API.Models;

namespace TPA_Web_API.Controllers
{
    public class AccountsController : ApiController
    {
        private DBLayer dblayer = new DBLayer();
        private TPA_ImplemEntities db = new TPA_ImplemEntities();

        [Route("api/users/{userID}/accounts")]
        [HttpGet]
        //public IHttpActionResult GetAccounts(int userID)
        public List<Mobile_Channel> GetAccounts(int userID)
        {
            List<Mobile_Channel> accounts = dblayer.GetAssignedMobile_Channels(userID);

            if (accounts.Count() <= 0 || accounts == null)
            {
                //return ResponseMessage(
                //   Request.CreateResponse(HttpStatusCode.NotFound, new { errormessage = "No records found", success = 0 })
                //   );
            }

            //return Ok(new { data = accounts, success = 1, errormessage = "" });
            return accounts;
        }
    }
}
