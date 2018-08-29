using System.Text;
using System.Web;
using System.Web.Security;

namespace RasmedTestEngine.Common.Crypto
{
    public class FrontendCryptoEngine
    {
        public static string Protect(string text, string purpose)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            byte[] stream = Encoding.UTF8.GetBytes(text);
            byte[] encodedValue = MachineKey.Protect(stream, purpose);
            return HttpServerUtility.UrlTokenEncode(encodedValue);
        }

        public static string Unprotect(string text, string purpose)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            byte[] stream = HttpServerUtility.UrlTokenDecode(text);
            if (stream != null)
            {
                byte[] decodedValue = MachineKey.Unprotect(stream, purpose);
                if (decodedValue != null) return Encoding.UTF8.GetString(decodedValue);
            }
            return null;
        }
    }
}