using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RasmedTestEngine.Core.Entities
{
    public class ExamQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Question")]
        public string QuestionContent { get; set; }

        public byte[] ImageData { get; set; }
        public string ImageMimeType { get; set; }

        public virtual ICollection<ExamOption> ExamOptions { get; set; }
        public virtual ExamAnswer ExamAnswer { get; set; }
        public virtual Examination Examination { get; set; }
    }
}