using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System;

namespace Hik.Api.Helpers
{
    internal static class PlayHelper
    {
        internal static T Invoke<T>(int port, Expression<Func<T>> func)
        {
            T result = func.Compile().Invoke();

            switch (result)
            {
                case int val when val < 0:
                case bool def when !def:
                    {
                        throw CreateException(port, func.ToString());
                    }
                default: return result;
            }
        }

        private static HikException CreateException(int port, string method)
        {
            uint lastErrorCode = PlayM4_GetLastError(port);
            return new HikException(method, lastErrorCode);
        }

        [DllImport(HikApi.PlayCtrl)]
        public static extern uint PlayM4_GetLastError(int nPort);
    }
}
