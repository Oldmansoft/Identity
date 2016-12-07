using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Util.AssemblyXml
{
    /// <summary>
    /// 注释文档读取器
    /// </summary>
    public static class SummaryReader
    {
        private static ConcurrentDictionary<string, SummaryXml> Xmls { get; set; }

        static SummaryReader()
        {
            Xmls = new ConcurrentDictionary<string, SummaryXml>();
        }

        /// <summary>
        /// 获取注释文档
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SummaryXml GetXmlInfo(Type type)
        {
            var key = type.FullName;
            SummaryXml result;
            if (Xmls.TryGetValue(key, out result)) return result;
            result = new SummaryXml(type);
            Xmls.TryAdd(key, result);
            return result;
        }

        /// <summary>
        /// 获取注释文档
        /// </summary>
        /// <param name="type"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static SummaryXml GetXmlInfo(Type type, string extension)
        {
            var key = string.Format("{0}:{1}", extension, type.FullName);
            SummaryXml result;
            if (Xmls.TryGetValue(key, out result)) return result;
            result = new SummaryXml(type);
            Xmls.TryAdd(key, result);
            return result;
        }

        /// <summary>
        /// 获取类型文档
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SummaryTypeInfo GetTypeInfo(Type type)
        {
            return GetXmlInfo(type).GetTypeInfo(type);
        }

        /// <summary>
        /// 获取类型文档
        /// </summary>
        /// <param name="type"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static SummaryTypeInfo GetTypeInfo(Type type, string extension)
        {
            return GetXmlInfo(type, extension).GetTypeInfo(type);
        }
    }
}
