using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RasmedTestEngine.Core.Entities
{
    public class Examination
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

       
        public string SeoUrl { get; set; }

        [Required]
        public string Instruction { get; set; }

        [Display(Name = "Allotted Time")]
        public int AllottedTime { get; set; }

        [Display(Name = "Max Allowed Attempt")]
        public int MaximumAttemptNumber { get; set; }

        public bool Active { get; set; }

        public DateTime CreationDate { get; set; }

        public virtual ICollection<ExamQuestion> ExamQuestions { get; set; }

        public virtual ICollection<Member> Members { get; set; } 


    }
}