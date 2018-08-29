using System.ComponentModel.DataAnnotations;

namespace RasmedTestEngine.Website.Models
{
   
    public class SingleQuestionUpload
    {
        public int ExamId { get; set; }

        [Required]
        public string Question { get; set; }

        public byte[] ImageData { get; set; }
        public string ImageMimeType { get; set; }

        [Required]
        public string Option1 { get; set; }
        [Required]
        public string Option2 { get; set; }
        [Required]
        public string Option3 { get; set; }
        [Required]
        public string Option4 { get; set; }
        [Required]
        public string Option5 { get; set; }

        [Required]
        public  int AnswerIdentifier { get; set; }
    }

   

  
}
