using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using RasmedTestEngine.Core.Abstract;
using RasmedTestEngine.Core.Entities;

namespace RasmedTestEngine.Core.Concrete
{
    public class EfExaminationRepository : IExaminationRepository
    {
        readonly EfDbContext _db = new EfDbContext();
        private readonly InMemoryCache _cacheService;

        public EfExaminationRepository(InMemoryCache cacheService)
        {
            _cacheService = cacheService;
        }
        public bool OngoingTest(string mId)
        {
            var checker1 = _db.ResultManagers.Where(x => x.MemberProfile.Member.Id == mId && x.Status == "STARTED");
            if (checker1.Any())
            {


                int num = checker1.Max(x => x.Id);
                var checker = checker1.FirstOrDefault(x => x.Id == num);

                if (checker != null)
                {
                    TimeSpan c = DateTime.Now - checker.ExpectedEndTime;
                    //if (c.TotalMinutes < checker.Examination.AllottedTime)
                    if (c.TotalMinutes < 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ComputeResult(string mId, int c, int w, int u, string allWrongEntry, DateTime finishTime, string remark)
        {
            var findtest = _db.ResultManagers.Where(x => x.MemberProfile.Member.Id == mId && x.Status == "STARTED")
                .OrderByDescending(x => x.StarTime).FirstOrDefault();
            if (findtest != null)
            {
                findtest.EndTime = finishTime;
                //findtest..TotalCorrect = c;
                //findtest..TotalWrong = w;
               // findtest.TotalUnanswered = u;
                findtest.Status = "FINISHED";
                findtest.Remark = remark;

                _db.Entry(findtest).State = EntityState.Modified;
            }
            _db.SaveChanges();

            var entity = new ResultLog
            {
                ResultManager = findtest,
                TotalCorrect = c,
                TotalUnanswered = u,
                TotalWrong = w,
                WrongQuestionandAnswerSelected = allWrongEntry
            };
            _db.ResultLogs.Add(entity);
            _db.SaveChanges();
        }

        public string InitiateTest(string mId, Examination ex)
        {
            var m = _db.Users.Find(mId);
            //var checkrunningtest = _db.ResultManagers.FirstOrDefault(x => x.MemberProfile.Id == m.MemberProfile.Id && x.Status == "STARTED");
            //if (checkrunningtest != null)
            //{
            //    TimeSpan c = DateTime.Now - checkrunningtest.ExpectedEndTime;
            //    if (c.Minutes < checkrunningtest.Examination.AllottedTime )
            //    {
            //        return "";
            //    }

               
            //}




            var newexam = new ResultManager
            {
                StarTime = DateTime.Now,
                EndTime = null,
                ExpectedEndTime = DateTime.Now + TimeSpan.FromMinutes(ex.AllottedTime),
                Status = "STARTED",
                MemberProfile = m.MemberProfile,
                Examination = ex
            };

            _db.Entry(newexam).State = EntityState.Added;

            _db.ResultManagers.Add(newexam);
            _db.SaveChanges();

           
           return newexam.ExpectedEndTime.ToString(CultureInfo.InvariantCulture);
        }

        public IList<ExamQuestion> LoadQuestions(int eId)
        {
            return _db.ExamQuestions.Include("ExamOptions").Where(x => x.Examination.Id == eId).ToList();
        }

        public bool CheckedIfPassed(string mId, Examination ex)
        {
            var m = _db.Users.Find(mId);
            var divider = ex.ExamQuestions.Count/2;
            var checker =
                _db.ResultLogs.FirstOrDefault(
                    x => x.ResultManager.MemberProfile.Id == m.MemberProfile.Id && x.ResultManager.Examination.Id == ex.Id && x.TotalCorrect >= divider);
            if (checker != null)
            {
                return true;
            }
            return false;
        }

        public bool CheckAnyAttempt(string mId, Examination ex)
        {
            var m = _db.Users.Find(mId);
            var checker =_db.ResultManagers.FirstOrDefault( x => x.MemberProfile.Id == m.MemberProfile.Id && x.Examination.Id == ex.Id);
            if (checker != null)
            {
                return true;
            }
            return false;
        }

        public Examination GetExaminationData(int eId)
        {
            var testInfo = _db.Examinations.Include("ExamQuestions").FirstOrDefault(x => x.Id == eId);
                     return testInfo;
        }

        public MemberProfile GetProfile(string username)
        {
            return _db.MemberProfiles.FirstOrDefault(x => x.Member.UserName == username);
        }

        public ExamOption GetExamOption(int id)
        {
            return _db.ExamOptions.Include("ExamQuestion").FirstOrDefault(x => x.Id == id);
        }

        public ExamQuestion GetExamQuestion(int id)
        {
            return _db.ExamQuestions.Include("ExamOptions").FirstOrDefault(x => x.Id == id);
        }

        public ExamAnswer GetExamAnswer(int id)
        {
            return _db.ExamAnswers.Include("ExamQuestion").FirstOrDefault(x => x.ExamQuestion.Id == id);
        }

        public int NumberOfRemainingAttempt(string username, int eId)
        {
           
            var exam = _db.Examinations.Find(eId);
            var totalAttempt =
                _db.ExamCountManagers.Count(x => x.Examination.Id == eId && x.Member.UserName == username);
            return exam.MaximumAttemptNumber - totalAttempt;
        }

        public ExamAccessControl GetDetails(string token)
        {
            return _db.ExamAccessControls.FirstOrDefault(x => x.AccessToken == token);
        }

        public void SaveAccessControl(ExamAccessControl eac)
        {
            if (eac.Id == 0)
            {
                _db.ExamAccessControls.Add(eac);
            }
            else
            {
                var entity = _db.ExamAccessControls.Find(eac.Id);
                if (entity != null)
                {
                    entity.Hit = eac.Hit;
                    entity.AccessToken = eac.AccessToken;
                    entity.ExpiryDate = eac.ExpiryDate;
                    entity.Examination = eac.Examination;


                }
                _db.Entry(entity).State = EntityState.Modified;

            }
            _db.SaveChanges();
        }

        public void SaveExamination(Examination ex)
        {
            if (ex.Id == 0)
            {
                _db.Examinations.Add(ex);
            }
            else
            {
                var entity = _db.Examinations.Find(ex.Id);
                if (entity != null)
                {
                    entity.MaximumAttemptNumber = ex.MaximumAttemptNumber;
                    entity.AllottedTime = ex.AllottedTime;
                    entity.ExamQuestions = ex.ExamQuestions;
                    entity.Instruction = ex.Instruction;
                    entity.Name = ex.Name;
                    entity.Active = ex.Active;
                    entity.SeoUrl = ex.SeoUrl;
                    
                }
                _db.Entry(entity).State = EntityState.Modified;
            }
            _db.SaveChanges();
        }

        public ExamQuestion SaveQuestion(ExamQuestion eq, HttpPostedFileBase upload)
        {

            if (eq.Id == 0)
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    eq.ImageData = new byte[upload.ContentLength];
                    eq.ImageMimeType = upload.ContentType;
                    upload.InputStream.Read(eq.ImageData, 0, upload.ContentLength);
                }
                _db.ExamQuestions.Add(eq);
                _db.SaveChanges();
                return eq;

            }
            var entity = _db.ExamQuestions.Find(eq.Id);
            if (entity != null)
            {
                entity.Examination = eq.Examination;
                entity.ExamAnswer = eq.ExamAnswer;
                entity.QuestionContent = eq.QuestionContent;

                if (upload != null && upload.ContentLength > 0)
                {
                    entity.ImageData = new byte[upload.ContentLength];
                    entity.ImageMimeType = upload.ContentType;
                    upload.InputStream.Read(entity.ImageData, 0, upload.ContentLength);
                }
            }
  

            _db.Entry(entity).State = EntityState.Modified;
            _db.SaveChanges();
            return entity;
        }

        public ExamOption SavExamOption(ExamOption eo)
        {
            _db.ExamOptions.Add(eo);
            _db.SaveChanges();
            return eo;
        }

        public void SaveAnswer(ExamAnswer ea)
        {
            if (ea.Id == 0)
            {
                _db.ExamAnswers.Add(ea);
            }
            else
            {
                var entity = _db.ExamAnswers.Find(ea.Id);
                if (entity != null)
                {
                    entity.ExamQuestion = ea.ExamQuestion;
                    entity.ExamOption = ea.ExamOption;
                    
                }
                _db.Entry(entity).State = EntityState.Modified;
            }
            _db.SaveChanges();
        }

        public void SaveExamCount(int eId, string mId)
        {
            var exam = _db.Examinations.Find(eId);
            var member=_db.Users.Find(mId);
            var entity = new ExamCountManager
            {
                Examination = exam,
                Member = member,
                AttemptDateTime = DateTime.Now
            };
            _db.ExamCountManagers.Add(entity);
            _db.SaveChanges();
        }

        public void UpdateMember(int id,string fullname,string address,string phoneNumber)
        {
            var entity = _db.MemberProfiles.Find(id);
            if (entity != null)
            {
                entity.FullName =fullname;
                entity.Address = address;
                entity.Member.PhoneNumber = phoneNumber;
                entity.Member.PhoneNumberConfirmed = true;

                //var user = _u
            }
           
            
            _db.SaveChanges();
        }

        public void DeleteExam(int id)
        {
            var k = _db.Examinations.Find(id);
            if (k != null)
            {
                _db.Examinations.Remove(k);
                _db.SaveChanges();
            }
        }

        public void DeleteQuestion(int id)
        {
            var answer = _db.ExamAnswers.FirstOrDefault(x => x.ExamQuestion.Id == id);
            if (answer != null)
            {
                _db.ExamAnswers.Remove(answer);
                _db.SaveChanges();
            }
            var options = _db.ExamOptions.Where(x => x.ExamQuestion.Id == id);
            if (options.Any())
            {
                foreach (var examOption in options)
                {
                    _db.ExamOptions.Remove(examOption);

                }
                _db.SaveChanges();

            }

            var quest = _db.ExamQuestions.Find(id);
            _db.ExamQuestions.Remove(quest);
            _db.SaveChanges();
           
            }
        

        public void AttachMembersToExam(string members, int examId)
        {
            var exam = _db.Examinations.Find(examId);
           // exam.Members.Clear();
            string[] strArray2 = members.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in strArray2)
            {

                var k = _db.Users.Find(str);
                if (k != null)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    _db.Users.Include("Examinations").FirstOrDefault(p => p.Id == k.Id).Examinations.Add(exam);
                    _db.SaveChanges();
                  
                }
             
            }
        }

        public void DetachMembersFromExam(int examId)
        {
            var exam = _db.Examinations.Find(examId);
            exam.Members.Clear();
            _db.SaveChanges();
        }

        public IQueryable<Examination> GetExaminations => _db.Examinations.Include("ExamQuestions");
        public IQueryable<ExamQuestion> GetExamQuestions => _db.ExamQuestions.Include("ExamOptions");


        //public IQueryable<Member> GetMembers  => _cacheService.GetOrSet("GetMembers", () => _db.Users);
        public IQueryable<Member> GetMembers => _db.Users;
        public IQueryable<MemberProfile> GetMemberProfiles =>  _db.MemberProfiles;

        public IQueryable<ResultManager> GetResultManagers => _db.ResultManagers;
        public IQueryable<ResultLog> GetResultLogs => _db.ResultLogs;
    }
}
