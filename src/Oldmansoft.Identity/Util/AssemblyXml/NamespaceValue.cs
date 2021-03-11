using System;

namespace Oldmansoft.Identity.Util.AssemblyXml
{
    /// <summary>
    /// 命名空间与值
    /// </summary>
    public class NamespaceValue
    {
        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// 创建命名空间与值
        /// </summary>
        /// <param name="context"></param>
        public NamespaceValue(string context)
        {
            if (string.IsNullOrWhiteSpace(context)) throw new ArgumentNullException("context");

            var contexts = context.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (contexts.Length == 1)
            {
                Namespace = string.Empty;
                Value = contexts[0];
            }
            else
            {
                Namespace = string.Join(".", contexts, 0, contexts.Length - 1);
                Value = contexts[contexts.Length - 1];
            }
        }
    }
}
