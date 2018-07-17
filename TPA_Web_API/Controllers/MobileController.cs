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
        private Entities db = new Entities();

        [Route("api/users/{userID}/resources")]
        [HttpGet]
        public IHttpActionResult GetResources(int userID)
        {
            List<Mobile_Channel> accounts = dblayer.GetAssignedMobile_Channels(userID);

            var initiatives = (from i in db.Data_Initiative
                               group i by new { i.tpb_id, i.year, i.month, i.channel, i.agency, i.initiative_type, i.initiative_sub_type, i.category, i.division, i.brand, i.initiative_title, i.materials, i.start_date, i.end_date, i.account, i.sell_in_date, i.account_allocation } into grp
                               select grp
                               .OrderByDescending(p => p.created_at)
                               .FirstOrDefault());
            var data = (from b_apd in db.Ref_Branch_APD
                        join ba in db.Ref_Branch_Assignment
                        on b_apd.branch_apd_id equals ba.branch_apd_id
                        group b_apd by b_apd.channel into channelGroup
                        orderby channelGroup.Key ascending
                        select new Mobile_Channel
                        {
                            Name = channelGroup.Key,
                            AccountList = (from b_apd in db.Ref_Branch_APD
                                           join ba in db.Ref_Branch_Assignment
                                           on b_apd.branch_apd_id equals ba.branch_apd_id
                                           where b_apd.channel == channelGroup.Key
                                           group b_apd by b_apd.account into accountGroup
                                           orderby accountGroup.Key ascending
                                           select new Mobile_Account
                                           {
                                               Name = accountGroup.Key,
                                               BranchList = (from b_apd in db.Ref_Branch_APD
                                                             join ba in db.Ref_Branch_Assignment
                                                             on b_apd.branch_apd_id equals ba.branch_apd_id
                                                             where b_apd.account == accountGroup.Key && b_apd.channel == channelGroup.Key
                                                             select new Mobile_Branch
                                                             {
                                                                 BranchName = b_apd.branch_name,
                                                                 BranchAddress = b_apd.address,
                                                                 BranchID = b_apd.branch_apd_id,
                                                                 CDS = b_apd.cds,
                                                                 TeamLead = b_apd.team_lead,
                                                                 InitiativeList = (from ib in db.Data_Initiative_Branch
                                                                                   join i in initiatives
                                                                                   on new { ib.tpb_id, ib.account, ib.materials } equals new { i.tpb_id, i.account, i.materials }
                                                                                   where ib.branch_apd_id == b_apd.branch_apd_id
                                                                                   select new Mobile_Initiative
                                                                                   {
                                                                                       TPB_ID = i.tpb_id,
                                                                                       Brand = i.brand,
                                                                                       Category = i.category,
                                                                                       Division = i.division,
                                                                                       EndDate = i.end_date,
                                                                                       InitiativeSubType = i.initiative_sub_type,
                                                                                       InitiativeTitle = i.initiative_title,
                                                                                       InitiativeType = i.initiative_type,
                                                                                       Month = i.month,
                                                                                       Year = i.year,
                                                                                       //MonthNo = dblayer.GetTPA_Calendar(dblayer.GetCurrentTime()).month_number,
                                                                                       //WeekNo = dblayer.GetTPA_Calendar(dblayer.GetCurrentTime()).week_number,
                                                                                       StartDate = i.start_date,
                                                                                       SellInDate = i.sell_in_date,
                                                                                   }).ToList()
                                                             }).ToList()
                                           }).ToList()
                        }
                       ).ToList();


            //if (accounts.Count() <= 0 || accounts == null)
            //{
            //    return ResponseMessage(
            //       Request.CreateResponse(HttpStatusCode.NotFound, new { errormessage = "No records found", success = 0 })
            //       );
            //}

            return Ok(new { data = data, success = 1, errormessage = "" });
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

            var initiatives = (from i in db.Data_Initiative
                               group i by new { i.tpb_id, i.year, i.month, i.channel, i.agency, i.initiative_type, i.initiative_sub_type, i.category, i.division, i.brand, i.initiative_title, i.materials, i.start_date, i.end_date, i.account, i.sell_in_date, i.account_allocation } into grp
                               select grp
                               .OrderByDescending(p => p.created_at)
                               .FirstOrDefault());
            var data = (from b_apd in db.Ref_Branch_APD
                        join ba in db.Ref_Branch_Assignment
                        on b_apd.branch_apd_id equals ba.branch_apd_id
                        group b_apd by b_apd.channel into channelGroup
                        orderby channelGroup.Key ascending
                        select new Mobile_Channel
                        {
                            Name = channelGroup.Key,
                            AccountList = (from b_apd in db.Ref_Branch_APD
                                           join ba in db.Ref_Branch_Assignment
                                           on b_apd.branch_apd_id equals ba.branch_apd_id
                                           where b_apd.channel == channelGroup.Key
                                           group b_apd by b_apd.account into accountGroup
                                           orderby accountGroup.Key ascending
                                           select new Mobile_Account
                                           {
                                               Name = accountGroup.Key,
                                               BranchList = (from b_apd in db.Ref_Branch_APD
                                                             join ba in db.Ref_Branch_Assignment
                                                             on b_apd.branch_apd_id equals ba.branch_apd_id
                                                             where b_apd.account == accountGroup.Key && b_apd.channel == channelGroup.Key
                                                             select new Mobile_Branch
                                                             {
                                                                 BranchName = b_apd.branch_name,
                                                                 BranchAddress = b_apd.address,
                                                                 BranchID = b_apd.branch_apd_id,
                                                                 CDS = b_apd.cds,
                                                                 TeamLead = b_apd.team_lead,
                                                                 InitiativeList = (from ib in db.Data_Initiative_Branch
                                                                                   join i in initiatives
                                                                                   on new { ib.tpb_id, ib.account, ib.materials} equals new { i.tpb_id, i.account, i.materials}
                                                                                   where ib.branch_apd_id==b_apd.branch_apd_id
                                                                                   select new Mobile_Initiative
                                                                                   {
                                                                                       TPB_ID = i.tpb_id,
                                                                                       Brand = i.brand,
                                                                                       Category = i.category,
                                                                                       Division = i.division,
                                                                                       EndDate = i.end_date,
                                                                                       InitiativeSubType = i.initiative_sub_type,
                                                                                       InitiativeTitle = i.initiative_title,
                                                                                       InitiativeType = i.initiative_type,
                                                                                       Month = i.month,
                                                                                       Year = i.year,
                                                                                       //MonthNo = dblayer.GetTPA_Calendar(dblayer.GetCurrentTime()).month_number,
                                                                                       //WeekNo = dblayer.GetTPA_Calendar(dblayer.GetCurrentTime()).week_number,
                                                                                       StartDate = i.start_date,
                                                                                       SellInDate = i.sell_in_date,
                                                                                   }).ToList()
                                                             }).ToList()
                                           }).ToList()
                        }
                       ).ToList();

            return data;
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
