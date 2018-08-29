using System.Threading;

namespace RasmedTestEngine.Common.Sms
{
    public class SendSms
    {
        
       
        public void SendMessage(SmsMessage msg)
        {
            SmsLiveProvider slp = new SmsLiveProvider();
           
            ThreadPool.QueueUserWorkItem(stateObject => slp.SendMsg(msg));

        }

        
       

    }
}
