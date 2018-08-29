using System;
using System.ComponentModel.DataAnnotations;

namespace RasmedTestEngine.Core.Entities
{
    public class ResultManager
    {
        [Key]
        public int Id { get; set; }

        public DateTime StarTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime ExpectedEndTime { get; set; }


        public string Status { get; set; } //Started,Finished

        public string Remark { get; set; } //Passed.Failed,MISCONDUCT

        [Key]
        public virtual MemberProfile MemberProfile { get; set; }

        [Key]
        public virtual Examination Examination { get; set; }
    }
}