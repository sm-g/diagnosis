using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Diagnosis.Common
{
    public static class ConvertExtensions
    {
        public static TDest? ConvertTo<TSource, TDest>(this TSource? source)
            where TDest : struct
            where TSource : struct
        {
            if (source == null)
            {
                return null;
            }
            return (TDest)Convert.ChangeType(source.Value, typeof(TDest));
        }

        public static T As<T>(this object obj)
        {
            if (obj == null)
                return default(T);
            return (T)obj;
        }
        public static byte[] GetBytes(this SecureString sstr)
        {
            // pointer to hold unmanaged reference to SecureString instance
            IntPtr bstr = IntPtr.Zero;
            byte[] ssBytes;
            try
            {
                // marshall SecureString into byte array
                ssBytes = new byte[sstr.Length * 2];
                bstr = Marshal.SecureStringToBSTR(sstr);
                Marshal.Copy(bstr, ssBytes, 0, ssBytes.Length);
            }
            finally
            {
                // Make sure that the clear text data is zeroed out
                Marshal.ZeroFreeBSTR(bstr);
            }
            return ssBytes;
        }

        public static string GetString(this SecureString value)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

    }
}
