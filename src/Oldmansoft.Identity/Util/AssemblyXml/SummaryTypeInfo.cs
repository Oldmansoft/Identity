using System.Collections.Generic;

namespace Oldmansoft.Identity.Util.AssemblyXml
{
    /// <summary>
    /// 类型文档
    /// </summary>
    public class SummaryTypeInfo : IEnumerable<KeyValuePair<string, string>>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 成员
        /// </summary>
        private Dictionary<string, string> Members { get; set; }

        /// <summary>
        /// 创建类型文档
        /// </summary>
        /// <param name="name"></param>
        public SummaryTypeInfo(string name)
        {
            Name = name;
            Members = new Dictionary<string, string>();
        }

        /// <summary>
        /// 添加成员
        /// </summary>
        /// <param name="member"></param>
        /// <param name="summary"></param>
        public void AddMember(string member, string summary)
        {
            Members.Add(member, summary);
        }

        /// <summary>
        /// 获取注释
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public string GetSummary(string member)
        {
            if (Members.ContainsKey(member))
            {
                return Members[member];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 返回循环访问的枚举数
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Members.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Members.GetEnumerator();
        }
    }
}
