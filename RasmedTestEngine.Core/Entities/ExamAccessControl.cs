using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasmedTestEngine.Core.Entities
{
    public class ExamAccessControl
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }

        public DateTime ExpiryDate { get; set; }

        public int Hit { get; set; }
        [Required]
        public virtual Examination Examination { get; set; }
    }
}
