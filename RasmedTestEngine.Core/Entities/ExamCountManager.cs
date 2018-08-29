using System;

namespace RasmedTestEngine.Core.Entities
{
    public class ExamCountManager
    {
        public int Id { get; set; }
        public virtual Member Member { get; set; }

        public virtual Examination Examination { get; set; }

        public DateTime AttemptDateTime { get; set; }
    }
}
