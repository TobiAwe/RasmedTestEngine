using System.ComponentModel.DataAnnotations;

namespace RasmedTestEngine.Core.Entities
{
    public class ExamAnswer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual ExamOption ExamOption { get; set; }

        [Required]
        public virtual ExamQuestion ExamQuestion { get; set; }
    }
}