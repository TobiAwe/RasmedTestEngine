using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using ImageResizer;
using RasmedTestEngine.Common.Email;
using RasmedTestEngine.Core.Abstract;
using RasmedTestEngine.Website.Models;

namespace RasmedTestEngine.Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly IExaminationRepository _examinationRepository;
       // readonly SendSms _messenger = new SendSms();
        readonly MailPusher _mailPusher = new MailPusher();

        //private readonly EfDbContext _context = new EfDbContext();
        public HomeController(IExaminationRepository examinationRepository)
        {
            _examinationRepository = examinationRepository;
        }

        public ActionResult Index()
        {
            var allActiveTest = _examinationRepository.GetExaminations.Where(x => x.Active).OrderByDescending(x => x.CreationDate).ToList();
            if (Request.IsAuthenticated && allActiveTest.Any())
            {
                var me = _examinationRepository.GetProfile(User.Identity.Name);
                allActiveTest =
                    allActiveTest.Where(x => x.Members.Contains(me.Member)).ToList();
                return View(allActiveTest);

            }
            return View(allActiveTest);
        }

        [Authorize]
        [MyExpirePageActionFilter]
        public ActionResult GetInstruction(int examId)
        {
            var me = _examinationRepository.GetProfile(User.Identity.Name);
            var checker = _examinationRepository.GetExaminationData(examId);
            if (checker.Members.Contains(me.Member) && checker.Active)
            {
                return View(checker);
            }
            return View("InvalidAccess");
        }

       

        //GetQuestionImage
        [OutputCache(Duration = 720000, Location = OutputCacheLocation.ServerAndClient, VaryByParam = "Id")]
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

        [MyExpirePageActionFilter]
        public ActionResult StartTest(int examId)
        {
            var me = _examinationRepository.GetProfile(User.Identity.Name);
            var checker = _examinationRepository.GetExaminationData(examId);
            if (checker.Members.Contains(me.Member) && checker.Active)
            {
                int count = _examinationRepository.NumberOfRemainingAttempt(User.Identity.Name, checker.Id);
                if (count < 1)
                {
                    return View("AttemptExhausted");
                }
                if (_examinationRepository.CheckedIfPassed(me.Member.Id,checker))
                {
                    //tell user he has passed and do not need to take test
                    return View("AlreadyPassed");
                }
                if (_examinationRepository.OngoingTest(me.Member.Id))
                {
                    //tell him he's presently taking a test he has not finished or it's less than 24hours he took a failed test
                    return View("OnGoingTest");
                }
                _examinationRepository.SaveExamCount(checker.Id,me.Member.Id);

                var endTime = _examinationRepository.InitiateTest(me.Member.Id, checker);

                TimeSpan diff = DateTime.Parse(endTime) - DateTime.Now;
                ViewBag.EndtimeSeconds = Convert.ToInt32(diff.TotalSeconds);
                ViewBag.EndtimeMilliSeconds = Convert.ToInt32(diff.TotalMilliseconds);
                ViewBag.EndtimeMinutes = Convert.ToInt32(diff.TotalMinutes) - 1;
                ViewBag.ExaminationId = checker.Id;
                var allQuestion = _examinationRepository.LoadQuestions(checker.Id);
                return View(allQuestion);
            }
            return View("InvalidAccess");
        }

        public ActionResult SubmitTest(FormCollection collection)
        {
            int correct = 0;
            int wrong = 0;
            const int unAnwsered = 0;
            string wrongAnwsers = "";


            var username = User.Identity.Name;
            var mp = _examinationRepository.GetProfile(username);

            foreach (var key in collection.Keys)
            {
                if ((string)key != "TotalCount" && (string)key != "ExaminationId")
                {
                    
                    int value = int.Parse(collection[key.ToString()]);
                    //int realvalue = int.Parse(collection[key..ToString()]);

                    var op = _examinationRepository.GetExamOption(value);

                    //  var checker = _examinationRepository.GetExamAnswer(op.ExamQuestion.Id);
                    // _db.TestOptions.Include("TestQuestion").FirstOrDefault(x => x.Id == value);
                    //get Question
                    var question = _examinationRepository.GetExamQuestion(op.ExamQuestion.Id);
                    //_db.TestQuestions.Include("TestAnswer").FirstOrDefault(x => x.Id == checker.TestQuestion.Id);

                    if (question.ExamAnswer.ExamOption.Id == op.Id)
                    {
                        correct++;
                    }
                    else
                    {
                        wrong++;
                        wrongAnwsers = wrongAnwsers + Environment.NewLine + question.QuestionContent + "(" + op.OptionContent + ")" +
                                       Environment.NewLine;
                        //add the question plus the selected anser
                    }
                }
            }
            string modifiedwrongAnwsers = wrongAnwsers
                                    .Replace(Environment.NewLine, "<br />")
                                    .Replace("\r", "<br />")
                                    .Replace("\n", "<br />");

            var total = collection["TotalCount"];
            int ide = int.Parse(collection["ExaminationId"]);
            var examname = _examinationRepository.GetExaminationData(ide);


            double calculate = Convert.ToDouble(correct) / Convert.ToDouble(total) * 100;
            // var rem="The PassMark is 50 percent, and you scored  " + calculate + " %";
            var rem = "You scored  " + Math.Round(Convert.ToDecimal(calculate), 2)+ " %";
            string stat;
            string storeStat;
            if (calculate >= 50)
            {
                stat = "Congratulations,youv passed our Evaluation.The Human Resource Team will get in touch with you shortly";
                //storeStat = "Passed";
                storeStat = calculate.ToString(CultureInfo.InvariantCulture);
                //send a mail to admin

                //create the message here
     
                var writer = new StringWriter();
                var html = new HtmlTextWriter(writer);

                html.RenderBeginTag(HtmlTextWriterTag.H1);
                html.WriteEncodedText("Evaluation Result");
                html.RenderEndTag();
                html.WriteEncodedText("Dear Admin ");
                html.WriteBreak();
                html.RenderBeginTag(HtmlTextWriterTag.P);
                html.WriteEncodedText("THIS IS TO INFORM YOU THAT A USER HAS TAKEN A TEST AND PASSED.FIND THE DETAILS BELOW :");
                html.WriteBreak();
                html.WriteEncodedText("USERNAME : " + mp.Member.UserName);
                html.WriteBreak();
                html.WriteEncodedText("FULL NAME : " + mp.FullName);
                html.WriteBreak();
                html.WriteEncodedText("TEST NAME : " + examname.Name);
                html.WriteBreak();
                html.WriteEncodedText("SCORE : " + calculate);
                html.WriteBreak();
                html.WriteEncodedText("Wrong Answers : " + modifiedwrongAnwsers);
                html.WriteBreak();
                html.WriteBreak();
                html.WriteBreak();
                html.WriteEncodedText("REGARDS");
                html.WriteBreak();
                html.WriteEncodedText("RASMED PUBLICATIONS HR TEAM ");
                html.WriteBreak();

                html.RenderEndTag();
                html.Flush();

                string htmlString = writer.ToString();

                var msg = new MailModel
                {
                    From = "evaluation@rasmedpublications.com",
                    To = "hr@rasmedpublications.com",
                    Bcc = "sanusibs@rasmedpublications.com",
                    Name = "Administrator",
                    Message = htmlString,
                    Subject = "Rasmed Publications Evaluation Result"
                };
            
                _mailPusher.SendEmail(msg.To,msg.Name,msg.Bcc,msg.Subject,msg.Message);
              




            }
            else
            {


                stat = "Sorry, You failed.Please check back in the next 24 hours for a retake if the exam permits multiple attempst";
             
                storeStat = calculate.ToString(CultureInfo.InvariantCulture);
            }

            _examinationRepository.ComputeResult(mp.Member.Id, correct, wrong, unAnwsered, modifiedwrongAnwsers, DateTime.Now,
                storeStat);
           var test = new Testresultvm
            {
                Correct = correct.ToString(),
                Wrong = wrong.ToString(),
                Outcome = rem,
                Remark = stat,
                RemainingAttempt =_examinationRepository.NumberOfRemainingAttempt(mp.Member.UserName, examname.Id)
            };


            return RedirectToAction("TestResult", test);
        }

        [MyExpirePageActionFilter]
        public ActionResult TestResult(Testresultvm testresultvm)
        {

            return View(testresultvm);
        }




    }
}