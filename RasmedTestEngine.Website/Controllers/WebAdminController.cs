using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RasmedTestEngine.Common;
using RasmedTestEngine.Common.Sms;
using RasmedTestEngine.Core.Abstract;
using RasmedTestEngine.Core.Entities;
using RasmedTestEngine.Website.Models;
using RasmedTestEngine.Common.Email;
using ImageResizer;
using PagedList;

namespace RasmedTestEngine.Website.Controllers
{
    [Authorize(Roles="Admin")]
    public class WebAdminController : Controller
    {
        private readonly IExaminationRepository _examinationRepository;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        readonly SendSms _messenger = new SendSms();
        readonly MailPusher _mailPusher = new MailPusher();

        

        //private readonly EfDbContext _context = new EfDbContext();
        public WebAdminController(IExaminationRepository examinationRepository)
        {
         
            _examinationRepository = examinationRepository;
        }
        public WebAdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        // GET: WebAdmin
        public ActionResult Index()
        {
            return View();
        }
   
        public ActionResult ResultDetails(int id)
        {
            var entity = _examinationRepository.GetResultLogs.FirstOrDefault(x => x.ResultManager.Id == id);
            return View(entity);
        }

        public ActionResult ResultManagerStepOne()
        {
            ViewBag.ExamId = new SelectList(_examinationRepository.GetExaminations.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult ReportSheet(FormCollection fm)
        {
            var capture = fm["ExamId"];
            if (capture.IsEmpty())
            {
                ViewBag.ExamId = new SelectList(_examinationRepository.GetExaminations.OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View();
            }

            int capture2 = int.Parse(capture);
            var data =
                _examinationRepository.GetResultManagers.Where(x => x.Examination.Id == capture2)
                    .OrderByDescending(x => x.StarTime)
                    .ToList();
            var info = new ReportSheet
            {
                ExamName = _examinationRepository.GetExaminationData(capture2).Name,
                TotalAttempts = data.Count,
                ResultList = data
            };
            ViewBag.NeededExamId= capture2;
            return View(info);// ReportSheet(info);
        }


        public ActionResult ExportResult(int Id)
        {
            var allResult = _examinationRepository.GetResultManagers.Where(x => x.Examination.Id == Id)
                    .OrderByDescending(x => x.StarTime)
                    .ToList();
            int i = 0;
            var dt = new List<ResultDownload>();
            if (allResult.Any())
            {
                foreach (var data in allResult)
                {
                    if (data.EndTime != null)
                    {
                        i = i + 1;
                        var d = new ResultDownload
                        {
                            Id = i,
                            Name = data.MemberProfile.FullName,
                            Date = data.StarTime.ToShortDateString(),
                            Email = data.MemberProfile.Member.Email,
                            MobileNumber = data.MemberProfile.Member.PhoneNumber,
                            Score = data.Remark,
                            TimeSpent = TimeSpent(data.EndTime.ToString(), data.StarTime)
                        };
                        dt.Add(d);
                    }
                }
            }
            
                var gv = new GridView { DataSource = dt };
                gv.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=result.xls");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "";
                var sw = new StringWriter();
                var htw = new HtmlTextWriter(sw);
                gv.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
                return null;
            }



        public string TimeSpent(string endtime, DateTime starttime)
        {
            TimeSpan t = DateTime.Parse(endtime) - starttime;

            return t.Hours + " : " + t.Minutes + " : " + t.Seconds;
        }

        //public ActionResult ReportSheet(ReportSheet rs)
        //{
        //    return View(rs);
        //}




        public ActionResult Member(string currentFilter, string searchString, int? page)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var siteuser = _examinationRepository.GetMemberProfiles.OrderBy(x => x.RegistrationDate).ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                siteuser = (List<MemberProfile>) siteuser.Where(s => s.FullName.Contains(searchString)
                                                      || s.Member.UserName.Contains(searchString));
            }
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(siteuser.ToPagedList(pageNumber, pageSize));
            //return View(siteuser);
        }

        public ActionResult AddNewMember()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddNewMember(RegisterViewModel rvm)
        {
            if (ModelState.IsValid)
            {
                var user = new Member
                {
                    UserName = rvm.Email,
                    Email = rvm.Email,
                    EmailConfirmed = true,
                    PhoneNumber = rvm.MobileNumber,
                    PhoneNumberConfirmed = true,
                    TwoFactorEnabled = false,
                    MemberProfile =
                        new MemberProfile
                        {
                            FullName = rvm.FullName,
                            RegistrationDate = DateTime.Now,
                            Address = rvm.Address
                        },
                };

                var result = UserManager.Create(user, rvm.Password);
                if (result.Succeeded)
                {
                    StringWriter writer = new StringWriter();
                    HtmlTextWriter html = new HtmlTextWriter(writer);

                    html.RenderBeginTag(HtmlTextWriterTag.H1);
                    html.WriteEncodedText("User Registration Notification");
                    html.RenderEndTag();
                    html.WriteEncodedText("Dear Sir/Ma ");
                    html.WriteBreak();
                    html.RenderBeginTag(HtmlTextWriterTag.P);
                    html.WriteEncodedText("This is to notify you that you've been successfully registered on our platform. Kindly login with the details below");
                    html.RenderEndTag();
                    html.WriteBreak();
                    html.WriteBreak();
                    html.WriteEncodedText("Username : " + user.UserName);
                    html.WriteBreak();
                    html.WriteEncodedText("Password :  " + rvm.Password);
                    html.WriteBreak();
                    html.WriteEncodedText("url: evaluation.rasmedpublications.com");
                    html.WriteBreak();
                    html.WriteBreak();
                    html.WriteEncodedText("Should you encounter any problem or difficulty, kindly get in touch with us via the support page");
                    html.WriteBreak();
                    html.WriteBreak();
                    html.WriteEncodedText("Regards");
                    html.WriteBreak();
                    html.WriteEncodedText("Rasmed Publications HR Team");
                    html.Flush();

                    string htmlmsg = writer.ToString();

                    _mailPusher.SendEmail(user.Email, "Administrator", "sanusibs@rasmedpublications.com", "Successful User Registration", htmlmsg);

                    var newmsg = new SmsMessage
                    {
                        Message = "You've been successfully added to Rasmed Publications Evaluation Platform. Kindly Check your E-mail for details",
                        Number = rvm.MobileNumber,
                        SenderId = "RASMED PUB"
                    };
                    _messenger.SendMessage(newmsg);



                    //sendmail and sms
                    TempData["notification"] = "The Member has been successfully added";
                    return RedirectToAction("Member");
                }
                AddErrors(result);
            }

            return View(rvm);
        }

        public ActionResult MemberBulkUpload()
        {
            return View();
        }
        [HttpPost]
        public ActionResult MemberBulkUpload(HttpPostedFileBase file)
        {
            var postedFileBase = Request.Files["MemberExelUpload"];
            if (postedFileBase != null && postedFileBase.ContentLength > 0)
            {
                string fileExtension =
                                     Path.GetExtension(postedFileBase.FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    var fileLocation = $"{Server.MapPath("~/App_Data/ExportData")}/{postedFileBase.FileName}";

                    if (System.IO.File.Exists(fileLocation))
                        System.IO.File.Delete(fileLocation);

                    // ReSharper disable once PossibleNullReferenceException
                    Request.Files["MemberExelUpload"].SaveAs(fileLocation);
                    string excelConnectionString;

                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {

                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    } //Create Connection to Excel work book and add oledb namespace

                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt;

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    DataSet ds = new DataSet();

                    string query = $"Select * from [{excelSheets[0]}]";
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        var checker = UserManager.FindByEmail(ds.Tables[0].Rows[i]["Email"].ToString());

                        if (checker==null)
                        {
                            var user = new Member
                            {
                                UserName = ds.Tables[0].Rows[i]["Email"].ToString(),
                                Email = ds.Tables[0].Rows[i]["Email"].ToString(),
                                EmailConfirmed = true,
                                PhoneNumber = ds.Tables[0].Rows[i]["PhoneNumber"].ToString(),
                                PhoneNumberConfirmed = true,
                                TwoFactorEnabled = false,
                                MemberProfile = new MemberProfile
                                {
                                    FullName = ds.Tables[0].Rows[i]["FullName"].ToString(), RegistrationDate = DateTime.Now, Address = ds.Tables[0].Rows[i]["Address"].ToString()
                                },

                            };

                            var result = UserManager.Create(user, ds.Tables[0].Rows[i]["Password"].ToString());
                            if (result.Succeeded)
                            {
                                StringWriter writer = new StringWriter();
                                HtmlTextWriter html = new HtmlTextWriter(writer);

                                html.RenderBeginTag(HtmlTextWriterTag.H1);
                                html.WriteEncodedText("User Registration Notification");
                                html.RenderEndTag();
                                html.WriteEncodedText("Dear Sir/Ma ");
                                html.WriteBreak();
                                html.RenderBeginTag(HtmlTextWriterTag.P);
                                html.WriteEncodedText("This is to notify you that you've been successfully registered on our platform. Kindly login with the details below");
                                html.RenderEndTag();
                                html.WriteBreak();
                                html.WriteBreak();
                                html.WriteEncodedText("Username : " + user.UserName);
                                html.WriteBreak();
                                html.WriteEncodedText("Password :  " + ds.Tables[0].Rows[i]["Password"]);         
                                html.WriteBreak();
                                html.WriteEncodedText("url: evaluation.rasmedpublications.com");
                                html.WriteBreak();
                                html.WriteBreak();
                                html.WriteEncodedText("Should you encounter any problem or difficulty, kindly get in touch with us via the support page");
                                html.WriteBreak();
                                html.WriteBreak();
                                html.WriteEncodedText("Regards");
                                html.WriteBreak();
                                html.WriteEncodedText("Rasmed Publications HR Team");
                                html.Flush();

                                string htmlmsg = writer.ToString();


                                
                              _mailPusher.SendEmail(user.Email, "Administrator", "sanusibs@rasmedpublications.com",
                                    "Successful User Registration", htmlmsg);

                                var newmsg = new SmsMessage
                                {
                                    Message = "You've been successfully added to Rasmed Publications Evaluation Platform. Kindly Check your E-mail for details",
                                    Number = ds.Tables[0].Rows[i]["PhoneNumber"].ToString(),
                                    SenderId = "RASMED PUB"
                                };
                                _messenger.SendMessage(newmsg);

                                //sendmail and sms

                            }
                            AddErrors(result);
                        }
                    }
                    excelConnection.Close();

                    if (System.IO.File.Exists(fileLocation))
                        System.IO.File.Delete(fileLocation);

                    TempData["notification"] = "The Members has been successfully uploaded";
                    return RedirectToAction("MemberBulkUpload");
                }
            }
            else
            {
               ModelState.AddModelError("", "Please,select a valid excel file!");
               return View();
            }

            return View();
        }


        public ActionResult UpdateMember(string id)
        {
            var entity = _examinationRepository.GetMemberProfiles.FirstOrDefault(x=>x.Member.Id==id);
            var newentity = new MemberUpdateViewModel
            {
                // ReSharper disable once PossibleNullReferenceException
                MemId = entity.Id,
                Address = entity.Address,
                FullName = entity.FullName,
                MobileNumber = entity.Member.PhoneNumber
            };

            return View(newentity);
        }


        [HttpPost]
        public ActionResult UpdateMember(MemberUpdateViewModel mp)
        {
            if (ModelState.IsValid)
            {
                var me = _examinationRepository.GetMemberProfiles.FirstOrDefault(x => x.Id == mp.MemId);
                if (me != null)
                {
                   
                    var user = UserManager.FindById(me.Member.Id);
                    if (user != null)
                    {
                        user.MemberProfile.FullName = mp.FullName;
                        user.MemberProfile.Address = mp.Address;
                        user.PhoneNumber = mp.MobileNumber;
                        user.PhoneNumberConfirmed = true;
                       
                        UserManager.Update(user);
                       TempData["notification"] = "The member has been updated successfully";
                        return RedirectToAction("Member");
                    }
                }
            }
          ModelState.AddModelError("","Kindly fill in all required information");
            return View(mp);
        }

        public ActionResult MemberDetails(string id)
        {
            var user = UserManager.FindById(id);
            ViewBag.RoleNames = UserManager.GetRoles(user.Id);
            return View(user.MemberProfile);
        }


        public ActionResult SuspendUser(string id)
        {
            var user = UserManager.FindById(id);
            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = DateTime.Now.AddYears(1);
            UserManager.Update(user);
            TempData["notification"] = "User has been successfully suspended";
            return RedirectToAction("MemberDetails", new { id });
        }
        public ActionResult UnSuspendUser(string id)
        {
            var user = UserManager.FindById(id);
            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = null;
            UserManager.Update(user);
            TempData["notification"] = "User has been successfully unsuspended";
            return RedirectToAction("MemberDetails", new { id });
        }

        public ActionResult PromoteToAdmin(string id, string returnUrl)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = UserManager.FindById(id);

            var rolesForUser = UserManager.GetRoles(user.Id);

            if (!rolesForUser.Contains("Admin"))
            {

                UserManager.AddToRole(user.Id, "Admin");

            }

            return RedirectToAction("MemberDetails", new { id });



        }

        public ActionResult RemoveFromAllRole(string id, string returnUrl)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = UserManager.FindById(id);

            var rolesForUser = UserManager.GetRoles(user.Id);

            if (rolesForUser.Contains("Admin"))
            {

                UserManager.RemoveFromRole(user.Id, "Admin");

            }

            //if (rolesForUser.Contains("SuperAdmin"))
            //{

            //    var result = UserManager.RemoveFromRole(user.Id, "Admin");

            //}

            return RedirectToAction("MemberDetails", new { id });



        }

        public ActionResult Examination(string currentFilter, string searchString, int? page)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var entity = _examinationRepository.GetExaminations.OrderByDescending(x => x.CreationDate).ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                entity = (List<Examination>) entity.Where(s => s.Name.Contains(searchString));
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(entity.ToPagedList(pageNumber, pageSize));

        }

        public ActionResult EditExam(int id)
        {
            var entity = _examinationRepository.GetExaminations.FirstOrDefault(x => x.Id == id);
            return View(entity);
        }

        [HttpPost]
        public ActionResult EditExam(Examination e)
        {
            e.SeoUrl = e.Name.ToSeoUrl();
           // e.CreationDate=DateTime.Now;
            _examinationRepository.SaveExamination(e);
            TempData["notification"] = "The Examination has been updated";
            return RedirectToAction("Examination");
        }



        public ActionResult AddExam()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddExam(Examination e)
        {
            e.SeoUrl = e.Name.ToSeoUrl();
            e.CreationDate=DateTime.Now;
            _examinationRepository.SaveExamination(e);
            TempData["notification"] = "The Examination has been added";
            return RedirectToAction("Examination");
        }


        public ActionResult ExamDetails(int id)
        {
            var entity = _examinationRepository.GetExaminations.FirstOrDefault(x => x.Id == id);
            return View(entity);
        }

        public ActionResult DeleteExam(int id)
        {
            
            _examinationRepository.DeleteExam(id);
            TempData["notification"] = "The Examination has been deleted";
            return RedirectToAction("Examination");
        }

        public ActionResult Qustion(int examId)
        {
            var allquestions = _examinationRepository.GetExamQuestions.Where(x => x.Examination.Id == examId);
            ViewBag.ExamName = _examinationRepository.GetExaminationData(examId).Name;
            ViewBag.ExamId = examId;
            return View(allquestions.ToList());
           
        }

        public ActionResult AddQuestion(int id)
        {
            ViewBag.ExamId = id;
            ViewBag.ExamName = _examinationRepository.GetExaminationData(id).Name;

            return View();

            //add question
            //add options
            //add answer
        }

        [HttpPost]
        public ActionResult AddQuestion(SingleQuestionUpload squ,HttpPostedFileBase upload)
        {
            int ex = squ.ExamId;
            if (ModelState.IsValid)
            {
                
                var kdk = new ExamQuestion
                {
                    Examination = _examinationRepository.GetExaminationData(squ.ExamId),
                    QuestionContent = squ.Question
                };

                var quest = _examinationRepository.SaveQuestion(kdk, upload);
                //add options
                var eo1 = new ExamOption
                {
                    OptionContent = squ.Option1,
                    ExamQuestion = quest
                };
                var op1=_examinationRepository.SavExamOption(eo1);

                var eo2 = new ExamOption
                {
                    OptionContent = squ.Option2,
                    ExamQuestion = quest
                };
                var op2 = _examinationRepository.SavExamOption(eo2);

                var eo3 = new ExamOption
                {
                    OptionContent = squ.Option3,
                    ExamQuestion = quest
                };
                var op3 = _examinationRepository.SavExamOption(eo3);

                var eo4 = new ExamOption
                {
                    OptionContent = squ.Option4,
                    ExamQuestion = quest
                };
                var op4 = _examinationRepository.SavExamOption(eo4);
                
                   var eo5 = new ExamOption
                    {
                        OptionContent = squ.Option5,
                        ExamQuestion = quest
                    };
                var op5 = _examinationRepository.SavExamOption(eo5);


                int value = squ.AnswerIdentifier;
                //save answer
                switch (value)
                {
                    case 1:
                        var ans1 = new ExamAnswer
                        {
                            ExamQuestion = quest,
                            ExamOption = op1
                        };
                        _examinationRepository.SaveAnswer(ans1);
                        break;


                    case 2:
                        var ans2 = new ExamAnswer
                        {
                            ExamQuestion = quest,
                            ExamOption = op2
                        };
                        _examinationRepository.SaveAnswer(ans2);
                        break;

                    case 3:
                        var ans3 = new ExamAnswer
                        {
                            ExamQuestion = quest,
                            ExamOption = op3
                        };
                        _examinationRepository.SaveAnswer(ans3);
                        break;
                    case 4:
                        var ans4 = new ExamAnswer
                        {
                            ExamQuestion = quest,
                            ExamOption = op4
                        };
                        _examinationRepository.SaveAnswer(ans4);
                        break;
                    case 5:
                        var ans5 = new ExamAnswer
                        {
                            ExamQuestion = quest,
                            ExamOption = op5
                        };
                        _examinationRepository.SaveAnswer(ans5);
                        break;


                }
            }
            TempData["notification"] = "The Question has been successfully uploaded";
            return RedirectToAction("Qustion", new { examId = ex });
        }



        public ActionResult BulkQuestionDataUpload(int id)
        {
            ViewBag.examId = id;
            ViewBag.ExamName = _examinationRepository.GetExaminationData(id).Name;
            return View();
        }



        [HttpPost]
        public ActionResult BulkQuestionDataUpload(FormCollection fc,HttpPostedFileBase questionFile)
        {
            int examId = int.Parse(fc["examId"]);
            var postedFileBase = Request.Files["questionFile"];
            if (postedFileBase != null && postedFileBase.ContentLength > 0)
            {
                string fileExtension =
                    Path.GetExtension(postedFileBase.FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    var fileLocation = $"{Server.MapPath("~/App_Data/ExportData")}/{postedFileBase.FileName}";

                    if (System.IO.File.Exists(fileLocation))
                        System.IO.File.Delete(fileLocation);

                    // ReSharper disable once PossibleNullReferenceException
                    Request.Files["questionFile"].SaveAs(fileLocation);

                    string excelConnectionString;

                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation +
                                            ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation +
                                                ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {

                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation +
                                                ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    } //Create Connection to Excel work book and add oledb namespace

                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt;

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    DataSet ds = new DataSet();

                    string query = $"Select * from [{excelSheets[0]}]";
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        var kdk = new ExamQuestion
                        {
                            Examination = _examinationRepository.GetExaminationData(examId),
                            QuestionContent = ds.Tables[0].Rows[i]["Question"].ToString()
                        };

                        var quest = _examinationRepository.SaveQuestion(kdk, null);
                        //add options
                        var eo1 = new ExamOption
                        {
                            OptionContent = ds.Tables[0].Rows[i]["Option1"].ToString(),
                            ExamQuestion = quest
                        };
                        var op1 = _examinationRepository.SavExamOption(eo1);

                        var eo2 = new ExamOption
                        {
                            OptionContent = ds.Tables[0].Rows[i]["Option2"].ToString(),
                            ExamQuestion = quest
                        };
                        var op2 = _examinationRepository.SavExamOption(eo2);

                        var eo3 = new ExamOption
                        {
                            OptionContent = ds.Tables[0].Rows[i]["Option3"].ToString(),
                            ExamQuestion = quest
                        };
                        var op3 = _examinationRepository.SavExamOption(eo3);

                        var eo4 = new ExamOption
                        {
                            OptionContent = ds.Tables[0].Rows[i]["Option4"].ToString(),
                            ExamQuestion = quest
                        };
                        var op4 = _examinationRepository.SavExamOption(eo4);

                        var eo5 = new ExamOption
                        {
                            OptionContent = ds.Tables[0].Rows[i]["Option5"].ToString(),
                            ExamQuestion = quest
                        };
                        var op5 = _examinationRepository.SavExamOption(eo5);


                        int value = int.Parse(ds.Tables[0].Rows[i]["Answer"].ToString());
                        //save answer
                        switch (value)
                        {
                            case 1:
                                var ans1 = new ExamAnswer
                                {
                                    ExamQuestion = quest,
                                    ExamOption = op1
                                };
                                _examinationRepository.SaveAnswer(ans1);
                                break;


                            case 2:
                                var ans2 = new ExamAnswer
                                {
                                    ExamQuestion = quest,
                                    ExamOption = op2
                                };
                                _examinationRepository.SaveAnswer(ans2);
                                break;

                            case 3:
                                var ans3 = new ExamAnswer
                                {
                                    ExamQuestion = quest,
                                    ExamOption = op3
                                };
                                _examinationRepository.SaveAnswer(ans3);
                                break;
                            case 4:
                                var ans4 = new ExamAnswer
                                {
                                    ExamQuestion = quest,
                                    ExamOption = op4
                                };
                                _examinationRepository.SaveAnswer(ans4);
                                break;
                            case 5:
                                var ans5 = new ExamAnswer
                                {
                                    ExamQuestion = quest,
                                    ExamOption = op5
                                };
                                _examinationRepository.SaveAnswer(ans5);
                                break;


                        }

                    }
                    excelConnection.Close();

                    if (System.IO.File.Exists(fileLocation))
                        System.IO.File.Delete(fileLocation);

                    TempData["notification"] = "The Questions,Options ans Answers have been uploaded";
                    return RedirectToAction("Qustion", new {examId});
                }

                else
                {
                    ModelState.AddModelError("", "Please,select a valid excel file!");
                    ViewBag.examId = examId;

                    return View(new {id = examId});
                }
            }

            ModelState.AddModelError("", "Please,select an excel file!");
            ViewBag.examId = examId;

            return View(new { id = examId });
        }

        public ActionResult DeleteQuestion(int id)
        {
            var exId = _examinationRepository.GetExamQuestion(id);
            int examId = exId.Examination.Id;
            _examinationRepository.DeleteQuestion(id);
            return RedirectToAction("Qustion", new {examId });
        }

        public ActionResult AddUsersToExam(int examId)
        {
            var allmembers =  UserManager.Users.OrderBy(x=>x.UserName).ToList();
            var exam = _examinationRepository.GetExaminationData(examId);
            var selecteditems = exam.Members.Select(x => x.Id).ToList();// _examinationRepository.GetMembers.Where(x => x.Examinations. == me.Country).Select(k => k.BackgroundCheckerMaster.Id).ToList();
           
            var data = new ExamCandidateSelectorVm { ExamId = examId,ExamName = exam.Name,MemberList = allmembers, SelectedUsers = selecteditems };
            return View(data);
            
        }

        [HttpPost]
        public ActionResult AddUsersToExam(FormCollection fm)
        {

            var selectedusers = fm["SelectedUsers"];
            var examId = int.Parse(fm["ExamId"]);
            var exam = _examinationRepository.GetExaminationData(examId);
            var preSelected = exam.Members.ToList();
            if (preSelected.Any())
            {

                _examinationRepository.DetachMembersFromExam(examId);
            }
            if (selectedusers.Length > 1)
            {
                _examinationRepository.AttachMembersToExam(selectedusers, examId);
            }


            TempData["notification"] = "The Users have been successfully added/detached";
            return RedirectToAction("AddUsersToExam", new {examId});

        }

       // [OutputCache(Duration = 720000, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "Id")]
        public FileContentResult GetQuestionImage(int id)
        {
            var v = _examinationRepository.GetExamQuestion(id);
            if (v != null)
            {

                byte[] byteArray = v.ImageData;
                using (var outStream = new MemoryStream())
                {
                    using (var inStream = new MemoryStream(byteArray))
                    {
                        //
                        var settings = new ResizeSettings("maxwidth=400&maxheight=100");
                        ImageBuilder.Current.Build(inStream, outStream, settings);
                        var outBytes = outStream.ToArray();
                        return new FileContentResult(outBytes, "image/jpeg");
                    }
                }
            }

            return null;
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}

