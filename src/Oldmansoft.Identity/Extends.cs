using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class Extends
    {
        /// <summary>
        /// 获取 SHA256 散列
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] GetSHA256Hash(this byte[] source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return new System.Security.Cryptography.SHA256CryptoServiceProvider().ComputeHash(source);
        }

        /// <summary>
        /// 获取 SHA256 十六进制散列
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetSHA256Hash(this string source)
        {
            if (source == null) throw new ArgumentNullException("source");
            var input = Encoding.UTF8.GetBytes(source);
            var output = input.GetSHA256Hash();
            return BitConverter.ToString(output).Replace("-", "");
        }

        /// <summary>
        /// 创建密码散列
        /// </summary>
        /// <param name="source"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string CreatePasswordSHA256Hash(this string source, string password)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (password == null) throw new ArgumentNullException("password");
            return (source.ToLower() + password).GetSHA256Hash();
        }

        /// <summary>
        /// 获取全名
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static string GetFullName(this Type source)
        {
            return source.FullName.Replace("+", ".");
        }
    }
}
