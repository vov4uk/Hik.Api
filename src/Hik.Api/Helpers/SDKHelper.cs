using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Hik.Api.Helpers
{
    internal static class SdkHelper
    {
        internal static T InvokeSDK<T>(Expression<Func<T>> func, bool throwException = true)
        {
            T result = func.Compile().Invoke();

            switch (result)
            {
                case int val when val < 0:
                case long longVal when longVal < 0:
                case bool def when !def:
                    {
                        if (throwException)
                        {
                            throw CreateException(func.ToString());
                        }
                        return result;
                    }
                default: return result;
            }
        }

        private static HikException CreateException(string method)
        {
            HikError lastErrorCode = NET_DVR_GetLastError();

            string msg = GetEnumDescription(lastErrorCode);

            return new HikException(method, msg);
        }

        private static string GetEnumDescription(HikError value)
        {
            string val = value.ToString();
            FieldInfo fi = value.GetType().GetField(val);

            if (fi != null && fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
            {
                return attributes.First().Description;
            }

            return val;
        }

        [DllImport(HikApi.HCNetSDK)]
        private static extern HikError NET_DVR_GetLastError();
    }
}
