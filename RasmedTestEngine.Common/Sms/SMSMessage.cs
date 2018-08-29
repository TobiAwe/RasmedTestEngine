using System;

namespace RasmedTestEngine.Common.Sms
{
    [Serializable]
    public class SmsMessage
    {
        public string Number { get; set; }

        public string SenderId { get; set; }

        public string Message { get; set; }
    }
}
