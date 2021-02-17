using System;

namespace DOMSLibrary.Utility
{
    public class Generic
    {

        internal static bool IsDateTime(string txtDate)
        {
            DateTime tempDate;
            return DateTime.TryParse(txtDate, out tempDate);
        }

        internal static string FormatDigit(string strDigit)
        {
            if (strDigit.Length > 0)
                return strDigit.Length > 1 ? strDigit : "0" + strDigit;
            else
                return "00";
        }
    }
}
