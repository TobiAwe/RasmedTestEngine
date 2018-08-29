using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RasmedTestEngine.Core.Entities;

namespace RasmedTestEngine.Core.Abstract
{
    public interface IExaminationRepository
    {
        bool OngoingTest(string mId);

        void ComputeResult(string mId, int c, int w, int u,string allWrongEntry, DateTime finishTime, string remark);

        string InitiateTest(string mId, Examination ex);

        IList<ExamQuestion> LoadQuestions(int eId);

        bool CheckedIfPassed(string mId, Examination c);

        bool CheckAnyAttempt(string mId, Examination c);

        Examination GetExaminationData(int eId);

        MemberProfile GetProfile(string username);

        ExamOption GetExamOption(int id);

        ExamQuestion GetExamQuestion(int id);

        ExamAnswer GetExamAnswer(int id);

        int NumberOfRemainingAttempt(string username,int eId);

        ExamAccessControl GetDetails(string token);

        void SaveAccessControl(ExamAccessControl eac);
        void SaveExamination(Examination ex);
        ExamQuestion  SaveQuestion(ExamQuestion eq, HttpPostedFileBase upload);
        ExamOption SavExamOption(ExamOption eo);
        void SaveAnswer(ExamAnswer ea);
        void SaveExamCount(int eId, string mId);

        void UpdateMember(int id, string fullname, string address, string phoneNumber);


        void DeleteExam(int id);
        void DeleteQuestion(int id);
        void AttachMembersToExam(string members, int examId);
        void DetachMembersFromExam(int examId);

        IQueryable<Examination> GetExaminations { get; }

        IQueryable<ExamQuestion> GetExamQuestions { get; }
        IQueryable<Member> GetMembers { get; }

        IQueryable<MemberProfile> GetMemberProfiles { get; }

        IQueryable<ResultManager> GetResultManagers { get; }

        IQueryable<ResultLog> GetResultLogs { get; }

    }
}
