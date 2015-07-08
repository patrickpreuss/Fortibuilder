using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Fortibuilder.guts
{
    class Tools
    {
        public String ParseIPList(string fromIP, string toIP){
            var IPregex = new Regex(@"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$");

            var IPfromuser = IPregex.Match(fromIP);
            var IPtouser = IPregex.Match(toIP);

            if ((IPfromuser.Success) || (IPtouser.Success))
            {
                return fromIP;
            }
            else
            {
              //  ShowError();
              //  return String.Format("Check your addresses.");
                return IPtouser.ToString();
            }
        }

        public static bool Isanip(string input)
        {
            var IPregex = new Regex(@"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$");
            var isanIpMatch = IPregex.Match(input);
            
            if (isanIpMatch.Success)
            {
                return true;
            }
                return false;
        }

        public static string IpRange(string ip, string subnet)
        {
            return ip;
        }


        public static bool Isanint(string input)
        {
            var unsignedinteger = new Regex(@"^\d*$");
            var isanInt = unsignedinteger.Match(input);

            return isanInt.Success;

        }

        public static int CountLinesInFile(string f)
        {
            var count = 0;

            using (var r = new StreamReader(f))
            {
                while (r.ReadLine() != null)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
