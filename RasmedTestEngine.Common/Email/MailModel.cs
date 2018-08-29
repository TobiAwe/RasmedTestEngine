namespace RasmedTestEngine.Common.Email
{
    public class MailModel
    {
        public string Name { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public string Subject { get; set; }

        public string Bcc { get; set; }

        public string Message { get; set; }
    }
}