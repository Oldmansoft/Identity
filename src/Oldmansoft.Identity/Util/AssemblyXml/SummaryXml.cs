using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Oldmansoft.Identity.Util.AssemblyXml
{
    /// <summary>
    /// 注释文档
    /// </summary>
    public class SummaryXml
    {
        private Dictionary<string, SummaryTypeInfo> Members { get; set; }

        /// <summary>
        /// 创建注释文档
        /// </summary>
        /// <param name="type">类型</param>
        public SummaryXml(Type type)
        {
            var members = new Dictionary<string, SummaryTypeInfo>();
            var path = System.IO.Path.ChangeExtension(System.Reflection.Assembly.GetAssembly(type).CodeBase, "xml");
            CreateSummary(members, path);
            Members = members;
        }

        /// <summary>
        /// 创建注释文档
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="extension">扩名展</param>
        public SummaryXml(Type type, string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) throw new ArgumentNullException(extension);

            var members = new Dictionary<string, SummaryTypeInfo>();
            var path = System.IO.Path.ChangeExtension(System.Reflection.Assembly.GetAssembly(type).CodeBase, string.Format("xml.{0}", extension));
            CreateSummary(members, path);
            Members = members;
        }

        private static void CreateSummary(Dictionary<string, SummaryTypeInfo> members, string file)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(file);
            }
            catch (Exception)
            {
                return;
            }
            var root = document.DocumentElement;
            if (root == null || root.Name != "doc") return;
            var node = root.SelectSingleNode("assembly/name");
            var nodes = root.SelectNodes("members/member");
            if (node == null || nodes == null) return;

            foreach (XmlElement item in nodes)
            {
                string[] name = item.GetAttribute("name").Split(':');
                switch (name[0])
                {
                    case "T":
                        members.Add(name[1], new SummaryTypeInfo(name[1]));
                        var tSummary = item.SelectSingleNode("summary");
                        if (tSummary == null) break;
                        members[name[1]].AddMember(name[1], tSummary.InnerText.Trim());
                        break;
                    case "P":
                    case "F":
                        var value = new NamespaceValue(name[1]);
                        var summary = item.SelectSingleNode("summary");
                        if (summary == null || !members.ContainsKey(value.Namespace)) break;
                        members[value.Namespace].AddMember(value.Value, summary.InnerText.Trim());
                        break;
                }
            }
        }

        /// <summary>
        /// 获取类型文档
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal SummaryTypeInfo GetTypeInfo(Type type)
        {
            string name = type.FullName;
            if (Members.ContainsKey(name))
            {
                return Members[name];
            }
            else
            {
                return new SummaryTypeInfo(name);
            }
        }
    }
}
