using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
        private WCPIADBEntities db = new WCPIADBEntities();

        [Route("api/users/{userID}/resources")]
        [HttpGet]
        public IHttpActionResult GetResources(int userID)
        {

            //var initiatives = (from i in db.Data_Initiative
            //                   group i by new { i.tpb_id, i.year, i.month, i.channel, i.agency, i.initiative_type, i.initiative_sub_type, i.category, i.division, i.brand, i.initiative_title, i.materials, i.start_date, i.end_date, i.account, i.sell_in_date, i.account_allocation } into grp
            //                   select grp
            //                   .OrderByDescending(p => p.created_at)
            //                   .FirstOrDefault());

            #region MyRegion
            //var month_no = dblayer.GetTPA_Calendar(dblayer.GetCurrentTime()).month_number;
            //var week_no = dblayer.GetTPA_Calendar(dblayer.GetCurrentTime()).week_number;

            //var data = (from b_apd in db.Ref_Branch_APD
            //            join ba in db.Ref_Branch_Assignment
            //            on b_apd.branch_apd_id equals ba.branch_apd_id
            //            group b_apd by b_apd.channel into channelGroup
            //            orderby channelGroup.Key ascending
            //            select new Mobile_Channel
            //            {
            //                Name = channelGroup.Key,
            //                AccountList = (from b_apd in db.Ref_Branch_APD
            //                               join ba in db.Ref_Branch_Assignment
            //                               on b_apd.branch_apd_id equals ba.branch_apd_id
            //                               where b_apd.channel == channelGroup.Key
            //                               group b_apd by b_apd.account into accountGroup
            //                               orderby accountGroup.Key ascending
            //                               select new Mobile_Account
            //                               {
            //                                   Name = accountGroup.Key,
            //                                   BranchList = (from b_apd in db.Ref_Branch_APD
            //                                                 join ba in db.Ref_Branch_Assignment
            //                                                 on b_apd.branch_apd_id equals ba.branch_apd_id
            //                                                 where b_apd.account == accountGroup.Key && b_apd.channel == channelGroup.Key
            //                                                 select new Mobile_Branch
            //                                                 {
            //                                                     BranchName = b_apd.branch_name,
            //                                                     BranchAddress = b_apd.address,
            //                                                     BranchID = b_apd.sfa_id,
            //                                                     CDS = b_apd.cds,
            //                                                     TeamLead = b_apd.team_lead,
            //                                                     InitiativeList = (from ib in db.Data_Initiative_Branch
            //                                                                       join i in initiatives
            //                                                                       on new { ib.tpb_id, ib.account, ib.materials } equals new { i.tpb_id, i.account, i.materials }
            //                                                                       where ib.branch_apd_id == b_apd.branch_apd_id
            //                                                                       select new Mobile_Initiative
            //                                                                       {
            //                                                                           TPB_ID = i.tpb_id,
            //                                                                           Brand = i.brand,
            //                                                                           Category = i.category,
            //                                                                           Division = i.division,
            //                                                                           EndDate = i.end_date,
            //                                                                           InitiativeSubType = i.initiative_sub_type,
            //                                                                           InitiativeTitle = i.initiative_title,
            //                                                                           InitiativeType = i.initiative_type,
            //                                                                           Month = i.month,
            //                                                                           Year = i.year,
            //                                                                           MonthNo = month_no,
            //                                                                           WeekNo = week_no,
            //                                                                           StartDate = i.start_date,
            //                                                                           SellInDate = i.sell_in_date,
            //                                                                           MaterialList = (from ib in db.Data_Initiative_Branch
            //                                                                                           where (ib.sfa_internal_id == b_apd.sfa_id) && (ib.tpb_id == i.tpb_id)
            //                                                                                           select new Mobile_Material
            //                                                                                           {
            //                                                                                               Material = ib.materials
            //                                                                                           }).ToList()
            //                                                                       }).ToList()
            //                                                 }).ToList()
            //                               }).ToList()
            //            }
            //           ).ToList();




            //if (accounts.Count() <= 0 || accounts == null)
            //{
            //    return ResponseMessage(
            //       Request.CreateResponse(HttpStatusCode.NotFound, new { errormessage = "No records found", success = 0 })
            //       );
            //} 
            #endregion

            List<Mobile_Channel> accounts = new List<Mobile_Channel>();
            try
            {
                accounts = dblayer.GetAssignedMobile_Channels(userID);
                if (accounts == null || accounts.Count() == 0)
                {
                    dblayer.Error_Insert("No Data Found","GetResources");
                    return Ok(new { data = accounts, success = 0, errormessage = "Oops. Something went wrong" });
                }
            }
            catch (Exception ex)
            {
                dblayer.Error_Insert(ex.ToString(),"GetResources");
                return Ok(new { data = accounts, success = 0, errormessage = "Oops. Something went wrong" });
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
                                                                 BranchID = b_apd.sfa_id,
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
            UserProfile userProfile = new UserProfile();
            try
            {
                userProfile = dblayer.LoginUser(login.Username, login.Password);
            }
            catch (Exception ex)
            {
                dblayer.Error_Insert(ex.ToString(),"UserLogin");
                return Ok(new { data = userProfile, success = 0, errormessage = "Oops. Something went wrong" });
            }

            return Ok(new { data = userProfile, success = 1, errormessage = "" });
        }


        [Route("api/users/initiatives")]
        [HttpPost]
        public IHttpActionResult PostInitiatives(Initiatives initiatives)
        {
            try
            {
                if (initiatives == null)
                {
                    //return Ok(new { success = 0, errormessage = "No data found!" });
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest));
                }
                else
                {
                    List<Initiatives> tmp = new List<Initiatives>();
                    tmp.Add(initiatives);

                    List<Initiatives> i = dblayer.PostInitiatives(tmp);
                }
            }
            catch (Exception ex)
            {
                dblayer.Error_Insert(ex.ToString(),"PostInitiatives");
                //return Ok(new { success = 0, errormessage = "Oops. Something went wrong" });
                //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { errormessage = "No data found", success = 0 }));
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
            //return Ok(new { success = 1, errormessage = "" });
            //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
            return Json(initiatives);
        }

        [Route("api/users/initiativeslist")]
        [HttpPost]
        public IHttpActionResult PostInitiativesList(List<Initiatives> initiatives)
        {
            try
            {
                if (initiatives == null||initiatives.Count()==0)
                {
                    //return Ok(new { success = 0, errormessage = "No data found!" });
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest));
                }
                else
                {
                    List<Initiatives> i = dblayer.PostInitiatives(initiatives);
                }
            }
            catch (Exception ex)
            {
                dblayer.Error_Insert(ex.ToString(), "PostInitiativesList");
                //return Ok(new { success = 0, errormessage = "Oops. Something went wrong" });
                //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { errormessage = "No data found", success = 0 }));
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
            //return Ok(new { success = 1, errormessage = "" });
            //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
            return Json(initiatives);

        }
        
        [Route("api/users/{username}/{password}/profile")]
        [HttpGet]
        public IHttpActionResult GetByUsername(string username, string password)
        {
            UserProfile userProfile = new UserProfile();
            try
            {
                userProfile = dblayer.GetByUsername(username, password);
                if (userProfile == null)
                {
                    return Ok(new { data = userProfile, success = 0, errormessage = "No data found" });
                }
            }
            catch(Exception ex)
            {
                dblayer.Error_Insert(ex.ToString(),"PostInitiativesList");
                return Ok(new { data = userProfile, success = 0, errormessage = "Oops. Something went wrong" });
            }
            return Ok(new { data = userProfile, success = 1, errormessage = "" });
        }

        [Route("api/users/{status_id}/issues")]
        [HttpGet]
        public IHttpActionResult GetIssues(int status_id)
        {
            Status status = new Status();
            try
            {
                status = dblayer.GetStatus(status_id);
                if (status == null)
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { errormessage = "No data found", success = 0 }));
                }
            }
            catch (Exception ex)
            {
                dblayer.Error_Insert(ex.ToString(),"GetIssues");
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { errormessage = "Oops. Something went wrong", success = 0 }));
            }
            return Ok(new { data = status, success = 1, errormessage = "" });
        }

        [Route("api/users/image")]
        [HttpPost]
        public IHttpActionResult PostImage(Images images)
        {
            List<Images> test = new List<Images>();
            test.Add(images);

            try
            {
                List<Images> i = dblayer.PostImages(test);
            }
            catch (Exception ex)
            {
                dblayer.Error_Insert(ex.ToString(),"GetImage");
            }

            //return Ok(new { data = images, success = 1, errormessage = "" });
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        [Route("api/users/image")]
        [HttpGet]
        public IHttpActionResult GetImage()
        {
            Images images = dblayer.GetImage();

            try
            {
                images = dblayer.GetImage();
            }
            catch (Exception ex)
            {
                dblayer.Error_Insert(ex.ToString(),"GetImage");
            }
            return Ok(new { data = images });
        }

        [Route("api/users/files")]
        [HttpPost]
        public IHttpActionResult PostFiles(List<Files> files)
        {
            try
            {
                if (files == null || files.Count() == 0)
                {
                    //return Ok(new { success = 0, errormessage = "No data found!" });
                    //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { errormessage = "No data found", success = 0 }));
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest));
                }
                else
                {
                    List<Files> i = dblayer.PostFiles(files);
                }
            }
            catch (Exception ex)
            {
                dblayer.Error_Insert(ex.ToString(),"PostFiles");
                //return Ok(new { success = 0, errormessage = "Oops. Something went wrong" });
                //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { errormessage = "No data found", success = 0 }));
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
            //return Ok();
            //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
            return Json(files);
        }



    }
}
