using System;
using System.Globalization;

namespace RasmedTestEngine.Common
{
    public class NumGen
    {
        public string Generate()
        {
           return DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + Guid.NewGuid().ToString().GetHashCode().ToString("x");   
        }

       
    }
}
