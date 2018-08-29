using System.Collections.Generic;
using RasmedTestEngine.Core.Entities;

namespace RasmedTestEngine.Website.Models
{
    public class ExamCandidateSelectorVm
    {
     
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public List<string> SelectedUsers { get; set; }
        public virtual List<Member> MemberList { get; set; }

    }
}