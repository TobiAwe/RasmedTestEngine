using RasmedTestEngine.Common.SmsLive247;

namespace RasmedTestEngine.Common.Sms
{
    public class SmsLiveProvider
    {

        public string SiteToken => "";

      
        public void SendMsg(SmsMessage smsMessage)
        {
            var sms = new SMSSiteAdminProxySoapClient();
            var newSms = new MessageInfo
            {
                CallBack = smsMessage.SenderId,
                Destination = new ArrayOfString {smsMessage.Number},
                DeliveryEmail = "me@email.com",
                Message = smsMessage.Message,
                MessageType = SMSTypeEnum.TEXT
            };


            var response = sms.SendSMS(SiteToken, newSms);

            if (response.ErrorCode != 0)
            {
                //reschedule in 2min
              
            }


        }
    }
}
