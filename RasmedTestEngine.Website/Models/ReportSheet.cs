using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RasmedTestEngine.Core.Entities;

namespace RasmedTestEngine.Website.Models
{
   
    public class ReportSheet
    {
        public int TotalAttempts { get; set; }

        public string ExamName { get; set; }
        public IList<ResultManager> ResultList { get; set; }

        
    }

   

  
}
