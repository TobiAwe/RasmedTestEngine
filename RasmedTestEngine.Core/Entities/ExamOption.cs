using System.ComponentModel.DataAnnotations;

namespace RasmedTestEngine.Core.Entities
{
    public class ExamOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OptionContent { get; set; }

        [Required]
        public virtual ExamQuestion ExamQuestion { get; set; }
    }
}