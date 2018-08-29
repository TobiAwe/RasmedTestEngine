using System;
using System.Web.Mvc;
using RasmedTestEngine.Common.Email;
using RasmedTestEngine.Website.Models;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;

namespace RasmedTestEngine.Website.Controllers
{
    public class SupportController : Controller
    {
        readonly MailPusher _mailPusher=new MailPusher();
        // GET: Support
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Support s)
        {
            RecaptchaVerificationHelper recaptchaHelper = this.GetRecaptchaVerificationHelper();
            if (String.IsNullOrEmpty(recaptchaHelper.Response))
            {
                ModelState.AddModelError("", "Captcha answer cannot be empty.");
                return View(s);
            }
            RecaptchaVerificationResult recaptchaResult = recaptchaHelper.VerifyRecaptchaResponse();
            if (recaptchaResult != RecaptchaVerificationResult.Success)
            {
                ModelState.AddModelError("", "Incorrect captcha answer.");
                return View(s);
            }
            if (ModelState.IsValid)
            {
                _mailPusher.SendEmail("hr@rasmedpublications.com", "Administrator","sanusibs@rasmedpublications.com", s.Subject +" "+ s.Name + "[" + s.MobileNumber + "]", s.Message);
                TempData["msg"] = "Your Message has been Successfully Sent";
                return View();
            }
            ModelState.AddModelError("", "Kindly ensure you supply all necessary and required details");
            return View(s);
        }

    }
}