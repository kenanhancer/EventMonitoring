using System;

namespace MonitoringLib
{
    public static class Extensions
    {
        public static string ValueOrNull(this string obj)
        {
            if (String.IsNullOrEmpty(obj))
                return null;
            return obj;
        }
    }
}