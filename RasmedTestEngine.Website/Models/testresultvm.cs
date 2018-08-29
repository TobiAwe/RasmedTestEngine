namespace RasmedTestEngine.Website.Models
{
    public class Testresultvm
    {
        public string Outcome { get; set; }
        public string Remark { get; set; }

        public string Correct { get; set; }

        public string Wrong { get; set; }

        public int RemainingAttempt { get; set; }
    }
}