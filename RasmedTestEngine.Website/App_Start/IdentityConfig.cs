using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using RasmedTestEngine.Common.Email;
using RasmedTestEngine.Common.Sms;
using RasmedTestEngine.Core.Concrete;
using RasmedTestEngine.Core.Entities;
using RasmedTestEngine.Website.Models;

namespace RasmedTestEngine.Website
{
    public class EmailService : IIdentityMessageService
    {
        readonly MailPusher _mailPusher = new MailPusher();
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            var msg = new MailModel
            {
                From = "evaluation@rasmedpublications.com",
                To = message.Destination,
                Bcc = "sanusibs@rasmedpublications.com",
                Name = "Rasmed Publications",
                Message =message.Body,
                Subject = message.Subject
            };
            
            _mailPusher.SendEmail(msg.To, msg.Name, msg.Bcc, msg.Subject, msg.Message);
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        readonly SendSms _messenger = new SendSms();
        public Task SendAsync(IdentityMessage message)
        {
            var newmsg = new SmsMessage
            {
                Message = message.Body,
                Number = message.Destination,
                SenderId = "RASMED PUB"
            };
            _messenger.SendMessage(newmsg);
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<Member>
    {
        public ApplicationUserManager(IUserStore<Member> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<Member>(context.Get<EfDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<Member>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<Member>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<Member>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<Member>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<Member, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(Member user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
