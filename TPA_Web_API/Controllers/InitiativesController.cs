using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TPA_Web_API.Models;

namespace TPA_Web_API.Controllers
{
    public class InitiativesController : ApiController
    {
        private TPA_ImplemEntities db = new TPA_ImplemEntities();
        private DBLayer dblayer = new DBLayer();

        public IHttpActionResult GetInitiativeByBID(string id)
        {
            //var initiative = db.Data_Initiative.FirstOrDefault((p) => p.ID == id);
            //var initiative = (from p in db.Data_Initiative
            //                  where p.TPB_ID==id
            //                  group p by p.TPB_ID into groups
            //                  select groups.OrderByDescending(p => p.Created_At).FirstOrDefault());
            Initiative initiative = dblayer.getInitiativeByID(id);

            if (initiative == null)
            {
                return NotFound();
            }

            return Ok(new {data = initiative});
        }

        public IHttpActionResult GetInitiativeList()
        {
            //List<Data_Initiative> initiativeList = (from p in db.Data_Initiative
            //                                        group p by p.TPB_ID into groups
            //                                        select groups.OrderByDescending(p => p.Created_At)).ToList();

            var initiativeList = (from p in db.Data_Initiative
                                  group p by p.TPB_ID into groups
                                  select groups.OrderByDescending(p => p.Created_At).FirstOrDefault()).ToList();

            List<Data_Initiative> list = initiativeList;
            return Ok(new { data=list});
        }
        
        [Route("api/branches/{branchID}/initiatives")]
        [HttpGet]
        public IHttpActionResult GetAllInitiativesByBranchID(int branchID)
        {
            //List<Initiative> initiatives = new List<Initiative>();

            //List<string> initiativeListID = new List<string>();

            var initiativeListID = (from p in db.Data_Branch_Allocation
                                    where p.Branch_ID == branchID
                                    group p by new { p.TPB_ID, p.Branch_ID } into groups
                                    select groups.Key.TPB_ID).ToList();

            List<Initiative> initiatives = new List<Initiative>();
            foreach(var item in initiativeListID)
            {
                initiatives.Add(dblayer.getInitiativeByID(item));
            }
            return Ok(new { data=initiatives});
        }

    }
}
