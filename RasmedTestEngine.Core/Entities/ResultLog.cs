using System.ComponentModel.DataAnnotations;

namespace RasmedTestEngine.Core.Entities
{
    public class ResultLog
    {
        [Key]
        public int Id { get; set; }
        public virtual ResultManager ResultManager { get; set; }

        public int TotalCorrect { get; set; }

        public int TotalWrong { get; set; }

        public int? TotalUnanswered { get; set; }

        
        public string WrongQuestionandAnswerSelected { get; set; }
    }
}
